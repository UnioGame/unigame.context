using UnityEngine;

namespace UniGame.Context.Runtime
{
    using global::UniGame.Core.Runtime;
    using Cysharp.Threading.Tasks;
    using global::UniGame.Context.Runtime;

    public abstract class AsyncObjectDataSource : MonoBehaviour, IAsyncDataSource
    {

        public abstract UniTask<IContext> RegisterAsync(IContext context);
    }
}
