using System;
using UniCore.Runtime.ProfilerTools;
using UniGame.Runtime.ProfilerTools;
using UniGame.Core.Runtime;

namespace UniGame.Context.Runtime
{
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using GameFlow.Runtime;
    using UnityEngine;

    public interface IEmptyGameService : IGameService
    {
        
    }

    public abstract class ServiceDataSourceAsset : ServiceDataSourceAsset<IEmptyGameService>
    {
    }
    
        
    public abstract class ServiceDataSourceAsset<TApi> :
        ScriptableObject,
        IAsyncDataSource
        where TApi : class, IGameService
    {
        #region inspector
        
        public bool enabled = true;

        public bool isSharedSystem = true;

        public bool waitServiceReady = false;
        
        public bool ownServiceLifeTime = true;
        
        #endregion

        private        TApi          _sharedService;
        private        SemaphoreSlim _semaphoreSlim;

        #region public methods
    
        public async UniTask<IContext> RegisterAsync(IContext context)
        {
            var lifeTime = context.LifeTime;
            
#if UNITY_EDITOR || GAME_LOGS_ENABLED || DEBUG
            var profileId = ProfilerUtils.BeginWatch($"Service_{typeof(TApi).Name}");
            GameLog.LogRuntime($"GameService Profiler Init : {typeof(TApi).Name} | {DateTime.Now}");
#endif 
            
            var service = await CreateServiceAsync(context)
                .AttachExternalCancellation(lifeTime.Token);

#if UNITY_EDITOR || GAME_LOGS_ENABLED || DEBUG
            var watchResult = ProfilerUtils.GetWatchData(profileId);
            GameLog.LogRuntime($"GameService Profiler Create : {typeof(TApi).Name} | Take {watchResult.watchMs} | {DateTime.Now}");
#endif

#if UNITY_EDITOR || GAME_LOGS_ENABLED || DEBUG
            watchResult = ProfilerUtils.GetWatchData(profileId,true);
            GameLog.LogRuntime($"GameService Profiler Publish: {typeof(TApi).Name} | Take {watchResult.watchMs} | {DateTime.Now}");
#endif
            
            context.Publish(service);
            return context;
        }
    
        /// <summary>
        /// service factory
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async UniTask<TApi> CreateServiceAsync(IContext context)
        {
            var lifeTime = context.LifeTime;
            if (!enabled) await UniTask.Never<TApi>(lifeTime.Token);

            if (isSharedSystem)
            {
                lock (this)
                {
                    _semaphoreSlim ??= new SemaphoreSlim(1,1);
                }
                
                await _semaphoreSlim.WaitAsync(lifeTime.Token);
            
                try {
                    if (isSharedSystem && _sharedService == null) {
                        _sharedService = await CreateServiceInternalAsync(context)
                            .AttachExternalCancellation(lifeTime.Token);
                        if (ownServiceLifeTime)
                        {
                            _sharedService.AddTo(lifeTime);
                        }
                    }
                }
                finally {
                    //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                    //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                    _semaphoreSlim.Release();
                }

                return _sharedService;
            }
            
            var service = ownServiceLifeTime 
                ? (await CreateServiceInternalAsync(context).AttachExternalCancellation(lifeTime.Token)).AddTo(lifeTime)
                : (await CreateServiceInternalAsync(context).AttachExternalCancellation(lifeTime.Token));
            
            return service;
        }

        #endregion

        protected abstract UniTask<TApi> CreateServiceInternalAsync(IContext context);

        private void OnDestroy()
        {
            _semaphoreSlim?.Dispose();
            _sharedService = null;
        }
    }
}