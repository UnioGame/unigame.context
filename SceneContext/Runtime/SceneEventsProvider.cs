using UniGame.Runtime.DataFlow;

namespace UniGame.Context.Runtime {
    using R3;
    using UnityEngine.SceneManagement;

    public class SceneEventsProvider : ISceneEventsProvider 
    {
        private LifeTimeDefinition                         _lifeTime       = new();
        private Subject<Scene>                             _unloadSubject  = new();
        private Subject<(Scene scene, LoadSceneMode mode)> _loadingSubject = new();
        private Subject<(Scene previous, Scene active)>    _activeSubject  = new();

        public SceneEventsProvider() {
            Observable.FromEvent(
                x => SceneManager.sceneLoaded += OnSceneLoad,
                x => SceneManager.sceneLoaded -= OnSceneLoad).Subscribe().AddTo(_lifeTime);

            Observable.FromEvent(
                x => SceneManager.sceneUnloaded += OnSceneUnload,
                x => SceneManager.sceneUnloaded -= OnSceneUnload).Subscribe().AddTo(_lifeTime);

            Observable.FromEvent(
                x => SceneManager.activeSceneChanged += OnActiveSceneChanged,
                x => SceneManager.activeSceneChanged -= OnActiveSceneChanged).Subscribe().AddTo(_lifeTime);
        }

        public Observable<Scene>                             Unloaded  => _unloadSubject;
        public Observable<(Scene scene, LoadSceneMode mode)> Loaded    => _loadingSubject;
        public Observable<(Scene previous, Scene active)>    Activated => _activeSubject;

        public void Dispose() => _lifeTime.Terminate();

        #region private methods

        private void OnSceneUnload(Scene scene) {
            _unloadSubject.OnNext(scene);
        }

        private void OnSceneLoad(Scene scene, LoadSceneMode mode) {
            _loadingSubject.OnNext((scene,mode));
        }

        private void OnActiveSceneChanged(Scene fromScene, Scene toScene) {
            _activeSubject.OnNext((fromScene,toScene));
        }

        #endregion
    }
}