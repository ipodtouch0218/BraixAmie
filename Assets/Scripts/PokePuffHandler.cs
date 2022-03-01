using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokePuffHandler : MonoBehaviour {

    public Material outlineMaterial;
    public PokePuff puffType;
    public int eatState = 3;

    private GameObject modelObject;

    public void Start() {
        UpdateModel();
    }
    public void Eat() {
        if (--eatState <= 0) {
            Destroy(gameObject);
            return;
        }

        UpdateModel();
    }
    private void UpdateModel() {
        if (modelObject)
            Destroy(modelObject);
        modelObject = Instantiate(puffType.eatingStates[^eatState], transform);
        modelObject.name = "PokePuff Model";

        foreach (var smr in modelObject.GetComponentsInChildren<SkinnedMeshRenderer>()) {
            List<Material> materials = new();
            smr.GetMaterials(materials);
            materials.Add(outlineMaterial);
            smr.materials = materials.ToArray();
        }
        foreach (var mr in modelObject.GetComponentsInChildren<MeshRenderer>()) {
            List<Material> materials = new();
            mr.GetMaterials(materials);
            materials.Add(outlineMaterial);
            mr.materials = materials.ToArray();
        }
    }
}
