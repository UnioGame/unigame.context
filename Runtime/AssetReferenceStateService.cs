namespace UniGame.Context.Runtime
{
    using System;
    using global::UniGame.AddressableTools.Runtime;
    using global::UniGame.Context.Runtime;

    [Serializable]
    public class AssetReferenceStateService : AssetReferenceScriptableObject<ServiceDataSourceAsset>
    {
        public AssetReferenceStateService(string guid) : base(guid)
        {
        }
    }
}