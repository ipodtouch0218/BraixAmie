using UnityEngine;

public class StreamedPokemonTexture : MonoBehaviour {

    [SerializeField] private Material material;

    private string originalMaterialName;

    public void Start() {
        GlobalManager.OnReload += OnReload;
    }

    private void OnReload(GlobalManager obj) {
        //Application.streamingAssetsPath
    }
}
