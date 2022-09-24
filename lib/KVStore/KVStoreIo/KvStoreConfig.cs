using System;
using UnityEngine;

namespace VPSTour.lib.KVStore.KVStoreIo {
    [CreateAssetMenu(fileName = "KvStoreConfig", menuName = MenuName, order = 1)]
    public class KvStoreConfig : ScriptableObject {
        public const string MenuName = "VPS Tour/KvStoreConfig";

        [SerializeField]
        [Tooltip("Obtain an API key from your KVStore.io Account Dashboard and copy it in here.")]
        private string apiKey = "";

        public string ApiKey {
            get {
                return !string.IsNullOrEmpty(_overrideApiKey) ? _overrideApiKey : apiKey;
            }
        }

        // Override API key for internal testing. You probably shouldn't touch this
        private readonly string _overrideApiKey = string.Empty;

        /// <summary>
        /// Gets the API Key from KvStoreConfig assets in Resources/ directories
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">Thrown if KvStoreConfig is not properly created</exception>
        public static string GetApiKey() {
            var kvStoreConfigs = Resources.LoadAll<KvStoreConfig>("");
            if (kvStoreConfigs.Length > 1) {
                var errorMessage = "There are multiple KvStoreConfig in Resources/ " +
                                   "directories, loading the first API key found. Remove extra" +
                                   " KvStoreConfigs to prevent API key problems. ";
                throw new Exception(errorMessage);
            }

            if (kvStoreConfigs.Length == 0) {
                throw new Exception($"Could not load a KvStoreConfig, please add one in a Resources/ directory");
            }

            if (string.IsNullOrEmpty(kvStoreConfigs[0].apiKey)) {
                throw new
                    Exception("API Key is empty. Please fill in API Key field in the KvStoreConfig asset in Resouces/");
            }

            return kvStoreConfigs[0].apiKey;
        }
    }
}