using System.Threading.Tasks;
using UnityEngine;

namespace VPSTour.lib.KVStore.Async {
    public static class AsyncOperationExtensions {
        public static async Task WaitAsync<T>(this T asyncOperation) where T : AsyncOperation {
            while (!asyncOperation.isDone) {
                await Task.Yield();
            }
        }
    }
}