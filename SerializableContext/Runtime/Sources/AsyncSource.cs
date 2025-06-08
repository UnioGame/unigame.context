namespace UniGame.Context.Runtime
{
    using Cysharp.Threading.Tasks;
    using UniGame.Core.Runtime;
    using UnityEngine;

    public abstract class AsyncSource : ScriptableObject, IAsyncDataSource
    {
        public abstract UniTask<IContext> RegisterAsync(IContext context);
    }
}