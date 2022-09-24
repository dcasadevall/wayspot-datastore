using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using VPSTour.lib.KVStore.Async;

namespace VPSTour.lib.KVStore.KVStoreIo {
    /// <summary>
    /// <see cref="IKvStore"/> Implementation that uses KVStore.io REST API
    /// </summary>
    public class KvStoreIO : IKvStore {
        private const string CollectionName = "wayspot_anchors";
        private const string CollectionsUri = "https://api.kvstore.io/collections";
        private const string CollectionFormat = "https://api.kvstore.io/collections/{0}";
        private const string KeyValueUriFormat = "https://api.kvstore.io/collections/{0}/items/{1}";
        private const string GetValuesUriFormat = "https://api.kvstore.io/collections/{0}/items";
        private const string CollectionsJsonKey = "collections";
        
        private string apiKey;
        private bool isInit;

        public KvStoreIO(string apiKey) {
            if (string.IsNullOrEmpty(apiKey)) {
                throw new Exception("Empty API Key");
            }
            
            this.apiKey = apiKey;
        }

        /// <summary>
        /// Initializes the KVStore.IO collection if needed
        /// </summary>
        private async Task Init() {
            var collectionExists = await CollectionExists();
            if (collectionExists) {
                isInit = true;
                return;
            }
            
            await PostJson(CollectionsUri, $"{{\"collection\" : \"{CollectionName}\"}}");
            isInit = true;
        }

        /// <inheritdoc cref="IKvStore.GetValue(string)"/>
        public async Task<string> GetValue(string key) {
            if (!isInit) {
                await Init();
            }
            
            var uri = string.Format(KeyValueUriFormat, CollectionName, key);
            var responseJson = await GetRequest(uri);
            var response = SimpleJSON.JSON.Parse(responseJson);
            return response["value"];
        }

        public async Task<IEnumerable<KeyValuePair<string, string>>> GetValues() {
            if (!isInit) {
                await Init();
            }
            
            var uri = string.Format(GetValuesUriFormat, CollectionName);
            var responseJson = await GetRequest(uri);
            var response = SimpleJSON.JSONNode.Parse(responseJson);
            
            return response.Children.Select(x => new KeyValuePair<string, string>(x["key"], x["value"]));
        }

        /// <inheritdoc cref="IKvStore.SetValue(string, string)"/>
        public async Task SetValue(string key, string value) {
            if (!isInit) {
                await Init();
            }
            
            var uri = string.Format(KeyValueUriFormat, CollectionName, key);
            await PutText(uri, value);
        }

        public async Task DeleteAllKeys() {
            // This does perform an extra RPC
            // Not the most efficient but works for prototyping
            var collectionExists = await CollectionExists();
            if (!collectionExists) {
                return;
            }
            
            var uri = string.Format(CollectionFormat, CollectionName);
            await DeleteRequest(uri); 
            
            // Set init back to false so the collection is created again when handling keys
            isInit = false;
        }

        /// <summary>
        /// Perform a POST request of the given text
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="text"></param>
        private async Task PostJson(string uri, string text) {
            // Posting Json requires that escaping is respected.
            // We have to use our own encoding (UnityWebRequest.Post()) urlencodes
            // the text for us, which we don't want
            var textBytes = Encoding.UTF8.GetBytes(text);

            using (var request = new UnityWebRequest(uri, "POST")) {
                request.uploadHandler = new UploadHandlerRaw(textBytes);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("kvstoreio_api_key", apiKey);

                await request.SendWebRequest().WaitAsync();

                HandleResult(request);
            }
        }

        /// <summary>
        /// Perform a PUT request of the given text
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="text"></param>
        private async Task PutText(string uri, string text) {
            using (var request = UnityWebRequest.Put(uri, text)) {
                request.SetRequestHeader("Content-Type", "text/plain");
                request.SetRequestHeader("kvstoreio_api_key", apiKey);

                await request.SendWebRequest().WaitAsync();

                HandleResult(request);
            }
        }
        
        /// <summary>
        /// Perform a DELETE request at the given uri
        /// </summary>
        /// <param name="uri"></param>
        private async Task DeleteRequest(string uri) {
            using (var request = UnityWebRequest.Delete(uri)) {
                request.SetRequestHeader("Content-Type", "text/plain");
                request.SetRequestHeader("kvstoreio_api_key", apiKey);

                await request.SendWebRequest().WaitAsync();

                HandleResult(request);
            }
        }

        /// <summary>
        /// Perform a GET request with the given uri
        /// </summary>
        /// <param name="uri"></param>
        /// <returns>The response text if successful, or null otherwise</returns>
        private async Task<string> GetRequest(string uri) {
            using (var request = UnityWebRequest.Get(uri)) {
                request.SetRequestHeader("kvstoreio_api_key", apiKey);
                await request.SendWebRequest().WaitAsync();

                return HandleResult(request);
            }
        }

        /// <summary>
        /// Handle any errors in the given finished request and return the string
        /// of the response value
        /// </summary>
        /// <param name="request"></param>
        /// <returns>the response string</returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private string HandleResult(UnityWebRequest request) {
            switch (request.result) {
                case UnityWebRequest.Result.Success:
                    return request.downloadHandler?.text;
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    throw new Exception($"{request.uri}. Error: {request.error}");
                case UnityWebRequest.Result.ProtocolError:
                    throw new Exception($"{request.uri}. HTTP Error: {request.error}");
                case UnityWebRequest.Result.InProgress:
                    throw new Exception("Result not available");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task<bool> CollectionExists() {
            var responseJson = await GetRequest(CollectionsUri);

            var response = SimpleJSON.JSON.Parse(responseJson);
            return response.HasKey(CollectionsJsonKey) && response[CollectionsJsonKey].HasKey(CollectionName);
        }
    }
}