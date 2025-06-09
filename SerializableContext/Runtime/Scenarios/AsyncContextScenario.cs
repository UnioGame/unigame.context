namespace UniGame.Context.Runtime.Runtime.Scenarios
{
    using System;
    using Context.Runtime;
    using global::UniGame.Rx.Runtime;
    using global::UniGame.Core.Runtime;

    [Serializable]
    public class AsyncContextScenario : 
        AsyncScenario<IAsyncContextCommand,IContext>,
        IAsyncContextCommand,
        IAsyncContextRollback,
        IAsyncScenario
    {

        
    }
}