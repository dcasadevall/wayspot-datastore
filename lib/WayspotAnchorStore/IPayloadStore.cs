using System.Threading.Tasks;
using Niantic.ARDK.AR.WayspotAnchors;

namespace VPSTour.lib.WayspotAnchorStore {
    /// <summary>
    /// This store allows developers to Persist <see cref="WayspotAnchorPayload"/> after a
    /// wayspot anchor has been created with <see cref="WayspotAnchorService"/>.
    /// 
    /// Once persisted, these payloads can be restored via <see cref="ICallbackPayloadStore.Restore"/>.
    /// This should allow developers to call <see cref="WayspotAnchorService.RestoreWayspotAnchors"/> with the
    /// given payloads.
    ///
    /// For simplicity, this store assumes 1 single collection with all payloads in it.
    /// Payloads will be incrementally added to this collection.
    /// 
    /// This interface exposes the Task oriented methods. It should be favored over
    /// <see cref="ICallbackPayloadStore"/> if one is familiar with Tasks and async / await.
    /// </summary>
    public interface IPayloadStore {
        Task Persist(IWayspotAnchor[] wayspotAnchors);
        Task<WayspotAnchorPayload[]> Restore();
        Task Clear();
    }
}