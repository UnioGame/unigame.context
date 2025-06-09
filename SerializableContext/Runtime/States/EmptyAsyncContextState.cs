namespace UniGame.Context.Runtime.Runtime.States
{
    using System;
    using Context.Runtime;
    using global::UniGame.Core.Runtime;
    using Cysharp.Threading.Tasks;

    [Serializable]
    public class EmptyAsyncContextState : IAsyncContextStateStatus
    {
        private readonly AsyncStatus _result;
        
        public EmptyAsyncContextState(AsyncStatus result) => _result = result;

        public UniTask<AsyncStatus> ExecuteAsync(IContext value) => UniTask.FromResult(_result);

        public UniTask ExitAsync() => UniTask.CompletedTask;

        public ILifeTime LifeTime => global::UniGame.Runtime.DataFlow.LifeTime.TerminatedLifetime;
       
        public bool IsActive => false;
    }
}