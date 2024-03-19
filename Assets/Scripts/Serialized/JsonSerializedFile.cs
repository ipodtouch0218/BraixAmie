using System;
using System.IO;
using UnityEngine;

namespace PokeAmie.Serialization {

    [Serializable]
    public class JsonSerializedFile {

        [NonSerialized]
        public FileInfo file;

        public JsonSerializedFile(string path) {
            file = new FileInfo(path);
            Load();
        }

        public void Load() {
            try {
                JsonUtility.FromJsonOverwrite(File.ReadAllText(file.FullName), this);
            } catch (Exception) {
                Debug.LogWarning($"Unable to load from {file.FullName}. Does it not exist? Generating default file.");
            }
            Save();
        }

        public void Save() {
            file.Directory.Create();
            File.WriteAllText(file.FullName, JsonUtility.ToJson(this, true));
        }
    }
}
