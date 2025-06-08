namespace UniGame.Context.Runtime.DataSources
{
    using UniCore.Runtime.ProfilerTools;
    using Core.Runtime.Extension;
    using Core.Runtime;
    using Runtime;
    using AddressableTools.Runtime;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using Object = UnityEngine.Object;
    
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    
#if UNITY_EDITOR
    using UnityEditor;
#endif
    
    [CreateAssetMenu(menuName = "UniGame/Sources/AddressableAsyncSources", fileName = nameof(AsyncDataSources))]
    public class AsyncDataSources : ScriptableObject, IAsyncDataSource
    {
        #region inspector

        public bool enabled = true;

#if ODIN_INSPECTOR
        [InlineEditor()] 
        [Searchable]
#endif
        public List<ScriptableObject> sources = new List<ScriptableObject>();
        
        [Space]
#if ODIN_INSPECTOR
        [LabelText("Async Sources")]
        [Searchable]
        [ListDrawerSettings(ListElementLabelName = "Name")]
#endif
        public List<AsyncSourceDescription> asyncSources = new List<AsyncSourceDescription>();

        public bool useTimeout = true;

        public float timeOutMs = 60000;

        #endregion

        public async UniTask<IContext> RegisterAsync(IContext context)
        {
            if (enabled == false)
                return context;

            var syncSources = sources
                .Where(x => x is IAsyncDataSource)
                .Select(x => x.ToSharedInstance())
                .Select(x => (x as IAsyncDataSource))
                .Select(x => RegisterContexts(context,x));

            var asyncValues = asyncSources
                .Select(x => RegisterContexts(context, x));
            
            await UniTask.WhenAll(syncSources.Concat(asyncValues));

            return context;
        }

#if UNITY_EDITOR
#if ODIN_INSPECTOR
        [Button]
#endif
        public void Save()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }
#endif
        
        private async UniTask<bool> RegisterContexts(
            IContext target,
            AsyncSourceDescription sourceReference)
        {
            if(sourceReference.enabled == false)
                return true;
            
            var sourceName = name;
            var sourceValue = sourceReference.source;

            GameLog.Log($"SOURCE: RegisterContexts {sourceName} {target.GetType().Name} LIFETIME CONTEXT");

            var source = await sourceValue
                .LoadAssetTaskAsync(target.LifeTime)
                .ToSharedInstanceAsync(target.LifeTime);

            if (source is not IAsyncDataSource asyncSource) return false;

            var isAwaitLoading = sourceReference.awaitLoading;
            var registerTask = RegisterContexts(target, asyncSource);

            if (!isAwaitLoading)
            {
                registerTask
                    .AttachExternalCancellation(target.LifeTime.Token)
                    .Forget();

                return true;
            }

            var result = await registerTask;
            return result;
        }
        
        
        private async UniTask<bool> RegisterContexts(IContext target, IAsyncDataSource source)
        {
            var sourceName = name;

            GameLog.Log($"SOURCE: RegisterContexts {sourceName} {target.GetType().Name} LIFETIME CONTEXT");

            var lifeTime = target.LifeTime;
            var sourceAsset = source as Object;
            var sourceAssetName = sourceAsset == null
                ? source.GetType().Namespace
                : sourceAsset.name;

            var cancellationTokenSource = new CancellationTokenSource();
            
#if DEBUG
            var timer = Stopwatch.StartNew();   
            timer.Restart();
#endif

            if (useTimeout && timeOutMs > 0)
            {
                HandleTimeout(sourceAssetName, cancellationTokenSource.Token)
                    .AttachExternalCancellation(cancellationTokenSource.Token)
                    .SuppressCancellationThrow()
                    .Forget();
            }

            await source.RegisterAsync(target)
                .AttachExternalCancellation(lifeTime.Token);

#if DEBUG
            var elapsed = timer.ElapsedMilliseconds;
            timer.Stop();
            GameLog.LogRuntime($"SOURCE: LOAD TIME {sourceAssetName} = {elapsed} ms");
#endif
            
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            
            GameLog.LogRuntime($"SOURCE: {sourceName} : REGISTER SOURCE {sourceAssetName}", Color.green);
            
            return true;
        }

        private async UniTask HandleTimeout(string assetName, CancellationToken cancellationToken)
        {
            if (!useTimeout || timeOutMs <= 0)
                return;

            var assetSourceName = name;

            await UniTask.Delay(TimeSpan.FromMilliseconds(timeOutMs), cancellationToken: cancellationToken)
                .AttachExternalCancellation(cancellationToken);

            GameLog.LogError($"SOURCE: {assetSourceName} : REGISTER SOURCE TIMEOUT {assetName}");
        }

        protected void OnDestroy() => GameLog.Log($"DESTROY {name}",Color.yellow);
        
    }

    [Serializable]
    public class AsyncSourceDescription
#if ODIN_INSPECTOR
        : ISearchFilterable
#endif
    {
        public bool enabled = true;
        public bool awaitLoading = true;
        
#if ODIN_INSPECTOR
        [DrawWithUnity]
#endif
        public AssetReferenceDataSource source;
        
        public string Name {

            get
            {
#if UNITY_EDITOR
                return source == null || source.editorAsset == null 
                    ? string.Empty
                    : source.editorAsset.name;
#endif
                return string.Empty;
            }
            
        }

        public bool IsMatch(string searchString)
        {
            if (string.IsNullOrEmpty(searchString)) return true;
            return Name.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}