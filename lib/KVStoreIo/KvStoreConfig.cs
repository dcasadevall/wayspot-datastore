using UnityEngine;

namespace VPSTour.lib.KVStoreIo {
    [CreateAssetMenu(fileName = "KvStoreConfig", menuName = "VPS Tour/KvStoreConfig", order = 1)]
    public class KvStoreConfig : ScriptableObject {
        [SerializeField]
        [Tooltip("Obtain an API key from your KVStore.io Account Dashboard and copy it in here.")]
        private string _apiKey = "";

        public string ApiKey {
            get {
                return !string.IsNullOrEmpty(_overrideApiKey) ? _overrideApiKey : _apiKey;
            }
        }

        // Override API key for internal testing. You probably shouldn't touch this
        private readonly string _overrideApiKey = string.Empty;
    }
}