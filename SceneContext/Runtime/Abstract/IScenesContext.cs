namespace UniGame.Context.Runtime.Abstract {
    using UnityEngine.SceneManagement;

#if UNITY_6000_3_OR_NEWER
    using SceneId = UnityEngine.SceneManagement.SceneHandle;
#else
    using SceneId = System.Int32;
#endif

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
        IReadOnlySceneContext Get(SceneId sceneHandle);

        SceneStatus GetStatus(SceneId sceneHandle);

    }
}