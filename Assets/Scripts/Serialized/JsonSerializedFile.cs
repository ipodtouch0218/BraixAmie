using System;
using System.IO;
using Newtonsoft.Json;
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
                JsonConvert.PopulateObject(File.ReadAllText(file.FullName), this);
            } catch (Exception e) {
                Debug.LogWarning($"Unable to load from {file.FullName}. Does it not exist? Generating default file.");
                Debug.Log(e.StackTrace);
            }
            Save();
        }

        public void Save() {
            file.Directory.Create();
            File.WriteAllText(file.FullName, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}
