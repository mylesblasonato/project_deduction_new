using UnityEngine;

namespace Immersive_Toolkit.Runtime
{
    public interface IGraphicsWrapper
    {
        public static IGraphicsWrapper Instance;
        public AsyncInstantiateOperation<GameObject> InstantiateObject(GameObject prefab);
        public AsyncInstantiateOperation<GameObject> InstantiateObjectAsChildOfParent(GameObject prefab,
            Transform parent);
    }
}