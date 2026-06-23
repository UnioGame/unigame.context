using UniCore.Runtime.ProfilerTools;
using UniGame.Runtime.ProfilerTools;
using UniGame.Core.Runtime;

namespace UniGame.Context.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using UniGame.Runtime.ObjectPool;
    using UnityEngine;
    using UnityEngine.Scripting;

#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    
#if UNITY_EDITOR
    using UnityEditor;
#endif
    
    [Preserve]
    public abstract class DataSourceAsset :
        ScriptableObject,
        IAsyncDataSource
    {
        public bool enabled = true;
        
        #region public methods

        public async UniTask<IContext> RegisterAsync(IContext context)
        {
            var lifeTime = context.LifeTime;

            if (!enabled) return context;
            
            var dependencies = await GetDependencies(context);
            var dependencyTasks = dependencies.Select(x => x.Resolve(context));

            var dependencyResults = await UniTask
                .WhenAll(dependencyTasks)
                .AttachExternalCancellation(lifeTime.Token);

            var resolved = true;
            
            foreach (var dataResolveResult in dependencyResults)
            {
                if(dataResolveResult.success) continue;
                resolved = false;
                break;
            }
            
            if (!resolved)
            {
                PrintResolveResult(dependencyResults);
                return context;
            }
            
#if UNITY_EDITOR || GAME_LOGS_ENABLED || GAME_DEBUG
            var profileId = ProfilerUtils.BeginWatch($"Service_{name}");
            GameLog.Log($"[Data Source] Init : {name} | {DateTime.Now}");
#endif
            
            await OnRegisterAsync(context)
                .AttachExternalCancellation(lifeTime.Token);

#if UNITY_EDITOR || GAME_LOGS_ENABLED || GAME_DEBUG
            var watchResult = ProfilerUtils.GetWatchData(profileId);
            GameLog.Log($"[Data Source] : {name} | Take {watchResult.watchMs} | {DateTime.Now}", Color.green);
#endif

            return context;
        }
        
        public virtual void ResetSource() {}

        #endregion
        
        private void PrintResolveResult(DataResolveResult[] results)
        {
            var stringBuilder = ClassPool.Spawn<StringBuilder>();
            
            stringBuilder.AppendLine($"[Data Source] {GetType().Name} failed to resolve dependencies:");
            var haveErrors = false;
            
            foreach (var dataResolveResult in results)
            {
                if(dataResolveResult.success) continue;
                haveErrors = true;
                stringBuilder.AppendLine($"\tfailed to resolve: [{dataResolveResult.message}]");
            }

            if (haveErrors)
            {
                Debug.LogError(stringBuilder);
            }
            
            stringBuilder.Clear();
            ClassPool.Despawn(stringBuilder);
        }

        protected virtual async UniTask<IEnumerable<IDataSourceDependency>> GetDependencies(IContext context)
        {
            return Enumerable.Empty<IDataSourceDependency>();
        }

        protected abstract UniTask<IContext> OnRegisterAsync(IContext context);

        private void OnDestroy()
        {
            ResetSource();
        }
        
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
    
    
    [Preserve]
    public abstract class DataSourceAsset<TApi> : DataSourceAsset
    {
        #region inspector
        
        public bool isSharedSystem = true;

        #endregion

        private TApi _sharedValue;
        private SemaphoreSlim _semaphoreSlim;
        
        #region public methods

        public override void ResetSource()
        {
            var value = _sharedValue;
            _sharedValue = default;
            if(value is IDisposable disposable)
                disposable.Dispose();
        }

        
        protected override async UniTask<IContext> OnRegisterAsync(IContext context)
        {
            var lifeTime = context.LifeTime;
            var result = await CreateAsync(context).AttachExternalCancellation(lifeTime.Token);
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

                        lifeTime.AddCleanUpAction(ResetSource);
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

            if (value is IDisposable disposableValue)
                disposableValue.AddTo(lifeTime);

            return value;
        }

        #endregion

        private void OnDestroy()
        {
            ResetSource();
            _semaphoreSlim?.Dispose();
            _semaphoreSlim = null;
        }
        
        protected abstract UniTask<TApi> CreateInternalAsync(IContext context);

#if ODIN_INSPECTOR
        [Button]
#endif
        public void SaveAsset()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
    }

    [Serializable]
    public class DataSourceDependency<TData> : IDataSourceDependency
    {
        public int timeOutMs = 5000;
        public int runtimeTimeoutMs = 50000;
        
        public async UniTask<DataResolveResult> Resolve(IContext context)
        {
            var lifeTime = context.LifeTime;
            
#if GAME_DEBUG || GAME_LOGS_ENABLED
            var startTime = Time.realtimeSinceStartup;
#endif
            
            var task = context.GetAsync<TData>();
            var duration = 0f;

            var timeout = Application.isEditor ? timeOutMs : runtimeTimeoutMs;
            var timeoutTask = task.TimeoutWithoutException(TimeSpan.FromMilliseconds(timeout));
            var dependency = await timeoutTask
                .AttachExternalCancellation(lifeTime.Token);
            
#if GAME_DEBUG || GAME_LOGS_ENABLED
            var finishTime = Time.realtimeSinceStartup;
            duration = finishTime - startTime;
#endif
            
            var success = !dependency.IsTimeout;
            var message = success ? string.Empty : $"{typeof(TData).Name} was not resolved in {timeOutMs} ms";

#if GAME_DEBUG || GAME_LOGS_ENABLED
            if (!success)
            {
                Debug.LogError($"[Data Source] {message}");
            }
#endif
            
            var result = new DataResolveResult()
            {
                success = success,
                time = duration,
                message = message,
            };

            return result;
        }
    }
    
    public interface IDataSourceDependency
    {
        UniTask<DataResolveResult> Resolve(IContext context);
    }

    [Serializable]
    public struct DataResolveResult
    {
        public bool success;
        public float time;
        public string message;
    }

}