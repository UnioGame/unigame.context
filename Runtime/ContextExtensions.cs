using UniCore.Runtime.ProfilerTools;
using UniGame.Context.Runtime;

namespace UniGame.Context.Runtime
{
    using System;
    using global::UniGame.Core.Runtime;
    using R3;
    using UnityEngine;

    public static class ContextExtensions
    {

        public static IContextConnection Merge(this IContext context, IContext targetContext)
        {
            if (context is IContextConnection connector)
                return connector.Merge(targetContext);
                    
            connector = new ContextConnection();
            connector.Broadcast(targetContext);
            connector.Broadcast(context);
            return connector;
        }
        
        public static IContextConnection Merge(this IContextConnection context, IContext targetContext)
        {
            context.Broadcast(targetContext);
            return context;
        }
        
        public static IContextConnection Merge(this IContext source,params IContext[] targetContext)
        {
            var connector = new ContextConnection();
            foreach (var context in targetContext)
            {
                connector.Broadcast(context);
            }
            connector.Broadcast(source);
            return connector;
        }
        
        public static Observable<Unit> ReceiveFirst<T>(this IContext targetContext, 
            IMessageReceiver sourceContext) where T : class
        {
            return sourceContext.Receive<T>()
                .Where(x => x != null)
                .Take(1)
                .Do(targetContext.Publish)
                .AsUnitObservable();
        }

        public static Observable<T> ReceiveFirst<T>(this IContext sourceContext, Action<T> action) where T : class
        {
            return sourceContext
                .Receive<T>()
                .Where(x => x != null)
                .Take(1)
                .Do(x => action?.Invoke(x));
        }

        public static IDisposable LogValue<T>(this IContext context)
        {
            return context.
                Receive<T>().
                Do(x => GameLog.Log($"{typeof(T).Name} CONTEXT Get {x.GetType().Name}", Color.gold)).
                Subscribe();
        }
        
        public static IDisposable LogValue<T>(this IContext context,string id)
        {
            return context.
                Receive<T>().
                Do(x => GameLog.Log($"{id} CONTEXT Get {x.GetType().Name}", Color.gold)).
                Subscribe();
        }
        
        public static IContext LogValue<T>(this IContext context,string id, ILifeTime lifeTime)
        {
            context.LogValue<T>(id).
                AddTo(lifeTime);
            return context;
        }
        
        public static Observable<Unit> ReceiveFirst<T>(this IContext targetContext, Observable<T> sourceContext) where T : class
        {
            return sourceContext
                .Take(1)
                .Do(targetContext.Publish)
                .AsUnitObservable();
        }
    }
}