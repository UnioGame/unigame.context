using UniGame.Core.Runtime;

namespace UniGame.Context.Runtime
{
    public static class ContextConnectionExtensions
    {
        public static IContextConnection ToConnector(this IContext context)
        {
            var connector = new ContextConnection();
            connector.Broadcast(context);
            return connector;
        }
        
    }
}
