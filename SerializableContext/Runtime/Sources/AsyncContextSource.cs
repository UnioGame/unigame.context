namespace UniGame.Context.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using AddressableTools.Runtime;
    using Core.Runtime;
    using Core.Runtime.Extension;
    using Cysharp.Threading.Tasks;
    using UniCore.Runtime.ProfilerTools;
    using UnityEngine;
    using Object = UnityEngine.Object;

#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    
#if UNITY_EDITOR
#if ODIN_INSPECTOR
    using Sirenix.Utilities.Editor;
#endif
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
        [Searchable(FilterOptions = SearchFilterOptions.ISearchFilterableInterface)]
        [ListDrawerSettings(ListElementLabelName = "Name",OnEndListElementGUI = nameof(EndDrawListElement))]
#endif
        public List<AsyncSourceDescription> asyncSources = new();

        public bool useTimeout = true;

        public float timeOutMs = 60000;
        public float editorTimeOutMs = 10000;

        #endregion

        private List<List<AsyncSourceDescription>> loadingPipeline = new();
        private List<UniTask> loadingTasks = new();

        public async UniTask<IContext> RegisterAsync(IContext context)
        {
            if (enabled == false) return context;

            loadingPipeline.Clear();
            List<AsyncSourceDescription> activeStep = new();
            
            foreach (var description in asyncSources)
            {
                if(description.enabled == false) continue;
                
                if (description.awaitLoading)
                {
                    if (activeStep.Count > 0)
                    {
                        loadingPipeline.Add(activeStep);
                        activeStep = new List<AsyncSourceDescription>();
                        activeStep.Add(description);
                        loadingPipeline.Add(activeStep);
                    }
                    else
                    {
                        activeStep.Add(description);
                        loadingPipeline.Add(activeStep);
                    }
                    
                    activeStep = new List<AsyncSourceDescription>();
                }
                else
                {
                    activeStep.Add(description);
                }
            }
            
            loadingPipeline.Add(activeStep);

            foreach (var step in loadingPipeline)
                await InitializeServiceStep(context, step);

            return context;
        }

        private async UniTask<bool> InitializeServiceStep(IContext context, List<AsyncSourceDescription> step)
        {
            var asyncValues = step.Select(x => RegisterContexts(context, x));
            var result = await UniTask.WhenAll(asyncValues);
            var success = result.All(x => x);
            return success;
        }
        
        private async UniTask<bool> RegisterContexts(IContext context, AsyncSourceDescription sourceReference)
        {
            if (sourceReference.enabled == false) return true;

            var sourceName = name;
            var sourceValue = sourceReference.source;

            GameLog.Log($"GAME CONTEXT [{name}]: RegisterContexts {sourceName} {context.GetType().Name} Source name: {sourceReference.Name}");

            var source = await sourceValue
                .LoadAssetTaskAsync(context.LifeTime)
                .ToSharedInstanceAsync(context.LifeTime);

            if (source is not IAsyncDataSource asyncSource) return false;

            var registerTask = RegisterContexts(context, asyncSource);

            var result = await registerTask;
            return result;
        }


        private async UniTask<bool> RegisterContexts(IContext target, IAsyncDataSource source)
        {
            var sourceName = name;

            GameLog.Log($"GAME CONTEXT [{name}]: RegisterContexts {sourceName} {target.GetType().Name} LIFETIME CONTEXT",Color.yellow);

            var lifeTime = target.LifeTime;
            var sourceAsset = source as Object;
            var sourceAssetName = sourceAsset == null
                ? source.GetType().Namespace
                : sourceAsset.name;

            var cancellationTokenSource = new CancellationTokenSource();

#if GAME_DEBUG || UNITY_EDITOR
            var timer = Stopwatch.StartNew();
            timer.Restart();
#endif

            var timeOut = Application.isEditor
                ? editorTimeOutMs
                : timeOutMs;
            
            if (useTimeout && timeOut > 0)
            {
                HandleTimeout(sourceAssetName, cancellationTokenSource.Token)
                    .SuppressCancellationThrow()
                    .Forget();
            }

            await source.RegisterAsync(target)
                .AttachExternalCancellation(lifeTime.Token);

#if GAME_DEBUG || UNITY_EDITOR
            var elapsed = timer.ElapsedMilliseconds;
            timer.Stop();
            GameLog.LogRuntime($"GAME CONTEXT [{name}]: LOAD TIME {sourceAssetName} = {elapsed} ms");
#endif

            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();

            GameLog.Log($"GAME CONTEXT [{name}]: {sourceName} : REGISTER SOURCE {sourceAssetName}", Color.green);

            return true;
        }

        private async UniTask HandleTimeout(string assetName, CancellationToken cancellationToken)
        {
            var assetSourceName = name;
            
            var timeOut = Application.isEditor
                ? editorTimeOutMs
                : timeOutMs;
            
            if (!useTimeout || timeOut <= 0) return;
            
            await UniTask.Delay(TimeSpan.FromMilliseconds(timeOut), cancellationToken: cancellationToken);

            GameLog.LogError($"GAME CONTEXT [{name}]: {assetSourceName} : TIMEOUT {assetName}");
        }
        
        private void EndDrawListElement(int index)
        {
#if UNITY_EDITOR
            var source = asyncSources[index];
            var sourceAsset = source.source.editorAsset;
            if (sourceAsset == null) return;

#if ODIN_INSPECTOR
            if (!SirenixEditorGUI.Button("open", ButtonSizes.Medium)) return;
#endif
            
            var type = sourceAsset.GetType();
            type.OpenEditorScript();
#endif
        }
        
    }
}