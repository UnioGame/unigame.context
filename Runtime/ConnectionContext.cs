namespace UniGame.Context.Runtime
{
    using System;
    using Core.Runtime;
    using R3;
    using UniGame.Runtime.Rx;

    public class ConnectionContext : IContext
    {
        public IContext connection;
        public EntityContext context;
        
        public ILifeTime LifeTime => context.LifeTime;
        
        public int BindingsCount => 0;

        public ConnectionContext(IContext connection)
        {
            this.connection = connection;
            context  = new EntityContext();
            connection.LifeTime.AddDispose(context);
        }

        public Observable<T> Receive<T>()
        {
            var contextObservable = context.Receive<T>();
            var connectionObservable = connection.Receive<T>();
            contextObservable = contextObservable.Merge(connectionObservable);

            return contextObservable;
        }

        public object Get(Type type)
        {
            var result = context.Get(type);
            return result ?? connection.Get(type);
        }

        public TData Get<TData>()
        {
            var result = context.Get<TData>();
            return result ?? connection.Get<TData>();
        }

        public bool Contains<TData>()
        {
            var result = context.Contains<TData>();
            return result || connection.Contains<TData>();
        }

        public bool HasValue => context.HasValue;

        public bool Remove<TData>()
        {
            return context.Remove<TData>();
        }

        public void Publish<T>(T message)
        {
            context.Publish(message);
        }

        public void Dispose()
        {
            if (LifeTime.IsTerminated) return;
            context.Dispose();
        }

        public IDisposable Broadcast(IMessagePublisher connection)
        {
            return Disposable.Empty;
        }

        public void Break(IMessagePublisher connection)
        {
        }
    }
}