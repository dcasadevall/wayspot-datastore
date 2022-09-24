using System.Collections.Generic;
using System.Threading.Tasks;
using Niantic.ARDK.AR.WayspotAnchors;
using UnityEngine;
using VPSTour.lib.KVStore;

namespace VPSTour.lib.WayspotAnchorStore {
    /// <summary>
    /// Implementation of <see cref="IPayloadStore"/> which uses
    /// </summary>
    public class KvPayloadStore : IPayloadStore {
        private readonly IKvStore kvStore;

        public KvPayloadStore(IKvStore kvStore) {
            this.kvStore = kvStore;
        }
        
        public async Task Persist(IWayspotAnchor[] wayspotAnchors) {
            foreach (var anchor in wayspotAnchors) {
                var id = anchor.ID.ToString();
                var serializedPayload = anchor.Payload.Serialize();
                Debug.Log("Saving: " + id);
                
                // This performs a call to our KVStore which can take a while
                // Usually, key value stores will perform an RPC
                // This is fine for prototypes / samples, but in a real application
                // one would want to choose a key value store that allows
                // for batch set values
                await kvStore.SetValue(id, serializedPayload);
                Debug.Log("Saved: " + id);
            }
        }

        public async Task<WayspotAnchorPayload[]> Restore() {
            // We need to get all key value pairs, because we don't know ahead of time
            // which ids we are storing.
            var kvps = await kvStore.GetValues();
            var payloads = new List<WayspotAnchorPayload>();
            
            // Note that the values returned are simply payloads, not the id + payload.
            // This is because WayspotAnchorService can restore wayspot anchors
            // from payloads, without needing their ids
            foreach (var keyValuePair in kvps) {
                payloads.Add(WayspotAnchorPayload.Deserialize(keyValuePair.Value));
            }

            return payloads.ToArray();
        }

        public Task Clear() {
            return kvStore.DeleteAllKeys();
        }
    }
}