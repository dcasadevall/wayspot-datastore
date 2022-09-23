namespace VPSTour.lib.KVStore.KVStoreIo {
    /// <summary>
    /// Class used to deserialize the json response from kvstore.io
    /// for /collections/{collection}/items
    /// </summary>
    public class JsonKeyValue {
        public string key;
        public string value;
    }
}