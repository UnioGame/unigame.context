using UniCore.Runtime.ProfilerTools;
using UniGame.Runtime.ProfilerTools;
using UniGame.Core.Runtime;

namespace UniGame.Context.Runtime
{
    using System;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using Sirenix.OdinInspector;
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif
    
    public abstract class DataSourceAsset<TApi> :
        ScriptableObject,
        IAsyncDataSource
    {
        #region inspector

        public bool enabled = true;

        public bool isSharedSystem = true;

        public bool ownDataLifeTime = true;

        #endregion

        private static TApi _sharedValue;
        private SemaphoreSlim _semaphoreSlim;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Reset()
        {
            if(_sharedValue is IDisposable disposable)
                disposable.Dispose();
            _sharedValue = default;
        }
        
        #region public methods

        public async UniTask<IContext> RegisterAsync(IContext context)
        {
            var lifeTime = context.LifeTime;
            
            if (!enabled) return context;

#if UNITY_EDITOR || GAME_LOGS_ENABLED || DEBUG
            var profileId = ProfilerUtils.BeginWatch($"Service_{typeof(TApi).Name}");
            GameLog.Log($"GameService Profiler Init : {typeof(TApi).Name} | {DateTime.Now}");
#endif

            var result = await CreateAsync(context).AttachExternalCancellation(lifeTime.Token);

#if UNITY_EDITOR || GAME_LOGS_ENABLED || DEBUG
            var watchResult = ProfilerUtils.GetWatchData(profileId);
            GameLog.Log($"GameService Profiler Create : {typeof(TApi).Name} | Take {watchResult.watchMs} | {DateTime.Now}");
#endif

            context.Publish(result);
            return context;
        }

        /// <summary>
        /// service factory
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async UniTask<TApi> CreateAsync(IContext context)
        {
            var lifeTime = context.LifeTime;

            if (isSharedSystem)
            {
                lock(this)
                {
                    _semaphoreSlim ??= new SemaphoreSlim(1, 1);
                }
                
                await _semaphoreSlim.WaitAsync(lifeTime.Token);
                try
                {
                    if (isSharedSystem && _sharedValue == null)
                    {
                        _sharedValue = await CreateInternalAsync(context)
                            .AttachExternalCancellation(lifeTime.Token);
                    
                        if (_sharedValue is IDisposable disposable && ownDataLifeTime)
                            disposable.AddTo(lifeTime);
                    }
                }
                finally
                {
                    //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                    //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                    _semaphoreSlim.Release();
                }
                
                return _sharedValue;
            }

            var value = await CreateInternalAsync(context)
                .AttachExternalCancellation(lifeTime.Token);

            if (value is IDisposable disposableValue && ownDataLifeTime)
                disposableValue.AddTo(lifeTime);

            return value;
        }

        #endregion

        private void OnDestroy()
        {
            _semaphoreSlim?.Dispose();
            _semaphoreSlim = null;
        }
        
        protected abstract UniTask<TApi> CreateInternalAsync(IContext context);

#if ODIN_INSPECTOR
        [Button]
#endif
        public void Save()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
    }
}