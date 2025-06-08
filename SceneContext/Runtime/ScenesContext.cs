namespace UniGame.Context.Runtime {
    using System;
    using System.Collections.Generic;
    using Abstract;
    using global::UniGame.Core.Runtime;
    using global::UniGame.Core.Runtime.Extension;
    using global::UniGame.Runtime.DataFlow;
    using R3;
    using UnityEngine.SceneManagement;

    [Serializable]
    public class ScenesContext : 
        IScenesContext ,
        IMessageReceiver
    {
        private static DummyReadOnlySceneContext dummyReadOnlySceneContext = new DummyReadOnlySceneContext();


        private readonly ISceneEventsProvider                    _eventsProvider;
        private          LifeTimeDefinition                      _lifeTime;
        private          Dictionary<int, ISceneContext>          _sceneContexts;
        private          ReactiveProperty<IReadOnlySceneContext> _activeContext;
        private          Subject<IReadOnlySceneContext>          _sceneContextChanged;
        private          EntityContext                           _context = new();

        public ScenesContext(ISceneEventsProvider eventsProvider) {
            _lifeTime            = new LifeTimeDefinition();
            _sceneContexts       = new Dictionary<int, ISceneContext>(8);
            _eventsProvider      = eventsProvider.AddTo(_lifeTime);
            _activeContext       = new ReactiveProperty<IReadOnlySceneContext>().AddTo(_lifeTime);
            _sceneContextChanged = new Subject<IReadOnlySceneContext>().AddTo(_lifeTime);
            //bind with scene events
            Initialize();
        }

        /// <summary>
        /// we must clean all refs before die
        /// </summary>
        ~ScenesContext() => _lifeTime.Terminate();


        #region properties

        /// <summary>
        /// all current scene contexts
        /// </summary>
        public IEnumerable<IReadOnlySceneContext> SceneContexts => _sceneContexts.Values;

        /// <summary>
        /// scene context lifetime
        /// </summary>
        public ILifeTime LifeTime => _lifeTime;

        /// <summary>
        /// always return context for current active scene
        /// </summary>
        public IReadOnlySceneContext Active { get; private set; }

        /// <summary>
        /// changes in current active SceneContexts ot status
        /// </summary>
        public ReadOnlyReactiveProperty<IReadOnlySceneContext> ActiveContext => _activeContext;

        /// <summary>
        /// context changes thread
        /// </summary>
        public Observable<IReadOnlySceneContext> ContextChanges => _sceneContextChanged;

        #endregion

        
        #region MessageBroker

        public Observable<TValue> Receive<TValue>() => _context.Receive<TValue>();
        
        #endregion
        
        public void Dispose() {
            _lifeTime.Terminate();
            GC.SuppressFinalize(this);
        }

        public IReadOnlySceneContext Get(int sceneHandle) => Find(sceneHandle);

        public void Release(int sceneHandle) => Find(sceneHandle).Release();
        
        public SceneStatus GetStatus(int sceneHandle) => Find(sceneHandle).Status.CurrentValue;

        #region private methods

        #region scene events

        private void OnSceneUnload(Scene scene) => Remove(scene);

        private void OnSceneLoad(Scene scene, LoadSceneMode mode) {
            var handle = scene.handle;
            UpdateSceneContext(handle);
        }

        private void OnActiveSceneChanged(Scene fromScene, Scene toScene) {
            var previous = Find(fromScene.handle);
            previous.UpdateSceneStatus();
            Active = UpdateSceneContext(toScene.handle);
        }

        #endregion

        private void Initialize() {
            Connect();

            _lifeTime.AddCleanUpAction(() => {
                _sceneContexts.ForEach(x => x.Value.Release());
                _sceneContexts.Clear();
            });

            Active = Find(SceneManager.GetActiveScene().handle);
        }

        private void Connect() {
            _eventsProvider.Unloaded.Subscribe(OnSceneUnload).AddTo(_lifeTime);
            _eventsProvider.Loaded.Subscribe(x => OnSceneLoad(x.scene, x.mode)).AddTo(_lifeTime);
            _eventsProvider.Activated.Subscribe(x => OnActiveSceneChanged(x.previous, x.active)).AddTo(_lifeTime);
        }

        private ISceneContext Find(int sceneHandle)
        {
            return _sceneContexts.TryGetValue(sceneHandle, out var context) 
                ? context : dummyReadOnlySceneContext;
        }

        private ISceneContext UpdateSceneContext(int sceneHandle) {
            var context = Find(sceneHandle);
            context.UpdateSceneStatus();

            if (!Equals(context, dummyReadOnlySceneContext))
                return context;

            //Create new scene context 
            var sceneContext                     = new SceneContext(sceneHandle);
            //connect context data with common channel
            sceneContext.Broadcast(_context).AddTo(sceneContext.LifeTime);

            _sceneContexts[sceneHandle] = sceneContext;

            //if status or scene mode changed
            sceneContext.Status.
                CombineLatest(sceneContext.IsActive, (x, y) => sceneContext).
                Subscribe(x => _sceneContextChanged.OnNext(sceneContext)).
                AddTo(_lifeTime);

            return sceneContext;
        }

        private void Remove(Scene scene) => Remove(scene.handle);

        private void Remove(int sceneHandle) {
            var context = Find(sceneHandle);
            UpdateSceneContext(sceneHandle);
            context.Release();
            _sceneContexts.Remove(sceneHandle);
        }

        #endregion
    }
}