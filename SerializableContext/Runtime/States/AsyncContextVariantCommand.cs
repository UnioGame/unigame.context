namespace UniGame.Context.Runtime.Runtime.States
{
    using System;
    using Context.Runtime;
    using global::UniGame.Core.Runtime;
    using Cysharp.Threading.Tasks;

    [Serializable]
    public class AsyncContextVariantCommand : 
        IAsyncContextCommand,
        IAsyncRollback<IContext>,
        IAsyncEndPoint<IContext>,
        IAsyncEndPoint
    {
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.InlineProperty]
        [Sirenix.OdinInspector.HideLabel]
#endif
        public AsyncContextCommandValue variant = new AsyncContextCommandValue();

        public async UniTask<AsyncStatus> ExecuteAsync(IContext value)
        {
            if (variant.HasValue == false)
                return AsyncStatus.Succeeded;
            return await variant.Value.ExecuteAsync(value);
        }

        public async UniTask Rollback(IContext source)
        {
            if (variant.HasValue == false)
                return;
            var value = variant.Value;
            if (value is IAsyncRollback<IContext> rollback)
                await rollback.Rollback(source);
        }

        public async UniTask ExitAsync(IContext data)
        {
            if (variant.HasValue == false)
                return;
            var value = variant.Value;
            switch (value)
            {
                case IAsyncEndPoint<IContext> dataEndPoint:
                    await dataEndPoint.ExitAsync(data);
                    break;
                case IAsyncEndPoint endPoint:
                    await endPoint.ExitAsync();
                    break;
            }
        }

        public async UniTask ExitAsync()
        {
            if (variant.HasValue == false)
                return;
            var value = variant.Value;
            if (value is IAsyncEndPoint endPoint)
                await endPoint.ExitAsync();
        }
    }
}