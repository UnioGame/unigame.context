namespace UniModules.UniContextData.Runtime.Entities
{
    using System;
    using global::UniGame.Context.Runtime;
    using global::UniGame.Core.Runtime;
    using UnityEngine;

    public class EntityComponent : MonoBehaviour
    {
        [NonSerialized] private EntityContext _context = new EntityContext();

        public IContext Context => _context;
    }
}