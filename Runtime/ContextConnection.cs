using UniGame.DataFlow;
using UniGame.Core.Runtime;
using UniGame.Runtime.Rx;
using UniGame.Runtime.DataFlow;

namespace UniGame.Context.Runtime
{
    using System;
    using System.Collections.Generic;
    
    using R3;
    
    public class ContextConnection :
        ValueConnection<IContext>,
        IContextConnection
    {
        private readonly Dictionary<Type, IContextSubscriptions> _subscriptions = new();
        private readonly EntityContext _cachedContext = new();

        public readonly int Id;

        public ContextConnection()
        {
            Id = Unique.GetId();
        }

        #region properties

        public int BindingsCount => Count;

        public bool HasValue => _cachedContext.HasValue;

        #endregion

        public IDisposable Broadcast(IMessagePublisher connection) => _cachedContext.Broadcast(connection);

        public void Break(IMessagePublisher connection) => _cachedContext.Break(connection);

        public void Disconnect(IContext connection) => base.Remove(connection);

        public IDisposable Connect(IContext source) => 
            ReferenceEquals(source, _cachedContext) || ReferenceEquals(source, this) 
            ? Disposable.Empty
            : Add(source);

        public bool Remove<TData>() => _cachedContext.Remove<TData>();

        public object Get(Type type) => _cachedContext.Get(type);

        public TData Get<TData>()
        {
            var result = default(TData);
            if (_cachedContext.Contains<TData>())
            {
                result = _cachedContext.Get<TData>();
                if (result != null) return result;
            }
            
            foreach (var context in _registeredItems)
            {
                if (!context.Contains<TData>())
                    continue;
                result = context.Get<TData>();
                if (result != null)
                    return result;
            }
            
            return result;
        }

        public bool Contains<TData>()
        {
            if (_cachedContext.Contains<TData>()) return true;
            foreach (var context in _registeredItems)
            {
                if (context.Contains<TData>()) return true;
            }

            return false;
        }

        public void Dispose() => Release();

        public void Publish<T>(T message) => _cachedContext.Publish(message);

        public Observable<T> Receive<T>()
        {
            //if already  subscribed just return cached context
            if (_subscriptions.TryGetValue(typeof(T), out var subscription))
                return _cachedContext.Receive<T>();

            //create stream
            foreach (var context in _registeredItems)
                AddContextReceiver<T>(context);

            return _cachedContext.Receive<T>();
        }
        
        private void UpdateSubscriptions(IContext context)
        {
            foreach (var subscription in _subscriptions)
            {
                subscription.Value.Add(context);
            }
        }

        private void AddContextReceiver<T>(IContext context)
        {
            if (context == null || context.LifeTime.IsTerminated)
                return;

            var targetType = typeof(T);
            
            if (!_subscriptions.TryGetValue(targetType, out var subscriptions))
            {
                subscriptions = new ContextSubscriptions<T>(_cachedContext);
                _subscriptions[targetType] = subscriptions;
            }
            
            var subscription = subscriptions as ContextSubscriptions<T>;
            subscription.Add(context);
            
            if (context.Contains<T>())
            {
                var value = context.Get<T>();
                Publish(value);
            }
        }

        protected override void OnBind(IContext connection)
        {
            UpdateSubscriptions(connection);
            connection.LifeTime.AddCleanUpAction(() => Remove(connection));
        }

        protected override void OnUnbind(IContext connection)
        {
            foreach (var subscription in _subscriptions)
            {
                subscription.Value.Remove(connection);
            }
        }

        protected override void OnRelease() => _cachedContext.Release();
    }

    public class ContextSubscriptions<TData> : IContextSubscriptions
    {
        public HashSet<IContext> contexts = new();
        public IContext target;
        public LifeTime lifeTime = new();
        public LifeTime connectionsLifeTime = new();

        public ContextSubscriptions(IContext context)
        {
            this.target = context;
        }
        
        public void Add(IContext context)
        {
            if (context == target) return;
            if (!contexts.Add(context)) return;
            Bind(context);
        }

        public void Remove(IContext context)
        {
            if (!contexts.Remove(context)) return;
            connectionsLifeTime.Restart();
            foreach (var item in contexts)
                Bind(item);
        }

        public void Dispose()
        {
            target = null;
            contexts.Clear();
            lifeTime.Restart();
            connectionsLifeTime.Restart();
        }

        private void Bind(IContext context)
        {
            context.Broadcast(target)
                .AddTo(connectionsLifeTime);
        }
    }

    public interface IContextSubscriptions : IDisposable
    {
        void Add(IContext context);
        void Remove(IContext context);
    }
}