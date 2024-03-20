using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ShinyMaterialSwapper : MonoBehaviour {

    [SerializeField] private Material[] materials;

    private Renderer smr;

    public void Start() {
        smr = GetComponent<Renderer>();
        SetMaterial(0);
    }

    public void SetMaterial(int index) {
        if (smr) {
            smr.material = materials[index];
        }
    }
}
