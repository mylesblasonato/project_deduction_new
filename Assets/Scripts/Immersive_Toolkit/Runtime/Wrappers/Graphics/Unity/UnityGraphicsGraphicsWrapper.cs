using UnityEngine;
using UnityEngine.SceneManagement;

namespace Immersive_Toolkit.Runtime
{
    public class UnityGraphicsGraphicsWrapper : MonoBehaviour, IGraphicsWrapper
    {
        private static UnityGraphicsGraphicsWrapper _instance;

        public static IGraphicsWrapper Instance
        {
            get
            {
                if (_instance == null)
                {
                    // Try to find one in the scene
                    _instance = FindFirstObjectByType<UnityGraphicsGraphicsWrapper>();

                    // Or create one if none exists
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("UnityGraphicsProcessor (Singleton)");
                        _instance = go.AddComponent<UnityGraphicsGraphicsWrapper>();
                        //DontDestroyOnLoad(go);
                    }
                }

                return _instance;
            }
        }

        public GameObject GetInstance()
        {
            return _instance.gameObject;
        }

        public AsyncInstantiateOperation<GameObject> InstantiateObject(GameObject prefab)
        {
            var instance = Object.InstantiateAsync(prefab);
            return instance;
        }

        public AsyncInstantiateOperation<GameObject> InstantiateObjectAsChildOfParent(GameObject prefab,
            Transform parent)
        {
            var instance = Object.InstantiateAsync(prefab, parent);
            return instance;
        }
    }
}
