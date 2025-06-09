namespace UniGame.Context.Runtime.Runtime.States
{
    using System;
    using System.Threading;
    using Context.Runtime;
    using global::UniGame.Core.Runtime;
    using Cysharp.Threading.Tasks;
    using global::UniGame.Runtime.DataFlow;

    [Serializable]
    public abstract class AsyncSharedContextState<TValue> : IAsyncContextState<TValue>
    {
        private LifeTimeDefinition _lifeTime;
        private SemaphoreSlim      _semafore;
        private bool               _isActive;
        private UniTask<TValue>    _taskHandle;

        private SemaphoreSlim semaphoreSlim => _semafore = (_semafore ?? new SemaphoreSlim(1, 1));
        
        public async UniTask<TValue> ExecuteAsync(IContext value)
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                if (!_isActive) {
                    
                    _isActive = true;
                    var contextLifetime = value.
                        LifeTime.
                        Compose(LifeTime);
                    
                    _taskHandle = OnExecute(value, contextLifetime);
                }
            }
            finally
            {
                //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                semaphoreSlim.Release();
            }
            
            var result = await _taskHandle;
            
            return result;
        }

        public async UniTask ExitAsync() {

            if (!_isActive)
                return;

            await OnExit();
            
            _isActive = false;

            _lifeTime?.Release();
            
        }

        public ILifeTime LifeTime => _lifeTime = (_lifeTime ?? new LifeTimeDefinition());

        public bool IsActive => _isActive;

        protected abstract UniTask<TValue> OnExecute(IContext context, ILifeTime executionLifeTime);

        protected virtual async UniTask OnExit() { }
    }
}