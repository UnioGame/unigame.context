
namespace UniGame.GameFlow.Runtime.Connections
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using UniGame.Runtime.Common;
    using global::UniGame.Runtime.ObjectPool;
    using global::UniGame.Core.Runtime.ObjectPool;
    using global::UniGame.Core.Runtime;
    using UniGame.Runtime.Rx;


    public class TypeDataReceiver : 
        IPoolable, 
        IBroadcaster<IMessagePublisher>,
        IMessagePublisher
    {
        private List<IMessagePublisher> _registeredItems = new();
        private int count = 0;

        public int ConnectionsCount => count;
        
        #region ipoolable
        
        public virtual void Release()
        {
            _registeredItems.Clear();
            UpdateCounter();
        }
        
        #endregion
        
        #region IContextData interface

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Publish<TData>(TData value)
        {
            for (var i = 0; i < count; i++)
            {
                var context = _registeredItems[i];
                context.Publish(value);
            }
        }

        #endregion


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IDisposable Broadcast(IMessagePublisher connection)
        {
            if (!_registeredItems.Contains(connection))
                _registeredItems.Add(connection);
            
            var disposable = ClassPool.Spawn<DisposableAction>();
            disposable.Initialize(() => Disconnect(connection));
            
            UpdateCounter();
            return disposable;
        }

        public void Disconnect(IMessagePublisher connection)
        {
            _registeredItems.Remove(connection);
            UpdateCounter();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateCounter()
        {
            count = _registeredItems.Count;
        }
    }
    
}
