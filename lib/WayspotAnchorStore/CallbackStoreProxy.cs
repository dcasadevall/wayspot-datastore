using System;
using System.Threading;
using System.Threading.Tasks;
using Niantic.ARDK.AR.WayspotAnchors;

namespace VPSTour.lib.WayspotAnchorStore {
    /// <summary>
    /// <see cref="ICallbackPayloadStore"/> implementation that converts a given <see cref="IPayloadStore"/>
    /// implementation into the callback version.
    /// </summary>
    public class CallbackStoreProxy : ICallbackPayloadStore {
        private readonly IPayloadStore payloadStore;

        public CallbackStoreProxy(IPayloadStore payloadStore) {
            this.payloadStore = payloadStore;
        }

        public void Persist(IWayspotAnchor[] payloads, Action resultHandler) {
            Task task = payloadStore.Persist(payloads);
            ExecutionContext context = ExecutionContext.Capture();
            void ContextCallback(object state) => resultHandler.Invoke();
            task.ContinueWith(t => ExecutionContext.Run(context, ContextCallback, null));
        }

        public void Restore(Action<WayspotAnchorPayload[]> resultHandler) {
            Task<WayspotAnchorPayload[]> task = payloadStore.Restore();
            ExecutionContext context = ExecutionContext.Capture();
            void ContextCallback(object state) => resultHandler.Invoke((WayspotAnchorPayload[]) state);
            task.ContinueWith(t => ExecutionContext.Run(context, ContextCallback, t.Result));
        }

        public void Clear(Action resultHandler) {
            Task task = payloadStore.Clear();
            ExecutionContext context = ExecutionContext.Capture();
            void ContextCallback(object state) => resultHandler.Invoke();
            task.ContinueWith(t => ExecutionContext.Run(context, ContextCallback, null));
        }
    }
}