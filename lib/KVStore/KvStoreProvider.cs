using System;
using UnityEngine;
using VPSTour.lib.KVStore.KVStoreIo;

namespace VPSTour.lib.KVStore {
    /// <summary>
    /// Class used to provide the default implementation of <see cref="IKvStore"/>.
    /// For simplicity, this class is a singleton and we avoid using D.I. as
    /// developers will likely be prototyping without a D.I. framework.
    ///
    /// If one wishes to inject this dependency, they can perform the wiring with
    /// their D.I. method of choice, by manually instantiating <see cref="KvStoreIO"/>
    /// using the key from <see cref="KvStoreConfig"/>.
    /// </summary>
    public class KvStoreProvider {
        private static IKvStore kvStore = null;

        public static IKvStore KvStore {
            get {
                if (kvStore == null) {
                    var apiKey = KvStoreConfig.GetApiKey();
                    kvStore = new KvStoreIO(apiKey);
                }

                return kvStore;
            }
        }

    }
}