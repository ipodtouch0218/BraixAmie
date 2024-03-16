using PokeAmie.Serialization;

[System.Serializable]
public class BaseSettings : JsonSerializedFile {
    public BaseSettings(string file) : base(file) { }

    public bool showBackground = true;
    public float volume = 1f;
    public BraixenSettings braixenSettings = new();
    public TwitchSettings twitchSettings = new();
}

[System.Serializable]
public class BraixenSettings {
    public float petDuration = 2.5f, petHappiness = 10;
    public float sleepThresholdInSeconds = 180;
    public float happinessThreshold = 60;
}

[System.Serializable]
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

[System.Serializable]
public class YoutubeSettings {
    public string channelId = "";
    public RedemptionSettings pettingCommand = new() { redemptionName = "!pet" };
}


[System.Serializable]
public class RedemptionSettings {
    public string redemptionName;
}

[System.Serializable]
public class TimerRedemptionSettings : RedemptionSettings {
    public bool useTimer;
    public int timer;
}

[System.Serializable]
public class PuffRedemptionSettings : RedemptionSettings {
    public string[] possiblePokePuffTiers;
}

[System.Serializable]
public class RecolorRedemptionSettings : TimerRedemptionSettings {
    public int colorIndex;
    public bool hasShinyParticles;
}

[System.Serializable]
public class EquippableRedemptionSettings : TimerRedemptionSettings {
    public string objectTag;
}
