using UniGame.Core.Runtime;

namespace UniGame.Context.Runtime
{
    public interface IAsyncContextCommand<TValue> : IAsyncCommand<IContext,TValue>
    {
        
    }
    
    public interface IAsyncContextCommand : IAsyncCommand<IContext,AsyncStatus>
    {
        
    }
}