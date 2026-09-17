namespace UniGame.Context.Runtime {
#if UNITY_6000_3_OR_NEWER
    using SceneId = UnityEngine.SceneManagement.SceneHandle;
#else
    using SceneId = System.Int32;
#endif

    using System.Collections.Generic;
    using UnityEngine.SceneManagement;

    public static class SceneManagerUtils {

        public static SceneInfo GetSceneInfo(SceneId sceneHandle) {
            
            var scene = SceneManagerUtils.GetRuntimeScene(sceneHandle);
            return GetSceneInfo(scene);
            
        }

        public static IEnumerable<string> GetScenesNames() {
            for (int i = 0; i < SceneManager.sceneCount; i++) {
                var scene = SceneManager.GetSceneAt(i);
                yield return scene.name;
            }    
        }
        
        public static IEnumerable<SceneId> GetScenesHandles() {
            for (int i = 0; i < SceneManager.sceneCount; i++) {
                var scene = SceneManager.GetSceneAt(i);
                yield return scene.handle;
            }    
        }

        
        public static SceneInfo GetSceneInfo(this Scene scene) {

            var handle = scene.handle;
            var status    = scene.isLoaded ? 
                SceneStatus.Loaded : SceneStatus.Unload;
            return new SceneInfo() {
                handle = handle,
                name = scene.name,
                path = scene.path,
                status = status,
                isActive = SceneManager.GetActiveScene().handle == handle,
            };
        }
        
        public static Scene GetRuntimeScene(string sceneName) 
        {
            for (int i = 0; i < SceneManager.sceneCount; i++) {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.name == sceneName)
                    return scene;
            }    
            return new Scene();
        }
        
        public static IEnumerable<Scene> GetRuntimeScenes(string sceneName) 
        {
            for (var i = 0; i < SceneManager.sceneCount; i++) {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.name == sceneName)
                    yield return scene;
            }    
        }
        
        public static Scene GetRuntimeScene(SceneId handle) 
        {
            for (int i = 0; i < SceneManager.sceneCount; i++) {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.handle == handle)
                    return scene;
            }    
            return new Scene();
        }
        
    }
}