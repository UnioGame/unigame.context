namespace UniGame.Context.Runtime
{
    using global::UniGame.Core.Runtime;
     

    public interface IContextConnection : 
        IConnection<IContext>,
        IDisposableContext
    {
        void Disconnect(IContext connection);
    }
}