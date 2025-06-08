using System;
using System.Collections.Generic;
using System.Linq;
using UniGame.Context.Runtime;
using UniGame.Core.Runtime;
using UniGame.Core.Runtime.SerializableType;
using UnityEngine;

namespace UniModules.UniGame.CoreModules.UniGame.Context.Editor.ContextDependencyGenerator
{
    [CreateAssetMenu(menuName = "UniGame/Context/DependencyAsset",fileName = nameof(ContextDependencyAsset))]
    public class ContextDependencyAsset : ScriptableObject
    {

        public List<SType> generatorTypes = new List<SType>()
        {
            typeof(SomeDemoClass)
        };


#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
#endif
        public void Create()
        {
            var context = new EntityContext();
            Type type = generatorTypes.FirstOrDefault();
            if (type == null || type.IsAbstract || type.IsInterface) return;
            
            ProceedType(type,context);
        }

        private void ProceedType(Type type, IContext context)
        {
            var constructors = type
                .GetConstructors()
                .Where(x => x.IsPublic);
            
            Debug.Log($"Proceed Type {type.Name}");
            
            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters();
                foreach (var parameterInfo in parameters)
                {
                    Debug.Log($"parameter : {parameterInfo.ParameterType.Name}");
                }
            }
        }
        
    }
    
    [Serializable]
    public class SomeDemoClass
    {
        public SomeDemoClass(List<int> values1,string value2)
        {
            
        }
    }
    
}
