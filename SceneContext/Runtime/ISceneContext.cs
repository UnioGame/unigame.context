namespace UniGame.Context.Runtime {
    using UnityEngine.SceneManagement;

#if UNITY_6000_3_OR_NEWER
    using SceneId = UnityEngine.SceneManagement.SceneHandle;
#else
    using SceneId = System.Int32;
#endif

    using global::UniGame.Core.Runtime;
    using global::UniGame.Runtime.Rx;
    using R3;


    public interface IReadOnlySceneContext : IMessageContext
    {
        SceneId                         Handle { get; }

        string                                 Name   { get; }
        
        ReadOnlyReactiveProperty<bool> IsActive { get; }

        ReadOnlyReactiveProperty<SceneStatus> Status { get; }
    }

    public interface ISceneContext : 
        IReadOnlySceneContext, 
        IManagedBroadcaster<IMessagePublisher>,
        IContext
    {

        void Release();
        
        void UpdateSceneStatus();

    }
}