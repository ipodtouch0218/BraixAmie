using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class ShinyMaterialSwapper : MonoBehaviour {

    [SerializeField] private Material[] materials;

    private SkinnedMeshRenderer smr;

    public void Start() {
        smr = GetComponent<SkinnedMeshRenderer>();
        SetMaterial(0);
    }

    public void SetMaterial(int index) {
        if (smr) {
            smr.material = materials[index];
        }
    }
}
