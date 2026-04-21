namespace UniGame.Context.Runtime
{
    using System;

#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif

    [Serializable]
    public class AsyncSourceDescription
#if ODIN_INSPECTOR
        : ISearchFilterable
#endif
    {
        public bool enabled = true;
        public bool awaitLoading = false;
        
#if ODIN_INSPECTOR
        [DrawWithUnity]
#endif
        public AssetReferenceDataSource source;
        
        public string Name {

            get
            {
#if UNITY_EDITOR
                return source == null || source.editorAsset == null 
                    ? string.Empty
                    : source.editorAsset.name;
#endif
                return string.Empty;
            }
            
        }

        public bool IsMatch(string searchString)
        {
            if (string.IsNullOrEmpty(searchString)) return true;
            return Name.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}