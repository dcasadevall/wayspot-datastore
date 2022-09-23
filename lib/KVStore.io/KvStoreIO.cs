using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using Niantic.ARDK.Utilities;
using UnityEngine;
using UnityEngine.Networking;
using VPSTour.lib.Async;

namespace VPSTour.lib.KVStore.io {
    public class KvStoreIO : IKvStore {
        private const string CollectionName = "wayspot_anchors";
        private const string CollectionCreateUri = "https://api.kvstore.io/collections";
        private const string KeyValueUriFormat = "https://api.kvstore.io/collections/{0}/items/{1}";
        
        private string apiKey;

        public KvStoreIO(string apiKey) {
            this.apiKey = apiKey;
        }

        /// <summary>
        /// Initializes the KVStore.IO collection if needed
        /// </summary>
        public async void Init() {
            var result = await PutText(CollectionCreateUri,
                                       $"{{'collection' : '{CollectionName}'}}");
            
            if (!result) {
                throw new Exception("Unable to create KVStore collection");
            }
        }

        /// <inheritdoc cref="IKvStore.GetValue(string)"/>
        public Task<string> GetValue(string key) {
            var uri = string.Format(KeyValueUriFormat, CollectionName, key);
            return GetRequest(uri);
        }

        /// <inheritdoc cref="IKvStore.SetValue(string, string)"/>
        public Task SetValue(string key, string value) {
            var uri = string.Format(KeyValueUriFormat, CollectionName, key);
            return PostText(uri, value);
        }

        /// <summary>
        /// Perform a POST request of the given text
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="text"></param>
        /// <returns>True if successful. False otherwise</returns>
        private async Task<bool> PostText(string uri, string text) {
            var textBytes = Encoding.UTF8.GetBytes(text);

            using (var request = new UnityWebRequest(uri, "POST")) {
                request.uploadHandler = new UploadHandlerRaw(textBytes);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("kvstoreio_api_key", apiKey);

                await request.SendWebRequest().WaitAsync();

                var result = HandleResult(request);
                return result != null;
            }
        }

        /// <summary>
        /// Perform a PUT request of the given text
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="text"></param>
        /// <returns>true if successful. False otherwise</returns>
        private async Task<bool> PutText(string uri, string text) {
            var textBytes = Encoding.UTF8.GetBytes(text);

            using (var request = new UnityWebRequest(uri, "PUT")) {
                request.uploadHandler = new UploadHandlerRaw(textBytes);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "text/plain");
                request.SetRequestHeader("kvstoreio_api_key", apiKey);

                await request.SendWebRequest().WaitAsync();

                var result = HandleResult(request);
                return result != null;
            }
        }

        /// <summary>
        /// Perform a GET request with the given uri
        /// </summary>
        /// <param name="uri"></param>
        /// <returns>The response text if successful, or null otherwise</returns>
        private async Task<string> GetRequest(string uri) {
            using (var request = UnityWebRequest.Get(uri)) {
                await request.SendWebRequest().WaitAsync();

                return HandleResult(request);
            }
        }

        /// <summary>
        /// Handle any errors in the given finished request and return the string (or null on error)
        /// of the response value
        /// </summary>
        /// <param name="request"></param>
        /// <returns>the response string if successful. Null otherwise.</returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private string HandleResult(UnityWebRequest request) {
            switch (request.result) {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError("Error: " + request.error);
                    return null;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("HTTP Error: " + request.error);
                    return null;
                case UnityWebRequest.Result.Success:
                    // Uncomment for debugging purposes
                    // Debug.Log("Received: " + request.downloadHandler.text);
                    return request.downloadHandler.text;
                case UnityWebRequest.Result.InProgress:
                    throw new Exception("Result not available");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}