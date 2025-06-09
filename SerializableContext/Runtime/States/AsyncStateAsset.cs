namespace UniGame.Context.Runtime.Runtime.States
{
    using System;
    using global::UniGame.Core.Runtime;
    using Cysharp.Threading.Tasks;
    using global::UniGame.Runtime.DataFlow;

    [Serializable]
    public abstract class AsyncStateAsset<TData,TValue> : 
        AsyncCommandAsset<TData,TValue>, 
        IAsyncStateCommand<TData,TValue>,
        IAsyncState<TData,TValue> ,
        IAsyncCompletion<TValue, TData>,
        IAsyncEndPoint<TData>  ,
        IAsyncRollback<TData>  
    {
        private AsyncStateProxyValue<TData, TValue> _asyncStateProxyValue;
        private LifeTime _lifeTime = new();
        
        public bool IsActive => _asyncStateProxyValue.IsActive;

        public ILifeTime LifeTime => _lifeTime;
        
        #region public methods

        public sealed override async UniTask<TValue> ExecuteAsync(TData value)
        {
            _lifeTime ??= new LifeTime();
            _asyncStateProxyValue ??= new AsyncStateProxyValue<TData, TValue>(this,this,this,this);
            var result = await _asyncStateProxyValue.ExecuteAsync(value);
            return result;
        }

        public async UniTask ExitAsync() => await _asyncStateProxyValue.ExitAsync();

        public virtual UniTask CompleteAsync(TValue value, TData data, ILifeTime lifeTime) => UniTask.CompletedTask;

        public virtual UniTask ExitAsync(TData data) => UniTask.CompletedTask;

        public virtual UniTask Rollback(TData source) => UniTask.CompletedTask;

        public virtual UniTask<TValue> ExecuteStateAsync(TData value) => UniTask.FromResult<TValue>(default);

        #endregion

    }

}