using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using TwitchLib.Unity;
using Random = UnityEngine.Random;

public class TwitchController : MonoBehaviour {

    //---Static
    public static TwitchController Instance { get; private set; }
    private static BaseSettings Settings => GlobalManager.Instance.settings;
    private static Statistics Statistics => GlobalManager.Instance.statistics;

    //---Private Variables
    private PubSub twitchApi;
    private Coroutine recolorCoroutine;
    private readonly Dictionary<EquippableRedemptionSettings, Coroutine> disableRoutines = new();
    private readonly Dictionary<string, List<ScriptablePokePuff>> puffTable = new();
    private ShinyMaterialSwapper[] materialSwaps;

    [Obsolete]
    public void Awake() {
        Instance = this;
        foreach (ScriptablePokePuff puff in Resources.LoadAll<ScriptablePokePuff>("PokePuffs")) {
            puffTable.TryGetValue(puff.tier.ToString(), out List<ScriptablePokePuff> list);

            list ??= (puffTable[puff.tier.ToString()] = new List<ScriptablePokePuff>());
            list.Add(puff);
        }

        materialSwaps = FindObjectsOfType<ShinyMaterialSwapper>(true);

        Connect();
    }

    #region Twitch Functions
    [Obsolete]
    private void Connect() {
        twitchApi?.Disconnect();
        twitchApi = new PubSub();

        twitchApi.OnPubSubServiceConnected += OnPubSubConnected;
        twitchApi.OnRewardRedeemed += OnRewardRedeemed;
        twitchApi.ListenToRewards(Settings.streamSettings.twitchSettings.channelId);
        twitchApi.Connect();
    }

    public void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            PokemonController.Instance.petTimer += Settings.pokemonSettings.petDuration;
            PokemonController.Instance.timeSinceLastInteraction = 0;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            List<ScriptablePokePuff> possibleFlavors = puffTable.ElementAt(Random.Range(0, puffTable.Count)).Value;
            ScriptablePokePuff pokePuff = possibleFlavors[Random.Range(0, possibleFlavors.Count)];

            PokePuff handler = ((GameObject) Instantiate(Resources.Load("Prefabs/PokePuff"))).GetComponent<PokePuff>();
            handler.PokePuffType = pokePuff;
            handler.transform.position = new Vector3(-1, Camera.main.transform.position.y + Random.Range(-0.5f, -0.2f), Random.Range(-0.8f, 0.8f));

            Instantiate(Resources.Load("Prefabs/Poof"), handler.transform.position, Quaternion.identity);
        }

    }

    private void OnPubSubConnected(object sender, EventArgs args) {
        Debug.Log("Connected to Twitch");
        twitchApi.SendTopics();
    }

    private void OnRewardRedeemed(object sender, TwitchLib.PubSub.Events.OnRewardRedeemedArgs args) {

        PuffRedemptionSettings puffRedemption = FindRedemption(args.RewardTitle, Settings.streamSettings.twitchSettings.pokePuffRedemptions);
        if (puffRedemption != null) {
            string[] puffs = puffRedemption.possiblePokePuffTiers;
            List<ScriptablePokePuff> possibleFlavors = puffTable[puffs[Random.Range(0, puffs.Length)]];
            ScriptablePokePuff pokePuff = possibleFlavors[Random.Range(0, possibleFlavors.Count)];

            PokePuff handler = ((GameObject) Instantiate(Resources.Load("Prefabs/PokePuff"))).GetComponent<PokePuff>();
            handler.PokePuffType = pokePuff;
            handler.transform.position = new Vector3(-1, Camera.main.transform.position.y + Random.Range(-0.5f, -0.2f), Random.Range(-0.8f, 0.8f));

            Instantiate(Resources.Load("Prefabs/Poof"), handler.transform.position, Quaternion.identity);

            ViewerStats stats = Statistics.GetStatsOfViewer(args.DisplayName);
            stats.pokepuffsFed++;
        }

        EquippableRedemptionSettings equipRedemption = FindRedemption(args.RewardTitle, Settings.streamSettings.twitchSettings.equippableRedemptions);
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

        RecolorRedemptionSettings recolorRedemption = FindRedemption(args.RewardTitle, Settings.streamSettings.twitchSettings.recolorRedemptions);
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

        if (args.RewardTitle == Settings.streamSettings.twitchSettings.pettingRedemption.redemptionName) {
            PokemonController.Instance.petTimer += Settings.pokemonSettings.petDuration;
            PokemonController.Instance.timeSinceLastInteraction = 0;

            ViewerStats stats = Statistics.GetStatsOfViewer(args.DisplayName);
            stats.petsGiven++;
        }

        Statistics.Save();
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

    private static IEnumerator DisableObjectsInTime(IEnumerable<GameObject> objects, int timer) {
        yield return new WaitForSeconds(timer);
        foreach (GameObject obj in objects) {
            obj.SetActive(false);
            Instantiate(Resources.Load("Prefabs/Poof"), obj.transform.position, Quaternion.identity);
        }
    }

    private static T FindRedemption<T>(string name, IEnumerable<T> redemptions) where T : RedemptionSettings {
        return redemptions.FirstOrDefault(redemption => redemption.redemptionName == name);
    }
    #endregion
}
