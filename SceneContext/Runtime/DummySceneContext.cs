using UniGame.Core.Runtime;
using UniGame.Runtime.DataFlow;
using UniGame.Runtime.Rx;

namespace UniGame.Context.Runtime
{
    using System;
    using System.Collections.Generic;
    using R3;

    public class DummyReadOnlySceneContext : ISceneContext
    {
        private ReadOnlyReactiveProperty<bool> isActive;
        private IReadOnlyDictionary<Type, IValueContainerStatus> editorValues;
        private LifeTimeDefinition lifeTime;
        private int handle;
        
        public DummyReadOnlySceneContext()
        {
            editorValues = new Dictionary<Type, IValueContainerStatus>(0);
            lifeTime = new LifeTimeDefinition();
            lifeTime.Terminate();
        }

        public void Release() { }

        public void Publish<T>(T message) { }

        public Observable<T> Receive<T>() => Observable.Empty<T>();

        public bool HasValue => false;

        public ReadOnlyReactiveProperty<bool> IsActive { get; } = 
            new ReactiveProperty<bool>(false);

        public ReadOnlyReactiveProperty<SceneStatus> Status { get; } =
            new ReactiveProperty<SceneStatus>(SceneStatus.Unload);

        public object Get(Type type) => null;

        public TData Get<TData>() => default(TData);

        public bool Contains<TData>() => false;

        public bool Remove<TData>() => false;

        public IReadOnlyDictionary<Type, IValueContainerStatus> EditorValues => editorValues;

        public void Dispose()
        {
        }

        public void UpdateSceneStatus()
        {
        }

        public ILifeTime LifeTime => lifeTime;

        public int Handle => Int32.MaxValue;

        public string Name => string.Empty;

        public IDisposable Broadcast(IMessagePublisher connection) => Disposable.Empty;

        public int BindingsCount => 0;

        public void Break(IMessagePublisher connection)
        {
        }
    }
}