using Cysharp.Threading.Tasks;
using UniGame.Context.Runtime;
using UniGame.Core.Runtime;

namespace UniGame.Runtime.Rx.Runtime
{
    public interface IAsyncSharedSource
    {
        UniTask<IAsyncDataSource> GetSource<TAsset>(IContext context);
    }
}