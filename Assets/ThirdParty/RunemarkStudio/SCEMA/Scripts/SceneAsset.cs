namespace Runemark.SCEMA
{
    using UnityEngine;

    [System.Serializable]
    public class SceneAsset
    {
        public string name = "";

#if UNITY_EDITOR
        public UnityEditor.SceneAsset Asset
        {
            get { return asset; }
            set 
            {
                asset = value;
                name = asset != null ? asset.name : "";
            }
        }

        
        [SerializeField]UnityEditor.SceneAsset asset;
        public SceneAsset(UnityEditor.SceneAsset scene)
        {
            Asset = scene;
        }

        
    #endif
    }
}