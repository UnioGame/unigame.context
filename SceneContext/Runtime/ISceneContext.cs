namespace UniGame.Context.Runtime {
    using global::UniGame.Core.Runtime;
    using global::UniGame.Runtime.Rx;
    using R3;


    public interface IReadOnlySceneContext : IMessageContext
    {
        int                             Handle { get; }

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