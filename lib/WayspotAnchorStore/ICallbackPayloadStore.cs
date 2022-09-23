using System;
using Niantic.ARDK.AR.WayspotAnchors;

namespace VPSTour.lib.WayspotAnchorStore {
    /// <summary>
    /// This store allows developers to Persist <see cref="WayspotAnchorPayload"/> after a
    /// wayspot anchor has been created with <see cref="WayspotAnchorService"/>.
    /// 
    /// Once persisted, these payloads can be restored via <see cref="ICallbackPayloadStore.RestorePayloads"/>.
    /// This should allow developers to call <see cref="WayspotAnchorService.Restore"/> with the
    /// given payloads.
    ///
    /// For simplicity, this store assumes 1 single collection with all payloads in it.
    /// Payloads will be incrementally added to this collection.
    /// 
    /// This interface exposes the callback oriented methods.
    /// For Task based methods (using async / await). One can use <see cref="IPayloadStore"/>.
    /// (recommended)
    /// </summary>
    public interface ICallbackPayloadStore {
        void Persist(IWayspotAnchor[] wayspotAnchors, Action resultHandler);
        void Restore(Action<WayspotAnchorPayload[]> resultHandler);
        void Clear(Action resultHandler);
    }
}