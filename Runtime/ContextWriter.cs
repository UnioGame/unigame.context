namespace UniModules.UniContextData.Runtime
{
    using global::UniGame.Core.Runtime.ObjectPool;
    using global::UniGame.Core.Runtime;
    using global::UniGame.Runtime.Rx;


    public class ContextPublisher : IMessagePublisher, IPoolable
    {
        private IContext               _context;
        private IContextData<IContext> _contextData;

        public void Initialize(IContext context, IContextData<IContext> contextData)
        {
            _context     = context;
            _contextData = contextData;
        }

        public void Publish<TValue>(TValue value)
        {
            if (_context == null || _contextData == null)
                return;

            _contextData.UpdateValue(_context, value);
        }

        public void Release()
        {
            _context     = null;
            _contextData = null;
        }
    }
}