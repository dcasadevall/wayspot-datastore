using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

namespace VPSTour.lib.KVStore.Async {
    /// <summary>
    /// Provides an <see cref="INotifyCompletion"/> implementing wrapper for <see cref="AsyncOperation"/>.
    /// This allows us to use <see cref="AsyncOperation"/> with the Task system
    /// 
    /// From: https://github.com/CrazyPandaLimited/SimplePromises
    /// </summary>
    [DebuggerNonUserCode]
    public readonly struct AsyncOperationAwaiter : INotifyCompletion {
        private readonly AsyncOperation _asyncOperation;
        public bool IsCompleted => _asyncOperation.isDone;

        public AsyncOperationAwaiter(AsyncOperation asyncOperation) =>
            _asyncOperation = asyncOperation;

        public void OnCompleted(Action continuation) =>
            _asyncOperation.completed += _ => continuation();

        public void GetResult() {
        }
    }

    /// <summary>
    /// Provides an <see cref="INotifyCompletion"/> implementing wrapper for <see cref="UnityWebRequestAsyncOperation"/>.
    /// This allows us to use <see cref="UnityWebRequestAsyncOperation"/> with the Task system
    /// 
    /// From: https://github.com/CrazyPandaLimited/SimplePromises
    /// </summary>
    [DebuggerNonUserCode]
    public readonly struct UnityWebRequestAwaiter : INotifyCompletion {
        private readonly UnityWebRequestAsyncOperation _asyncOperation;

        public bool IsCompleted => _asyncOperation.isDone;

        public UnityWebRequestAwaiter(UnityWebRequestAsyncOperation asyncOperation) =>
            _asyncOperation = asyncOperation;

        public void OnCompleted(Action continuation) =>
            _asyncOperation.completed += _ => continuation();

        public UnityWebRequest GetResult() => _asyncOperation.webRequest;
    }
}