namespace UniGame.Context.Runtime {
    using UnityEngine.SceneManagement;

#if UNITY_6000_3_OR_NEWER
    using SceneId = UnityEngine.SceneManagement.SceneHandle;
#else
    using SceneId = System.Int32;
#endif

    using System;

    [Serializable]
    public struct SceneInfo {
        public SceneId     handle;
        public string      name;
        public string      path;
        public bool        isActive;
        public SceneStatus status;
    }
}