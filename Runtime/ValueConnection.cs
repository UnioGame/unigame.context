
namespace UniGame.Context.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using global::UniGame.Core.Runtime;
    using global::UniGame.Runtime.Common;
    using global::UniGame.Runtime.DataFlow;
    using global::UniGame.Runtime.ObjectPool;
    using global::UniGame.Core.Runtime.ObjectPool;
    using R3;


    public class ValueConnection<TData> : 
        ILifeTimeContext,
        IPoolable
    {
        protected HashSet<TData> _registeredItems = new();
        protected LifeTime _lifeTime = new();

        public int Count => _registeredItems.Count;
        
        public ILifeTime LifeTime => _lifeTime;
        
        #region ipoolable
        
        public virtual void Release()
        {
            _lifeTime.Release();
            _registeredItems.Clear();
            OnRelease();
        }
        
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IDisposable Add(TData connection)
        {
            if (!_registeredItems.Add(connection))
                return Disposable.Empty;

            var disposable = ClassPool.Spawn<DisposableAction>();
            disposable.Initialize(() => Remove(connection));
            
            OnBind(connection);
            
            return disposable.AddTo(LifeTime);
        }

        public void Remove(TData connection)
        {
            _registeredItems.Remove(connection);
            OnUnbind(connection);
        }

        protected virtual void OnUnbind(TData connection) { }
        
        protected virtual void OnBind(TData connection) { }

        protected virtual void OnRelease() { }
    }
    
}
