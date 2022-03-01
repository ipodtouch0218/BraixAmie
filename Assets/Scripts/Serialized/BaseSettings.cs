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
            new() { redemptionName = "Feed a Random PokePuff",
                    possiblePokePuffTiers = new string[] { "Basic", "Fancy", "Frosted", "Deluxe", "Supreme" } }
         };
    public PetRedemptionSettings pettingRedemption = new();
}

[System.Serializable]
public class PuffRedemptionSettings {
    public string redemptionName;
    public string[] possiblePokePuffTiers;
}

[System.Serializable]
public class PetRedemptionSettings {
    public string redemptionName = "Pet the Phox";
}