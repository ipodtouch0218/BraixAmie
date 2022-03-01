using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using TwitchLib.Unity;

public class TwitchController : MonoBehaviour {

    public static TwitchController Instance { get; private set; }
    
    private readonly Dictionary<string, List<PokePuff>> puffTable = new();
    private PubSub twitchApi;

    public BaseSettings settings;
    public Statistics statistics;

    public GameObject background;
    public AudioMixer audioMixer;

    [System.Obsolete]
    public void Awake() {
        Instance = this;
        foreach (PokePuff puff in Resources.LoadAll<PokePuff>("PokePuffs")) {
            puffTable.TryGetValue(puff.tier.ToString(), out List<PokePuff> list);
            if (list == null)
                list = puffTable[puff.tier.ToString()] = new();
            list.Add(puff);
        }
        settings = new(Application.persistentDataPath + "/configuration.json");
        statistics = new(Application.persistentDataPath + "/statistics.json");
        settings.Save();
        statistics.Save();
        OnReload();
        
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
    void Connect() {
        if (twitchApi != null)
            twitchApi.Disconnect();

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
    
    private void OnSubscription(object sender, TwitchLib.PubSub.Events.OnChannelSubscriptionArgs args) {
        BraixenController.Instance.happiness += ParseNullableInt(args.Subscription.Months) * ((int) args.Subscription.SubscriptionPlan + 1);
        BraixenController.Instance.animator.SetTrigger("emote");
    }

    private int ParseNullableInt(int? number) {
        if (number != null)
            return (int) number;
        return 1;
    }

    private void OnRewardRedeemed(object sender, TwitchLib.PubSub.Events.OnRewardRedeemedArgs args) {
        
        PuffRedemptionSettings puffRedemption = FindPuffRedemption(args.RewardTitle);
        if (puffRedemption != null) {
            string[] puffs = puffRedemption.possiblePokePuffTiers;
            List<PokePuff> possibleFlavors = puffTable[puffs[Random.Range(0, puffs.Length)]];
            PokePuff pokePuff = possibleFlavors[Random.Range(0, possibleFlavors.Count)];

            PokePuffHandler handler = ((GameObject) Instantiate(Resources.Load("Prefabs/PokePuff"))).GetComponent<PokePuffHandler>();
            handler.puffType = pokePuff;
            handler.transform.position = new(-1, Random.Range(1.5f, 1.8f), Random.Range(-0.8f, 0.8f));

            ViewerStats stats = statistics.GetStatsOfViewer(args.DisplayName);
            stats.pokepuffsFed++;
        }
        if (args.RewardTitle == settings.twitchSettings.pettingRedemption.redemptionName) {
            BraixenController.Instance.petTimer = settings.braixenSettings.petDuration;

            ViewerStats stats = statistics.GetStatsOfViewer(args.DisplayName);
            stats.petsGiven++;
        }

        statistics.Save();
    }
    
    private PuffRedemptionSettings FindPuffRedemption(string name) {
        foreach (var redemptions in settings.twitchSettings.pokePuffRedemptions) {
            if (redemptions.redemptionName == name)
                return redemptions;
        }
        return null;
    }
    #endregion
}
