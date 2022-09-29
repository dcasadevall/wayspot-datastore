# Wayspot Anchor Datastore

Demo project showcasing how to store [lightship VPS](https://lightship.dev/docs/ardk/vps/index.html) wayspot anchors using KvStore.io.

# KVStore.io

This sample library uses [KVStore.io](https://kvstore.io) to store data remotely.
You will need to create an account there and write down the API Key, which
you can generate from the dashboard.

# Setup

* Create a new Unity project, or use an existing Lightship Project that has all the "Quick Start" setup
  steps already performed (Feel free to use https://github.com/dcasadevall/lightship-template)
* Clone the repository and copy its contents anywhere in your Unity project
* With unity open when working on your Unity project, right click on your Project Assets view, then 
click `Create -> VPSTour -> KVStoreConfig`
* Enter your API Key in the newly created scriptable object field `Api Key` 

You are now ready to use the `IPayloadStore`.

# IPayloadStore

We use `IPayloadStore` to Persist / Restore wayspot anchors:

```c#
public interface IPayloadStore {
    Task Persist(IWayspotAnchor[] wayspotAnchors);
    Task<WayspotAnchorPayload[]> Restore();
    Task Clear();
}
```

Note that if you are not familiar with `Task`s and do not want to ramp up on them
at this point, you can use the `ICallbackPayloadStore` instead.

# Obtaining a store instance

For simplicity, we provide with a `Providers` class that provides with Singleton instances
of `IPayloadStore` and `ICallbackPayloadStore`.

If your project uses Dependency Injection, you can construct these dependencies yourself.
Implementation specific classes have been left public (not internal) for that purpose.

# Persisting / Restoring Wayspot Anchors

After creating wayspot anchors in your application, you can persist them via `IPayloadStore.Persist()`
and restore them at any point in time with `IPayloadStore.Restore()`

# Sample

This repository comes with a modified version of the ARDK sample `WayspotAnchorSample` in `Demo/Scenes/WayspotAnchorStore`.
The class `WayspotStoreExampleManager` is a carbon copy of `WayspotAnchorExampleManager`, except for the places
commented with `Store Logic`. This should give a good idea of where to use the `IPayloadStore`

# Using another remote Datastore

If you wanted to use something like Firebase rather than `KVStore.io`, you could create a different implementation of
`IKVStore` that connects to that service. You would then change the bindings in `Providers.cs` to use your new implementation
when providing the `IKVStore` to the payload store.
TODO
