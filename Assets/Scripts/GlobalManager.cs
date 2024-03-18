using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class GlobalManager : MonoBehaviour {

    //---Static
    public static GlobalManager Instance { get; private set; }

    //---Public Variables
    public BaseSettings settings;
    public Statistics statistics;

    //---Serialized Variables
    [SerializeField] public GameObject petHand;
    [SerializeField] public ParticleSystem hearts;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private GameObject background;

    [SerializeField] private ScriptablePokemon[] allPokemon;

    //---Private Variables
    private PokemonController currentPokemon;

    public void Awake() {
        Instance = this;
        LoadFromFile();
        OnReload();
    }

    public void Update() {
        if (Input.GetKeyDown(KeyCode.F5)) {
            settings.Load();
            OnReload();
        }
    }

    public void OnReload() {
        // Set the volume
        audioMixer.SetFloat("Volume", settings.volume);

        // Enable/disable the background
        background.SetActive(settings.graphicsSettings.showBackground);
        Camera.main.backgroundColor = settings.graphicsSettings.backgroundColor;

        // Create the Pokemon
        if (currentPokemon) {
            Destroy(currentPokemon.transform.parent.gameObject);
        }

        ScriptablePokemon newPokemon = allPokemon.FirstOrDefault(sp => sp.name == settings.pokemonSettings.pokemonName);
        if (newPokemon) {
            GameObject newPokemonObject = Instantiate(newPokemon.prefab);
            currentPokemon = newPokemonObject.GetComponentInChildren<PokemonController>();

            Transform cameraTransform = Camera.main.transform;
            Vector3 position = cameraTransform.position;
            position.y = newPokemon.cameraHeight;
            cameraTransform.position = position;
        }
    }

    public void LoadFromFile() {
        settings = new BaseSettings(Application.persistentDataPath + "/configuration.json");
        statistics = new Statistics(Application.persistentDataPath + "/statistics.json");
    }
}