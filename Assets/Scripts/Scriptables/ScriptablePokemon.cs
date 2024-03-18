using UnityEngine;

[CreateAssetMenu(fileName = "ScriptablePokemon", menuName = "Scriptables/Pokemon")]
public class ScriptablePokemon : ScriptableObject {

    public GameObject prefab;
    public float cameraHeight = 1f;

}