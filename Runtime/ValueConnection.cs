using UniGame.Core.Runtime;
using UniGame.Runtime.Common;
using UniGame.Runtime.DataFlow;
using UniGame.Runtime.ObjectPool;
using UniGame.Core.Runtime.ObjectPool;

namespace UniGame.Context.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
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
            _lifeTime.Restart();
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
