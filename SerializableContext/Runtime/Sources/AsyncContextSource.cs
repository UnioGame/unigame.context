namespace UniGame.Context.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using AddressableTools.Runtime;
    using Core.Runtime;
    using Core.Runtime.Extension;
    using Cysharp.Threading.Tasks;
    using UniCore.Runtime.ProfilerTools;
    using UnityEngine;
    using Object = UnityEngine.Object;

#if ODIN_INSPECTOR
    using Sirenix.Utilities.Editor;
    using Sirenix.OdinInspector;
#endif

#if UNITY_EDITOR
    using UniModules.Editor;
    using UnityEditor;
#endif
    
    
    [Serializable]
    public class AsyncContextSource : IAsyncDataSource
    {
        #region inspector

        public bool enabled = true;

        public string name;
        
        [Space]
#if ODIN_INSPECTOR
        [LabelText("Async Sources")]
        [Searchable]
        [ListDrawerSettings(ListElementLabelName = "Name",OnEndListElementGUI = nameof(EndDrawListElement))]
#endif
        public List<AsyncSourceDescription> asyncSources = new();

        public bool useTimeout = true;

        public float timeOutMs = 60000;

        #endregion

        public async UniTask<IContext> RegisterAsync(IContext context)
        {
            if (enabled == false)
                return context;

            var asyncValues = asyncSources
                .Select(x => RegisterContexts(context, x));

            await UniTask.WhenAll(asyncValues);

            return context;
        }

        private async UniTask<bool> RegisterContexts(IContext target, AsyncSourceDescription sourceReference)
        {
            if (sourceReference.enabled == false)
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

            GameLog.Log($"SOURCE: {sourceName} : REGISTER SOURCE {sourceAssetName}", Color.green);

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
        
        private void EndDrawListElement(int index)
        {
#if UNITY_EDITOR
            var source = asyncSources[index];
            var sourceAsset = source.source.editorAsset;
            if (sourceAsset == null) return;
            
            if (!SirenixEditorGUI.Button("open", ButtonSizes.Medium)) return;
            
            var type = sourceAsset.GetType();
            type.OpenEditorScript();
#endif
        }
        
    }
}