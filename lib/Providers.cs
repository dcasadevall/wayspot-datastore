using VPSTour.lib.KVStore.KVStoreIo;
using VPSTour.lib.WayspotAnchorStore;

namespace VPSTour.lib {
    /// <summary>
    /// Class used to provide the default implementations of <see cref="IPayloadStore"/>.
    /// For simplicity, this class provides singletons and we avoid using D.I. as
    /// developers will likely be prototyping without a D.I. framework.
    ///
    /// If one wishes to inject this dependency, they can perform the wiring with
    /// their D.I. method of choice, by manually instantiating <see cref="KvPayloadStore"/>
    /// as well as <see cref="KvStoreIO"/>.
    /// </summary>
    public static class Providers {
        private static IPayloadStore payloadStore;
        private static ICallbackPayloadStore callbackPayloadStore;

        public static IPayloadStore ProvidePayloadStore() {
            if (payloadStore == null) {
                var apiKey = KvStoreConfig.GetApiKey();
                var kvStore = new KvStoreIO(apiKey);
                payloadStore = new KvPayloadStore(kvStore);
            }

            return payloadStore; 
        }
        
        public static ICallbackPayloadStore ProvideCallbackPayloadStore() {
            if (callbackPayloadStore == null) {
                callbackPayloadStore = new CallbackStoreProxy(ProvidePayloadStore());
            }

            return callbackPayloadStore; 
        }
    }
}