using System;
using System.Collections.Generic;
using UnityEngine;

using PokeAmie.Serialization;

[Serializable]
public class BaseSettings : JsonSerializedFile {
    public BaseSettings(string file) : base(file) { }

    public float volume = 1f;
    public MicrophoneSettings microphoneSettings = new();
    public GraphicsSettings graphicsSettings = new();
    public StreamSettings streamSettings = new();
    public PokemonSettings pokemonSettings = new();
    public InputSettings inputSettings = new();
}

[Serializable]
public class MicrophoneSettings {
    public float minMicAmplitude = 0.0008f;
    public float talkDuration = 0.1f;
    public float talkStartJumpHeight = 1.5f;
    public float talkStartJumpDuration = 0.1f;
    public float talkStartJumpCooldown = 0.4f;
}

[Serializable]
public class GraphicsSettings {
    public bool showBackground = true;
    public Color32 backgroundColor = Color.green;
}

[Serializable]
public class PokemonSettings {
    public string pokemonName = "Braixen";
    public float petDuration = 2.5f, petHappiness = 20;
    public float sleepThresholdInSeconds = 300;
    public float happinessThreshold = 60;
}

[Serializable]
public class InputSettings {
    public bool inputInBackground = true;
    public Dictionary<string, string> controls = new() {
        {"pet","1"},
        {"feed","2"},
        {"sleep","3"},
        {"shiny","4"}
    };
    public Dictionary<string, string> emotions = new() {
        {"happy", "UpArrow"},
        {"angry", "RightArrow"},
        {"sad", "DownArrow"},
        {"bored", "LeftArrow"}
    };
}

[Serializable]
public class StreamSettings {
    public TwitchSettings twitchSettings = new();
    public YoutubeSettings youtubeSettings = new();
}

[Serializable]
public class TwitchSettings {
    public string channelId = "80535602";
    public PuffRedemptionSettings[] pokePuffRedemptions = {
            new() { redemptionName = "Feed a Basic PokePuff", possiblePokePuffTiers = new string[] { "Basic" } },
            new() { redemptionName = "Feed a Fancy PokePuff", possiblePokePuffTiers = new string[] { "Fancy" } },
            new() { redemptionName = "Feed a Frosted PokePuff", possiblePokePuffTiers = new string[] { "Frosted" } },
            new() { redemptionName = "Feed a Deluxe PokePuff", possiblePokePuffTiers = new string[] { "Deluxe" } },
            new() { redemptionName = "Feed a Supreme PokePuff", possiblePokePuffTiers = new string[] { "Supreme" } },
            new() { redemptionName = "Feed a Random PokePuff", possiblePokePuffTiers = new string[] { "Basic", "Fancy", "Frosted", "Deluxe", "Supreme" } }
        };
    public RedemptionSettings pettingRedemption = new() { redemptionName = "Pet the Phox" };
    public EquippableRedemptionSettings[] equippableRedemptions = {
            new() { redemptionName = "Deal With It", objectTag = "PixelGlasses", useTimer = true, timer = 600 },
            new() { redemptionName = "Deal With It", objectTag = "Crown", useTimer = false, timer = 600 }
        };
    public RecolorRedemptionSettings[] recolorRedemptions = {
            new() { redemptionName = "Shiny-ify the Phox", colorIndex = 1, hasShinyParticles = true, useTimer = true, timer = 600 }
        };
}

[Serializable]
public class YoutubeSettings {
    public string oAuth2Key = "<YOUR YOUTUBE OAUTH KEY HERE>";
    public RedemptionSettings pettingCommand = new() { redemptionName = "!pet" };
}


[Serializable]
public class RedemptionSettings {
    public string redemptionName;
}

[Serializable]
public class TimerRedemptionSettings : RedemptionSettings {
    public bool useTimer;
    public int timer;
}

[Serializable]
public class PuffRedemptionSettings : RedemptionSettings {
    public string[] possiblePokePuffTiers;
}

[Serializable]
public class RecolorRedemptionSettings : TimerRedemptionSettings {
    public int colorIndex;
    public bool hasShinyParticles;
}

[Serializable]
public class EquippableRedemptionSettings : TimerRedemptionSettings {
    public string objectTag;
}
