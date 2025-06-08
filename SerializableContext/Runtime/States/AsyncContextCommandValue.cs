namespace UniModules.UniGame.Context.SerializableContext.Runtime.States
{
    using System;
    using Abstract;
    using global::UniGame.Common;
    using global::UniGame.Core.Runtime;

    [Serializable]
    public class AsyncContextCommandValue : 
        VariantValue<IAsyncContextCommand,AsyncContextStateAsset,IAsyncCommand<IContext,AsyncStatus>>
    {
        
    }
}