using UnityEngine;

public class PokePuff : MonoBehaviour {

    //---Properties
    public ScriptablePokePuff PokePuffType { get; set; }

    //---Private Variables
    private int eatState = 3;
    private GameObject modelObject;

    public void Start() {
        UpdateModel();
    }

    public bool Eat() {
        if (--eatState <= 0) {
            Destroy(gameObject);
            return true;
        }

        UpdateModel();
        return false;
    }

    private void UpdateModel() {
        if (modelObject) {
            Destroy(modelObject);
        }

        modelObject = Instantiate(PokePuffType.eatingStates[^eatState], transform);
        modelObject.name = "PokePuff Model";
    }
}
