using UnityEngine;
using VPSTour.lib.KVStore.KVStoreIo;

namespace VPSTour.Demo {
    public class Test : MonoBehaviour {
        [SerializeField]
        private KvStoreConfig cfg;
        
        private async void Start() {
            var store = new KvStoreIO(cfg.ApiKey);
            await store.SetValue("foo", "bar");
            var value = await store.GetValue("foo");
            Debug.Log("Value: " + value);
            await store.SetValue("foo", "baz");
            value = await store.GetValue("foo");
            Debug.Log("Value: " + value);
        }
    }
}