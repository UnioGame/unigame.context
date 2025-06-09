namespace UniGame.Context.Runtime
{
    using System;
    using global::UniGame.Core.Runtime;

    public interface IAsyncContextPrototype<TResult> 
        : IDisposable, IAsyncFactory<IContext,TResult>
    {
    }
}