namespace UniGame.Context.Runtime {
    using System;
    using R3;
    using UnityEngine.SceneManagement;

    public interface ISceneEventsProvider : IDisposable {
        Observable<Scene>                             Unloaded  { get; }
        Observable<(Scene scene, LoadSceneMode mode)> Loaded    { get; }
        Observable<(Scene previous, Scene active)>    Activated { get; }
    }
}