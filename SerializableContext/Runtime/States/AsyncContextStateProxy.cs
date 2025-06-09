namespace UniGame.Context.Runtime.Runtime.States
{
    using System;
    using Context.Runtime;
    using global::UniGame.Core.Runtime;

    [Serializable]
    public class AsyncContextStateProxy : AsyncStateProxyValue<IContext, AsyncStatus>, IAsyncContextState
    {

        public AsyncContextStateProxy(
            IAsyncStateCommand<IContext, AsyncStatus> command = null,
            IAsyncCompletion<AsyncStatus, IContext> onComplete = null,
            IAsyncEndPoint<IContext> endPoint = null,
            IAsyncRollback<IContext> onRollback = null) : base(command, onComplete, endPoint, onRollback)
        {
        }

        protected sealed override AsyncStatus GetInitialExecutionValue() => AsyncStatus.Pending;
    }

}