namespace UniGame.Context.Runtime
{
    using System.Threading;
    using Core.Runtime;
    using Cysharp.Threading.Tasks;
    using UnityEngine;

    public static class GameContext
    {
        public static IContext Context;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void ResetContext()
        {
            Context = null;
        }

        public static T Get<T>()
        {
            if (Context == null)
            {
                Debug.LogError($" Context is null. Can't get value of type {typeof(T)}");
                return default;
            }

            return Context.Get<T>();
        }

        public static async UniTask<T> GetAsync<T>(CancellationToken token = default)
        {
            if (Context != null && Context.Contains<T>())
                return Context.Get<T>();

            await UniTask.WaitWhile(() => Context == null, cancellationToken: token);
            
            return await Context.GetAsync<T>();
        }

        public static async UniTask<IContext> GetContextAsync(CancellationToken token = default)
        {
            if (Context != null)
                return Context;

            await UniTask.WaitWhile(() => Context == null,cancellationToken:token);
            return Context;
        }
    }
}