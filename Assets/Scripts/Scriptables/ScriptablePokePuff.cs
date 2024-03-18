using UnityEngine;

[CreateAssetMenu(fileName = "ScriptablePokePuff", menuName = "Scriptables/PokePuff")]
public class ScriptablePokePuff : ScriptableObject {

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
