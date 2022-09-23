using System.Threading.Tasks;
using UnityEngine;

namespace VPSTour.lib.Async {
    public static class AsyncOperationExtensions {
        public static async Task<T> WaitAsync<T>(this T asyncOperation) where T : AsyncOperation {
            while (!asyncOperation.isDone) {
                await Task.Yield();
            }

            return asyncOperation;
        }
    }
}