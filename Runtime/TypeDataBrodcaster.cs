
namespace UniGame.GameFlow.Runtime.Connections
{
    using System;
    using System.Runtime.CompilerServices;
    using Context.Runtime;
    using global::UniGame.Core.Runtime;
    using UniGame.Runtime.Rx;


    public class TypeDataBrodcaster : 
        ValueConnection<IMessagePublisher>,
        IManagedBroadcaster<IMessagePublisher> ,
        IMessagePublisher
    {

        public int BindingsCount => Count;
        
        #region IContextData interface

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Publish<TData>(TData value) {
            foreach (var messagePublisher in _registeredItems)
            {
                messagePublisher.Publish(value);
            }
        }

        #endregion

        public IDisposable Broadcast(IMessagePublisher connection) => Add(connection);

        public void Break(IMessagePublisher connection) => Remove(connection);
    }
    
}
