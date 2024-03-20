using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class PokemonController : MonoBehaviour {

    //---Static
    public static PokemonController Instance { get; private set; }
    private static BaseSettings Settings => GlobalManager.Instance.settings;

    private static readonly int ParamHappy = Animator.StringToHash("happy");
    private static readonly int ParamSleep = Animator.StringToHash("sleep");
    private static readonly int ParamEating = Animator.StringToHash("eating");
    private static readonly int ParamIdle = Animator.StringToHash("idle");
    private static readonly int ParamEmote = Animator.StringToHash("emote");

    private static readonly int ParamBaseMapOffset = Shader.PropertyToID("_BaseMapOffset");
    private static readonly int ParamOcclusionMapOffset = Shader.PropertyToID("_OcclusionMapOffset");
    private static readonly int ParamNormalMapOffset = Shader.PropertyToID("_NormalMapOffset");

    //---Properties
    public int CurrentMaterial { get; private set; }

    //---Serialized Variables
    [SerializeField] private SkinnedMeshRenderer eyeRenderer, mouthRenderer;
    [SerializeField] private SkinnedMeshRenderer[] irisRenderers;
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource sfx;
    [SerializeField] private Quaternion neckOffset = Quaternion.identity;
    [SerializeField] private Transform[] neckTransforms;

    [SerializeField] private EyeState petEyeState;
    [SerializeField] private MouthState petMouthState;
    [SerializeField] private MouthState talkMouthState;

    [SerializeField] private KeyState[] keyStates;

    //---Private Variables
    private MaterialPropertyBlock eyeBlock, mouthBlock;
    private MaterialPropertyBlock[] irisBlocks;
    private ShinyMaterialSwapper[] materialSwaps;

    private Quaternion[] previousRotations;

    private PokePuff targetPokePuff;
    private float happiness;
    private int queuedParticles;
    private bool headLock, beingPet;

    public Transform petTarget;
    public float timeSinceLastInteraction;


    public float headLockTransition, petTimer, randomLookTimer, particleSpawnTimer;
    public Vector3? randomLookPosition;

    private ParticleSystem.EmissionModule heartsEmission;

    private Vector3 startPosition;
    private float talkTimer, talkStartTimer, talkStartCooldown;

    private KeyCode? heldCode;


    public void OnValidate() {
        this.SetIfNull(ref sfx, Utils.GetComponentSearch.Children);
        this.SetIfNull(ref animator, Utils.GetComponentSearch.Children);
    }

    public void Start() {
        Instance = this;
        startPosition = transform.localPosition;

        previousRotations = new Quaternion[neckTransforms.Length];
        for (int i = 0; i < neckTransforms.Length; i++) {
            previousRotations[i] = neckTransforms[i].rotation;
        }
        heartsEmission = GlobalManager.Instance.hearts.emission;

        // Create material blocks
        eyeRenderer.GetPropertyBlock(eyeBlock = new MaterialPropertyBlock());
        mouthRenderer.GetPropertyBlock(mouthBlock = new MaterialPropertyBlock());

        irisBlocks = new MaterialPropertyBlock[irisRenderers.Length];
        for (int i = 0; i < irisRenderers.Length; i++) {
            irisRenderers[i].GetPropertyBlock(irisBlocks[i] = new MaterialPropertyBlock());
        }

        materialSwaps = FindObjectsOfType<ShinyMaterialSwapper>(true);

        // Start corotuines
        StartCoroutine(CheckForPokepuffs());
        StartCoroutine(AttemptIdleAnimation());
    }

    public void Update() {
        EyeState eyeState = null;
        MouthState mouthState = null;

        if (IsIdle()) {
            if (Input.GetMouseButton(0)) {
                // Left click to look around
                randomLookPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 6.5f));
                randomLookTimer = 20;

            } else if (Input.GetMouseButtonUp(0)) {
                // Left click released
                randomLookPosition = Camera.main.transform.position;

            } else {
                // Increase interaction timer
                timeSinceLastInteraction += Time.deltaTime;
            }

            if (MicInput.MicLoudness > Settings.microphoneSettings.minMicAmplitude) {
                if (talkTimer <= 0 && talkStartCooldown <= 0) {
                    talkStartTimer = Settings.microphoneSettings.talkStartJumpDuration;
                }

                talkTimer = Settings.microphoneSettings.talkDuration;
                talkStartCooldown = Settings.microphoneSettings.talkStartJumpCooldown;
                timeSinceLastInteraction = 0;
            }

            foreach (KeyState ks in keyStates) {

                if (Input.GetKeyDown(ks.key)) {
                    if (heldCode == ks.key) {
                        heldCode = null;
                        break;
                    }

                    heldCode = ks.key;
                }

                if (heldCode != ks.key) {
                    continue;
                }

                if (ks.mouthEnable) {
                    mouthState = ks.mouth;
                }

                if (ks.eyeEnable) {
                    eyeState = ks.eye;
                }
            }

            if ((talkTimer -= Time.deltaTime) > 0) {
                mouthState = talkMouthState;
            }
        }

        happiness = Mathf.Max(0, happiness - (Time.deltaTime / 60f * Mathf.Log(happiness + 1)));
        heartsEmission.rateOverTime = Mathf.Max(0, Mathf.Min(8, happiness / 80f));

        if ((randomLookTimer -= Time.deltaTime) <= 0) {
            randomLookTimer = Random.Range(1f, 3f);
            if (Random.value < 0.4f) {
                randomLookPosition = Camera.main.transform.position;
                randomLookTimer += 20f;
            } else {
                Vector3 target = neckTransforms[0].position + (transform.forward * 3f);
                target.y += Random.Range(-1.1f, 1.1f);
                target.z += Random.Range(-2f, 2f);
                randomLookPosition = target;
            }
        }

        beingPet = IsIdle() && !targetPokePuff && petTimer > 0;
        if (beingPet) {
            GameObject petHand = GlobalManager.Instance.petHand;

            animator.ResetTrigger(ParamIdle);
            petHand.SetActive(true);
            eyeState = petEyeState;
            mouthState = petMouthState;
            float petDuration = Settings.pokemonSettings.petDuration;
            petTimer = Mathf.Max(0, petTimer - Time.deltaTime);
            if (petTimer % petDuration <= Mathf.Max(0, petTimer - Time.deltaTime) % petDuration) {
                happiness += Settings.pokemonSettings.petHappiness;
                queuedParticles += (int) (Settings.pokemonSettings.petHappiness / 2);
            }
            if (petTimer <= 0) {
                beingPet = false;
                animator.Play("Pet_agree");
                petHand.SetActive(false);
            }
        }
        SetAnimatorStates();

        if (queuedParticles > 0 && (particleSpawnTimer -= Time.deltaTime) < 0) {
            string clipName = animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
            if (clipName == "Pet_agree" || clipName == "Pet_very_happy" || beingPet) {
                GlobalManager.Instance.hearts.Emit(1);
                queuedParticles--;
                particleSpawnTimer = 0.11f;
            }
        }

        SetStateThisFrame(eyeRenderer, eyeBlock, eyeState);
        SetStateThisFrame(mouthRenderer, mouthBlock, mouthState);
        for (int i = 0; i < irisRenderers.Length; i++) {
            irisRenderers[i].enabled = eyeState == null || eyeState.showIris;
            SetStateThisFrame(irisRenderers[i], irisBlocks[i], eyeState);
        }
    }

    public void LateUpdate() {
        LookAtLocation(FindBestLookTarget());
        headLockTransition = Mathf.Max(0, headLockTransition - Time.deltaTime);

        Vector3 position = startPosition;
        if ((talkStartTimer -= Time.deltaTime) > 0) {
            float timer = talkStartTimer / Settings.microphoneSettings.talkStartJumpDuration;
            position += Settings.microphoneSettings.talkStartJumpHeight * Mathf.Sin(timer * Mathf.PI) * Vector3.up;
        }

        talkStartCooldown -= Time.deltaTime;
        transform.localPosition = position;
    }

    #region Coroutines
    private IEnumerator CheckForPokepuffs() {
        WaitForSeconds waitOneSecond = new(1);

        while (true) {
            if (!targetPokePuff && !beingPet) {
                animator.ResetTrigger(ParamIdle);

                PokePuff[] pokePuffs = FindObjectsOfType<PokePuff>();
                if (pokePuffs.Length > 0) {
                    Array.Sort(pokePuffs, ComparePokepuffs);
                    targetPokePuff = pokePuffs[0];
                    timeSinceLastInteraction = 0f;
                }
            }
            yield return waitOneSecond;
        }
    }

    private IEnumerator AttemptIdleAnimation() {
        WaitForSeconds waitFiveSeconds = new(5);

        while (true) {
            if (timeSinceLastInteraction > Settings.pokemonSettings.sleepThresholdInSeconds/4
                && timeSinceLastInteraction < Settings.pokemonSettings.sleepThresholdInSeconds
                && Random.value < 0.3f) {

                animator.SetTrigger(ParamIdle);
            }

            yield return waitFiveSeconds;
        }
    }

    #endregion

    #region Animation Stuffs
    private void LookAtLocation(Vector3? target) {
        float time = 4f * Time.deltaTime;

        for (int i = 0; i < neckTransforms.Length; i++) {
            Transform t = neckTransforms[i];

            Quaternion newRotation;
            if (target == null) {
                // Previous to current
                newRotation = headLockTransition > 0 ? Quaternion.Slerp(previousRotations[i], t.rotation, 1f - (headLockTransition / (1 / 7f))) : t.rotation;
            } else {
                // Current to target
                Vector3 forward = target.Value - t.position;
                Quaternion targetRot = Quaternion.LookRotation(forward, Vector3.forward) * neckOffset;
                newRotation = Quaternion.Slerp(previousRotations[i], targetRot, time);
            }

            t.rotation = previousRotations[i] = newRotation;
        }
    }

    private Vector3? FindBestLookTarget() {
        if (headLock) {
            if (headLockTransition > 0) {
                return null;
            }

            return null;
        }

        if (targetPokePuff) {
            return targetPokePuff.transform.position;
        }

        if (beingPet) {
            return petTarget.position;
        }

        return randomLookPosition;
    }

    private void SetAnimatorStates() {
        animator.SetBool(ParamHappy, happiness > Settings.pokemonSettings.happinessThreshold);
        animator.SetBool(ParamSleep, Settings.pokemonSettings.sleepThresholdInSeconds > 0 && timeSinceLastInteraction > Settings.pokemonSettings.sleepThresholdInSeconds);
        animator.SetBool(ParamEating, targetPokePuff != null);
    }

    private bool IsIdle() {
        return animator.GetCurrentAnimatorStateInfo(0).IsName("Pet_idle");
    }

    private static void SetStateThisFrame(Renderer renderer, MaterialPropertyBlock block, State state) {
        if (state == null) {
            renderer.SetPropertyBlock(null, 0);
            return;
        }

        block.SetVector(ParamBaseMapOffset, state.vec);
        block.SetVector(ParamOcclusionMapOffset, state.vec);
        block.SetVector(ParamNormalMapOffset, state.normals);

        renderer.SetPropertyBlock(block, 0);
    }

    #endregion

    public void SetMaterial(int index, bool shiny) {
        GameObject[] shinyObjects = GameObject.FindGameObjectsWithTag("Shiny");

        foreach (ShinyMaterialSwapper swapper in materialSwaps) {
            swapper.SetMaterial(index);
        }

        foreach (GameObject shinyObj in shinyObjects) {
            shinyObj.SetActive(shiny);
        }

        if (index != CurrentMaterial) {
            CurrentMaterial = index;
            Instantiate(
                Resources.Load("Prefabs/Poof"),
                Camera.main.transform.position + (3f * Camera.main.transform.forward),
                Quaternion.identity);
        }
    }

    #region Eating PokePuffs
    private static int ComparePokepuffs(PokePuff p1, PokePuff p2) {
        return p1.PokePuffType.tier - p2.PokePuffType.tier;
    }

    public void EatPokepuff() {
        if (!targetPokePuff) {
            return;
        }

        if (targetPokePuff.Eat()) {
            int tier = (int) targetPokePuff.PokePuffType.tier + 1;
            int additionalHappiness = tier * tier;
            happiness += additionalHappiness;
            queuedParticles += additionalHappiness / 4;
            animator.SetTrigger(ParamEmote);
        }
    }
    #endregion

    #region Animation State Helpers
    public void LockHead() {
        if (!headLock) {
            headLockTransition = 1/7f;
        }

        headLock = true;
    }

    public void UnlockHead() {
        if (headLock) {
            for (int i = 0; i < neckTransforms.Length; i++) {
                previousRotations[i] = neckTransforms[i].rotation;
            }
        }

        headLock = false;
    }

    public void PlaySound(string sound) {
        sfx.PlayOneShot((AudioClip) Resources.Load(sound));
    }

    [Serializable]
    public class State {
        public Vector2 vec;
        public Vector2 normals;
        public State(Vector2 vec, Vector2 normals) {
            this.vec = vec;
            this.normals = normals;
        }
    }
    [Serializable]
    public class EyeState : State {
        public bool showIris;
        public EyeState(Vector2 vec, Vector2 normals, bool iris) : base(vec, normals) {
            showIris = iris;
        }
    }
    [Serializable]
    public class MouthState : State {
        public MouthState(Vector2 vec, Vector2 normals) : base(vec, normals) { }
    }
    #endregion

    [Serializable]
    public class KeyState {
        public KeyCode key;
        public bool mouthEnable;
        public MouthState mouth;
        public bool eyeEnable;
        public EyeState eye;
    }
}
