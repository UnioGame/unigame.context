namespace UniGame.Context.Runtime
{
    using System;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using Object = UnityEngine.Object;

#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    
    [Serializable]
    public class AssetReferenceDataSource<TAsset> : AssetReferenceScriptableObject<TAsset,IAsyncDataSource> 
        where TAsset : ScriptableObject
    {
        public AssetReferenceDataSource(string guid) : base(guid) {}
        
    }
    
    [Serializable]
    public class AssetReferenceDataSource : AssetReferenceT<ScriptableObject>
    {
        public AssetReferenceDataSource(string guid) : base(guid)
        {
        }

        public override bool ValidateAsset(string path)
        {
#if UNITY_EDITOR
            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            return asset is IAsyncDataSource;
#else
            return false;
#endif
        }
        
        public override bool ValidateAsset(Object obj)
        {
            return obj is ScriptableObject and IAsyncDataSource;
        }
    }
}
