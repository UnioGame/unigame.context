namespace UniGame.Context.Runtime 
{
    using System;
    using R3;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    
    public static class SceneContextSystem 
    {
        private static ScenesContext scenesContext = new ScenesContext(new SceneEventsProvider());

        static SceneContextSystem() {

            //editor only scene behaviour
// #if UNITY_EDITOR
//             EditorApplication.playModeStateChanged += x => {
//                 if (x != PlayModeStateChange.ExitingPlayMode)
//                     return;
//                 scenesContext.Dispose();
//                 scenesContext = new ScenesContext(new SceneEventsProvider());
//             };
// #endif

        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize() {}

        public static IReadOnlySceneContext Active                               => scenesContext.Active;
        public static IReadOnlySceneContext GetActiveContext(this object source) => scenesContext.Active;

        public static IReadOnlySceneContext GetContextFor(this GameObject gameObject) => scenesContext.Get(gameObject.scene.handle);

        public static IReadOnlySceneContext GetContextFor(this Component component) => GetActiveContext(component.gameObject);

        public static IReadOnlySceneContext GetContextFor(this int sceneHandle) => scenesContext.Get(sceneHandle);

        public static TCurrentValue ReleaseWithScene<TCurrentValue>(this TCurrentValue CurrentValue, int handle)
            where TCurrentValue : IDisposable 
        {
            var context = GetContextFor(handle);
            var lifeTime = context;
            lifeTime.LifeTime.AddDispose(CurrentValue);
            return CurrentValue;
        }

        public static TCurrentValue ReleaseWithScene<TCurrentValue>(this TCurrentValue CurrentValue, Scene scene)
            where TCurrentValue : IDisposable {
            return ReleaseWithScene(CurrentValue, scene.handle);
        }

        public static void ReleaseContextFor(this GameObject gameObject) {
            if (!gameObject) return;
            scenesContext.Release(gameObject.scene.handle); 
        }

        public static void ReleaseContextFor(this Component component) {
            if (!component)
                return;
            ReleaseContextFor(component.gameObject);
        }
        
        #region notifications

        public static Observable<IReadOnlySceneContext> NotifyActiveSceneContext(this object source) {
            return scenesContext.ActiveContext;
        }
        
        public static Observable<IReadOnlySceneContext> NotifyActiveSceneContext(this object source,SceneStatus status) {
            return scenesContext.ActiveContext.Where(x => x.Status.CurrentValue == status);
        }
        
        public static Observable<IReadOnlySceneContext> NotifyOnAllSceneContext(this object source) {
            return scenesContext.ContextChanges;
        }
        
        public static Observable<IReadOnlySceneContext> NotifyOnAllSceneContext(this object source,SceneStatus status) {
            return scenesContext.ContextChanges.Where(x => x.Status.CurrentValue == status);
        }
        
        public static Observable<IReadOnlySceneContext> NotifyOnSceneContext(this object source,string sceneName) 
        {
            return NotifyOnSceneContext(sceneName);
        }
        
        public static Observable<IReadOnlySceneContext> NotifyOnSceneContext(this GameObject gameObject) 
        {
            return NotifyOnSceneContext(gameObject.scene.handle);
        }
        
        public static Observable<IReadOnlySceneContext> NotifyOnSceneContext(this Component component) 
        {
            return NotifyOnSceneContext(component.gameObject);
        }
        
        public static Observable<IReadOnlySceneContext> NotifyOnSceneContext(int handle) {

            var scene  = SceneManagerUtils.GetRuntimeScene(handle);
            var filter = scenesContext.
                ContextChanges.
                Where(x => x.Handle == handle);
            
            return scene.isLoaded ? 
                Observable.Return(scenesContext.Get(scene.handle)).
                    Concat(filter) : filter;
        }

        public static Observable<IReadOnlySceneContext> NotifyOnSceneContext(int handle,SceneStatus status) {
            return NotifyOnSceneContext(handle).
                Where(x => x.Status.CurrentValue == status);
        }

        public static Observable<IReadOnlySceneContext> NotifyOnSceneContext(string sceneName) {

            var scene = SceneManagerUtils.GetRuntimeScene(sceneName);
            var filter = scenesContext.
                ContextChanges.
                Where(x => x.Name == sceneName);
            
            return scene.isLoaded ? 
                Observable.Return(scenesContext.Get(scene.handle)).
                    Concat(filter) : 
                filter;
        }

        public static Observable<IReadOnlySceneContext> NotifyOnSceneContext(string sceneName,SceneStatus status) {
            return NotifyOnSceneContext(sceneName).
                Where(x => x.Status.CurrentValue == status);
        }


        #endregion
        
        #region messages

        /// <summary>
        /// receive data from Any SceneContext
        /// </summary>
        /// <typeparam name="TCurrentValue"></typeparam>
        /// <returns></returns>
        public static Observable<TCurrentValue> ReceiveFromAnyScene<TCurrentValue>(this object source) {
            return scenesContext.Receive<TCurrentValue>();
        }
        
        public static Observable<TCurrentValue> ReceiveFromScene<TCurrentValue>(this object source,int sceneHanle) {
            var sceneThread = NotifyOnSceneContext(sceneHanle, SceneStatus.Loaded).
                Select(x => x.Receive<TCurrentValue>()).
                Switch();
            return sceneThread;
        }
        
        public static Observable<TCurrentValue> ReceiveFromScene<TCurrentValue>(this object source,string sceneName) {

            var sceneThread = NotifyOnSceneContext(sceneName, SceneStatus.Loaded).
                Select(x => x.Receive<TCurrentValue>()).
                Switch();
            return sceneThread;
        }

        public static void PublishToAllScenes<TCurrentValue>(this object source, TCurrentValue CurrentValue) {
            foreach (var context in scenesContext.SceneContexts) {
                context.Publish(CurrentValue);
            }
        }

        public static void PublishToScene<TCurrentValue>(this object source,string name, TCurrentValue CurrentValue) {
            foreach (var scene in SceneManagerUtils.GetRuntimeScenes(name)) {
                var context = scenesContext.Get(scene.handle);
                context.Publish(CurrentValue);
            }
        }
        
        public static void PublishToScene<TCurrentValue>(this object source,int handle, TCurrentValue CurrentValue) {
            var context = scenesContext.Get(handle);
            context.Publish(CurrentValue);
        }
        
        #endregion

    }
}
