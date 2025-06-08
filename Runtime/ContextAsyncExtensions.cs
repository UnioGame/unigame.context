using System;
using UniGame.Runtime.Extension;
using UniGame.Core.Runtime;

namespace UniGame.Context.Runtime
{
    using Cysharp.Threading.Tasks;
    using R3;
    using UnityEngine;

    public static class ContextAsyncExtensions
    {
        public static async UniTask<TValue> ReceiveFirstAsync<TValue>(this IReadOnlyContext context)
        {
            return await context.ReceiveFirstAsync<TValue>(context.LifeTime);
        }

        public static async UniTask<TValue> ReceiveFirstAsync<TValue>(this IReadOnlyContext context,
            ILifeTime lifeTime)
        {
            if (context == null) return default;
            
            if (context.Contains<TValue>())
                return context.Get<TValue>();

            return await context
                .Receive<TValue>()
                .FirstAsync(cancellationToken:lifeTime.Token);
        }

        public static async UniTask<TValue> ReceiveFirstAsync<TValue>(this IReadOnlyContext context, IObservable<TValue> observable)
        {
            if (context == null) return default;
            return await observable.AwaitFirstAsync(context.LifeTime);
        }

        public static async UniTask<TValue> ReceiveFirstAsync<TValue>(this ILifeTime lifeTime, IObservable<TValue> observable)
        {
            if (lifeTime == null) return default;
            return await observable.AwaitFirstAsync(lifeTime);
        }

        public static async UniTask<TComponent> ReceiveFirstFromSceneAsync<TComponent>(this IContext context, bool register = true)
            where TComponent : Object
        {
            TComponent result   = null;
            var        lifeTime = context.LifeTime;

            if (context.Contains<TComponent>())
                return context.Get<TComponent>();
            
            while (lifeTime.IsTerminated == false)
            {
                result = Object.FindObjectOfType<TComponent>();
                if (result != null)
                    break;

                await UniTask.Yield();
            }

            if(register)
                context.Publish(result);
    
            return result;
        }
    }
}