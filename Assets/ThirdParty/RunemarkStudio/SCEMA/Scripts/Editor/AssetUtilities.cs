namespace Runemark.SCEMA
{
    using UnityEditor;
    using UnityEngine;

    public class AssetUtilities : MonoBehaviour
    {
        public static T LoadAsset<T>(string guid) where T : Object
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
        public static T CreateAsset<T>(string path, string name) where T : ScriptableObject
        {
            string[] folders = path.Split('/');
            string currentPath = "Assets";
            for (int i = 0; i < folders.Length; i++)
            {
                if (!AssetDatabase.IsValidFolder(currentPath + "/" + folders[i]))
                {
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                }
                currentPath += "/" + folders[i];
            }

            var obj = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(obj, currentPath + "/" + name + ".asset");
            return obj;

        }
    }
}
