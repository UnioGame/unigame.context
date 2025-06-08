namespace UniGame.Context.Runtime.Abstract {
    using System;
    using System.Collections.Generic;
    using R3;


    public interface IScenesContext
    {
        IEnumerable<IReadOnlySceneContext> SceneContexts { get; }

        /// <summary>
        /// always return context for current active scene
        /// </summary>
        IReadOnlySceneContext Active { get; }

        /// <summary>
        /// reactive active context
        /// </summary>
        ReadOnlyReactiveProperty<IReadOnlySceneContext> ActiveContext { get; }

        /// <summary>
        /// context changes thread
        /// </summary>
        Observable<IReadOnlySceneContext> ContextChanges { get; }

        
        /// <summary>
        /// Get Scene context by scene handle
        /// </summary>
        IReadOnlySceneContext Get(int sceneHandle);

        SceneStatus GetStatus(int sceneHandle);

    }
}