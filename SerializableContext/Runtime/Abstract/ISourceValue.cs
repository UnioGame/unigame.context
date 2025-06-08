using Cysharp.Threading.Tasks;
using UniModules.UniGame.Context.SerializableContext.Runtime.Abstract;

namespace UniModules.UniGame.SerializableContext.Runtime.Abstract
{
    using global::UniGame.Runtime.Rx;
    using global::UniGame.Context.Runtime;
    using global::UniGame.Core.Runtime;

    public interface ISourceValue<TApiValue> : 
        IObservableValue<TApiValue>, 
        IPrototype<ISourceValue<TApiValue>>,
        IAsyncDataSource
    {
    }
    
    public interface IAsyncSourceValue<TApiValue> : 
        IObservableValue<TApiValue>, 
        IAsyncContextPrototype<IAsyncSourceValue<TApiValue>>
    {
        UniTask<TApiValue> CreateValue(IContext context);
    }
    
    
}