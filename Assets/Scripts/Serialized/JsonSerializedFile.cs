using UnityEngine;

namespace PokeAmie.Serialization {

    [System.Serializable]
    public class JsonSerializedFile {

        [System.NonSerialized]
        protected System.IO.FileInfo file;

        public JsonSerializedFile(string path) {
            file = new(path);
            Load();
        }
        public void Load() {
            try {
                JsonUtility.FromJsonOverwrite(System.IO.File.ReadAllText(file.FullName), this);
            } catch (System.Exception) {
                Debug.LogWarning($"Unable to load from {file.FullName}. Does it not exist? Generating default file.");
                Save();
            }
        }
        public void Save() {
            file.Directory.Create();
            System.IO.File.WriteAllText(file.FullName, JsonUtility.ToJson(this, true));
        }
    }
}
