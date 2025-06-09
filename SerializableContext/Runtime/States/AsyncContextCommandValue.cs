namespace UniGame.Context.Runtime.Runtime.States
{
    using System;
    using Context.Runtime;
    using global::UniGame.Common;
    using global::UniGame.Core.Runtime;

    [Serializable]
    public class AsyncContextCommandValue : 
        VariantValue<IAsyncContextCommand,AsyncContextStateAsset,IAsyncCommand<IContext,AsyncStatus>>
    {
        
    }
}