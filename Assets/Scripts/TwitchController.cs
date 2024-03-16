using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

using TwitchLib.Unity;

public class TwitchController : MonoBehaviour {

    public static TwitchController Instance { get; private set; }

    private Coroutine recolorCoroutine;
    private readonly Dictionary<EquippableRedemptionSettings, Coroutine> disableRoutines = new();
    private readonly Dictionary<string, List<PokePuff>> puffTable = new();
    private PubSub twitchApi;

    public BaseSettings settings;
    public Statistics statistics;

    public GameObject background;
    public AudioMixer audioMixer;

    private ShinyMaterialSwapper[] materialSwaps;

    [System.Obsolete]
    public void Awake() {
        Instance = this;
        foreach (PokePuff puff in Resources.LoadAll<PokePuff>("PokePuffs")) {
            puffTable.TryGetValue(puff.tier.ToString(), out List<PokePuff> list);
            if (list == null) {
                list = puffTable[puff.tier.ToString()] = new();
            }

            list.Add(puff);
        }
        settings = new(Application.persistentDataPath + "/configuration.json");
        statistics = new(Application.persistentDataPath + "/statistics.json");
        settings.Save();
        statistics.Save();
        OnReload();

        materialSwaps = (ShinyMaterialSwapper[]) FindObjectsOfTypeAll(typeof(ShinyMaterialSwapper));

        Connect();
    }

    public void Update() {
        if (Input.GetKeyDown(KeyCode.F5)) {
            settings.Load();
            OnReload();
        }
    }

    public void OnReload() {
        SetVolume();
        background.SetActive(settings.showBackground);
    }
    public void SetVolume() {
        audioMixer.SetFloat("Volume", settings.volume);
    }

    #region Twitch Functions
    [System.Obsolete]
    private void Connect() {
        if (twitchApi != null) {
            twitchApi.Disconnect();
        }

        twitchApi = new PubSub();

        twitchApi.OnPubSubServiceConnected += OnPubSubConnected;
        twitchApi.OnRewardRedeemed += OnRewardRedeemed;
       // twitchApi.OnChannelSubscription += OnSubscription;
        twitchApi.ListenToRewards(settings.twitchSettings.channelId);
       // twitchApi.ListenToSubscriptions(settings.twitchSettings.channelId);
        twitchApi.Connect();
    }

    private void OnPubSubConnected(object sender, System.EventArgs args) {
        Debug.Log("Connected to Twitch");
        twitchApi.SendTopics();
    }

    //private void OnSubscription(object sender, TwitchLib.PubSub.Events.OnChannelSubscriptionArgs args) {
    //    PokemonController.Instance.happiness += ParseNullableInt(args.Subscription.Months) * ((int) args.Subscription.SubscriptionPlan + 1);
    //    PokemonController.Instance.animator.SetTrigger("emote");
    //}

    //private int ParseNullableInt(int? number) {
    //    if (number != null)
    //        return (int) number;
    //    return 1;
    //}

    private void OnRewardRedeemed(object sender, TwitchLib.PubSub.Events.OnRewardRedeemedArgs args) {

        PuffRedemptionSettings puffRedemption = FindRedemption(args.RewardTitle, settings.twitchSettings.pokePuffRedemptions);
        if (puffRedemption != null) {
            string[] puffs = puffRedemption.possiblePokePuffTiers;
            List<PokePuff> possibleFlavors = puffTable[puffs[Random.Range(0, puffs.Length)]];
            PokePuff pokePuff = possibleFlavors[Random.Range(0, possibleFlavors.Count)];

            PokePuffHandler handler = ((GameObject) Instantiate(Resources.Load("Prefabs/PokePuff"))).GetComponent<PokePuffHandler>();
            handler.puffType = pokePuff;
            handler.transform.position = new Vector3(-1, Camera.main.transform.position.y + Random.Range(-0.5f, -0.2f), Random.Range(-0.8f, 0.8f));

            Instantiate(Resources.Load("Prefabs/Poof"), handler.transform.position, Quaternion.identity);

            ViewerStats stats = statistics.GetStatsOfViewer(args.DisplayName);
            stats.pokepuffsFed++;
        }

        EquippableRedemptionSettings equipRedemption = FindRedemption(args.RewardTitle, settings.twitchSettings.equippableRedemptions);
        if (equipRedemption != null) {

            GameObject[] objects = GameObject.FindGameObjectsWithTag(equipRedemption.objectTag);
            foreach (GameObject obj in objects) {
                obj.SetActive(equipRedemption.useTimer || !obj.activeSelf);
                Instantiate(Resources.Load("Prefabs/Poof"), obj.transform.position, Quaternion.identity);
            }

            if (equipRedemption.useTimer) {
                if (disableRoutines.TryGetValue(equipRedemption, out Coroutine routine)) {
                    StopCoroutine(routine);
                }

                disableRoutines[equipRedemption] = StartCoroutine(DisableObjectsInTime(objects, equipRedemption.timer));
            }
        }

        RecolorRedemptionSettings recolorRedemption = FindRedemption(args.RewardTitle, settings.twitchSettings.recolorRedemptions);
        if (recolorRedemption != null) {

            foreach (ShinyMaterialSwapper swapper in materialSwaps) {
                swapper.SetMaterial(recolorRedemption.colorIndex);
            }

            GameObject[] shinyObjects = GameObject.FindGameObjectsWithTag("Shiny");
            foreach (GameObject shiny in shinyObjects) {
                shiny.SetActive(recolorRedemption.hasShinyParticles);
            }

            if (recolorCoroutine != null) {
                StopCoroutine(recolorCoroutine);
            }

            if (recolorRedemption.useTimer) {
                recolorCoroutine = StartCoroutine(DisableRecolorInTime(recolorRedemption.timer));
            }
        }

        if (args.RewardTitle == settings.twitchSettings.pettingRedemption.redemptionName) {
            PokemonController.Instance.petTimer += settings.braixenSettings.petDuration;
            PokemonController.Instance.timeSinceLastInteraction = 0;

            ViewerStats stats = statistics.GetStatsOfViewer(args.DisplayName);
            stats.petsGiven++;
        }

        statistics.Save();
    }

    private IEnumerator DisableRecolorInTime(int timer) {
        yield return new WaitForSeconds(timer);

        foreach (ShinyMaterialSwapper swapper in materialSwaps) {
            swapper.SetMaterial(0);
        }

        GameObject[] shinyObjects = GameObject.FindGameObjectsWithTag("Shiny");
        foreach (GameObject shiny in shinyObjects) {
            shiny.SetActive(false);
        }
    }

    private IEnumerator DisableObjectsInTime(GameObject[] objects, int timer) {
        yield return new WaitForSeconds(timer);
        foreach (GameObject obj in objects) {
            obj.SetActive(false);
            Instantiate(Resources.Load("Prefabs/Poof"), obj.transform.position, Quaternion.identity);
        }
    }

    private T FindRedemption<T>(string name, T[] redemptions) where T : RedemptionSettings {
        foreach (T redemption in redemptions) {
            if (redemption.redemptionName == name) {
                return redemption;
            }
        }
        return null;
    }
    #endregion
}
