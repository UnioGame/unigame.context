namespace UniGame.Context.Runtime
{
    using AddressableTools.Runtime;
    using UniGame.Context.Runtime;
    using System.Collections.Generic;
    using global::UniGame.Core.Runtime;
    using Cysharp.Threading.Tasks;
    using UnityEngine;

    [CreateAssetMenu(menuName = "UniGame/Sources/SpriteAtlasesSource", fileName = nameof(SpriteAtlasesSource))]
    public class SpriteAtlasesSource : AsyncSource
    {
        public List<AssetReferenceSpriteAtlas> Atlases = new List<AssetReferenceSpriteAtlas>();
        
        public sealed override async UniTask<IContext> RegisterAsync(IContext context)
        {
            var lifeTime = context.LifeTime;
            var atlases = await UniTask.WhenAll(Atlases.Select(x => x.LoadAssetTaskAsync(lifeTime)))
                .AttachExternalCancellation(lifeTime.Token);
            context.Publish(atlases);
            return context;
        }
    }
}
