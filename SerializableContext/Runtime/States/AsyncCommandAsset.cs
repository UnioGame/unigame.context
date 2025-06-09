namespace UniGame.Context.Runtime.Runtime.States
{
    using global::UniGame.Core.Runtime;
    using Cysharp.Threading.Tasks;
    using UnityEngine;

    public abstract class AsyncCommandAsset<TData, TValue> :
        ScriptableObject, 
        IAsyncCommand<TData,TValue>
    {
        public abstract UniTask<TValue> ExecuteAsync(TData value);
    }
}