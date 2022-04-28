using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BraixenController : MonoBehaviour {

    public static BraixenController Instance { get; private set; }
    private BaseSettings Settings {
        get {
            return TwitchController.Instance.settings;
        }
    }

    MaterialPropertyBlock eyeBlock, mouthBlock;
    MaterialPropertyBlock[] irisBlocks;
    public SkinnedMeshRenderer eyeRenderer, mouthRenderer;
    public SkinnedMeshRenderer[] irisRenderers;

    public GameObject petHand, glasses;

    public Animator animator;
    public Transform neckTransform, petTarget;
    public float happiness, timeSinceLastInteraction;

    public bool headLock, beingPet;
    public PokePuffHandler targetPokePuff;

    public Quaternion previousRotation;

    public float headLockTransition, petTimer, randomLookTimer, particleSpawnTimer;
    public Vector3? randomLookPosition = null;

    public ParticleSystem heartsParticle;
    private ParticleSystem.EmissionModule emissionModule;
    private AudioSource sfx;

    public int queuedParticles;

    public void Start() {
        Instance = this;
        animator = GetComponentInChildren<Animator>();
        sfx = GetComponent<AudioSource>();
        previousRotation = neckTransform.rotation;
        StartCoroutine(CheckForPokepuffs());
        StartCoroutine(AttemptIdleAnimation());

        emissionModule = heartsParticle.emission;

        eyeRenderer.GetPropertyBlock(eyeBlock = new());
        mouthRenderer.GetPropertyBlock(mouthBlock = new());
        irisBlocks = new MaterialPropertyBlock[irisRenderers.Length];
        for (int i = 0; i < irisRenderers.Length; i++)
            irisRenderers[i].GetPropertyBlock(irisBlocks[i] = new());
    }

    public void Update() {
        EyeState eyeState = null;
        MouthState mouthState = null;

        if (IsIdle())
            timeSinceLastInteraction += Time.deltaTime;

        headLockTransition = Mathf.Max(0, headLockTransition - Time.deltaTime);
        happiness = Mathf.Max(0, happiness - (Time.deltaTime / 60f * Mathf.Log(happiness + 1)));

        emissionModule.rateOverTime = Mathf.Max(0, Mathf.Min(8, happiness / 80f));

        if ((randomLookTimer -= Time.deltaTime) <= 0) {
            randomLookTimer = Random.Range(1f, 3f);
            if (Random.value < 0.4f) {
                randomLookPosition = Camera.main.transform.position;
                randomLookTimer += 20f;
            } else {
                Vector3 target = transform.position + (transform.forward * 3f) + (Vector3.up * 1.5f);
                target.y += Random.Range(-1.1f, 1.1f);
                target.z += Random.Range(-2f, 2f);
                randomLookPosition = target;
            }
        }

        beingPet = IsIdle() && !targetPokePuff && petTimer > 0;
        if (beingPet) {
            animator.ResetTrigger("idle");
            petHand.SetActive(true);
            eyeState = EyeState.HappyEmote;
            mouthState = MouthState.SmallOpen;
            float petDuration = Settings.braixenSettings.petDuration;
            petTimer = Mathf.Max(0, petTimer - Time.deltaTime);
            if (petTimer % petDuration <= Mathf.Max(0, petTimer - Time.deltaTime) % petDuration) {
                happiness += Settings.braixenSettings.petHappiness;
                queuedParticles += (int) (Settings.braixenSettings.petHappiness / 2);
            }
            if (petTimer <= 0) {
                beingPet = false;
                animator.Play("Pet_agree");
                petHand.SetActive(false);
            }
        }
        SetAnimatorStates();

        if (queuedParticles > 0 && (particleSpawnTimer -= Time.deltaTime) < 0) {
            string name = animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
            if (name == "Pet_agree" || name == "Pet_very_happy" || beingPet) {
                heartsParticle.Emit(1);
                queuedParticles--;
                particleSpawnTimer = 0.11f;
            }
        }

        SetStateThisFrame(eyeRenderer, eyeBlock, eyeState, "_BaseMapOffset");
        SetStateThisFrame(mouthRenderer, mouthBlock, mouthState, "_BaseMapOffset");
        for (int i = 0; i < irisRenderers.Length; i++) {
            irisRenderers[i].enabled = eyeState == null || eyeState.showIris;
            SetStateThisFrame(irisRenderers[i], irisBlocks[i], eyeState, "_BaseMapOffset", "_OcclusionMapOffset", "_NormalMapOffset");
        }
    }

    public void LateUpdate() {
        Vector3? target = FindBestLookTarget();
        if (target != null)
            LookAtLocation((Vector3) target);
    }

    #region Coroutines
    IEnumerator CheckForPokepuffs() {
        while (true) {
            if (!targetPokePuff && !beingPet) {
                animator.ResetTrigger("idle");
                PokePuffHandler[] pokePuffs = FindObjectsOfType<PokePuffHandler>();
                if (pokePuffs.Length > 0) {
                    System.Array.Sort(pokePuffs, ComparePokepuffs);
                    targetPokePuff = pokePuffs[0];
                    timeSinceLastInteraction = 0f;
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator AttemptIdleAnimation() {
        while (true) {
            if (timeSinceLastInteraction > Settings.braixenSettings.sleepThresholdInSeconds/4 
                && timeSinceLastInteraction < Settings.braixenSettings.sleepThresholdInSeconds
                && Random.value < 0.3f) {

                animator.SetTrigger("idle");
            }
            yield return new WaitForSeconds(5f);
        }
    }

    #endregion


    #region Animation Stuffs
    void LookAtLocation(Vector3 target) {
        Quaternion targetRot = Quaternion.LookRotation(target - transform.position - 2 * Vector3.up, Vector3.forward);
        float time = (headLockTransition > 0 ? 12 : 4) * Time.deltaTime;
        previousRotation = neckTransform.rotation = Quaternion.Slerp(previousRotation, targetRot, time);
    }
    Vector3? FindBestLookTarget() {
        if (headLock) {
            if (headLockTransition > 0)
                return Camera.main.transform.position + (Vector3.down * 2);
            return null;
        }

        if (targetPokePuff)
            return targetPokePuff.transform.position;

        if (beingPet)
            return petTarget.position;

        return randomLookPosition;
    }
    void SetAnimatorStates() {
        animator.SetBool("happy", happiness > Settings.braixenSettings.happinessThreshold);
        animator.SetBool("sleep", timeSinceLastInteraction > Settings.braixenSettings.sleepThresholdInSeconds);
        animator.SetBool("eating", targetPokePuff != null);
    }
    bool IsIdle() {
        return animator.GetCurrentAnimatorStateInfo(0).IsName("Pet_idle");
    }

    void SetStateThisFrame(SkinnedMeshRenderer renderer, MaterialPropertyBlock block, State state, params string[] offsets) {
        if (state == null) {
            renderer.SetPropertyBlock(null, 0);
            return;
        }
        for (int i = 0; i < offsets.Length; i++)
            block.SetVector(offsets[i], state.vec);
        renderer.SetPropertyBlock(block, 0);
    }

    #endregion

    #region Eating PokePuffs
    static int ComparePokepuffs(PokePuffHandler p1, PokePuffHandler p2) {
        return p1.puffType.tier - p2.puffType.tier;
    }
    public void EatPokepuff() {
        if (!targetPokePuff)
            return;

        targetPokePuff.Eat();
        if (targetPokePuff.eatState == 0) {
            int tier = (int) targetPokePuff.puffType.tier + 1;
            happiness += tier * tier;
            queuedParticles += (int) (tier * tier / 4);
            animator.SetTrigger("emote");
        }
    }
    #endregion

    #region Animation State Helpers
    public void LockHead() {
        if (!headLock)
            headLockTransition = 1/7f;
        headLock = true;
    }
    public void UnlockHead() {
        if (headLock)
            previousRotation = neckTransform.rotation;
        headLock = false;
    }
    public void PlaySound(string sound) {
        sfx.PlayOneShot((AudioClip) Resources.Load(sound));
    }

    public class State {
        public Vector2 vec;
        public State(float x, float y) { 
            vec = new(x, y); 
        }
    }
    public class EyeState : State {
        public static EyeState NormalOpen = new(0, 1, true), NeutralClosed = new(0, 1.25f, false), NormalClosed = new(0, 1.25f, false),
            HalfOpen = new(0, 1.75f, true), Angry = new(1, 1, true), Sad = new(1, 1.5f, true), HappyEmote = new(1, 1.75f, false);
        public bool showIris;
        public EyeState(float x, float y, bool iris) : base(x, y) {
            showIris = iris;
        }
    }
    public class MouthState : State {
        public static MouthState NormalSmile = new(0, 0), Neutral = new(0, 0.25f), BigOpen = new(0, 0.5f),
            MediumOpen = new(0, 0.75f), SmallOpen = new(1, 0), Angry = new(1, 0.5f), Frown = new(1, 0.75f);
        public MouthState(float x, float y) : base(x, y) { }
    }
    #endregion
}
