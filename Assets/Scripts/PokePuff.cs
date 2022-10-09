using UnityEngine;

[CreateAssetMenu(fileName = "PokePuff", menuName = "Scriptables/PokePuff")]
public class PokePuff : ScriptableObject {

    public PokePuffFlavor flavor = PokePuffFlavor.Citrus;
    public PokePuffTier tier = PokePuffTier.Basic;
    public GameObject[] eatingStates;

    public enum PokePuffFlavor {
        Citrus, Mint, Mocha, Spice, Sweet
    }
    public enum PokePuffTier {
        Basic = 1, Fancy, Frosted, Deluxe, Supreme
    }
}
