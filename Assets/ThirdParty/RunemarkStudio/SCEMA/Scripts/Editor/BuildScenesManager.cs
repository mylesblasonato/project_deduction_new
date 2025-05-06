namespace Runemark.SCEMA
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;

    public class BuildScenesManager
    {
        public bool Contains(SceneAsset scene)
        {
            string scenePath = AssetDatabase.GetAssetPath(scene.Asset);
            return Contains(scenePath);
        }
        public bool Contains(string scenePath, List<EditorBuildSettingsScene> sceneList = null)
        {     
            if(sceneList == null) sceneList = EditorBuildSettings.scenes.ToList();

            foreach (var s in sceneList)
            {
                if (s.path == scenePath) return true;
            }
            return false;
        }

        public void Add(SceneAsset scene) 
        {
            string scenePath = AssetDatabase.GetAssetPath(scene.Asset);
            var sceneList = EditorBuildSettings.scenes.ToList();

            if (!Contains(scenePath, sceneList))
            {
                var buildSettingsScene = new EditorBuildSettingsScene(scenePath, true);
                sceneList.Add(buildSettingsScene);
                EditorBuildSettings.scenes = sceneList.ToArray();
            }             
        }
        public void Remove(SceneAsset scene)
        {
            string scenePath = AssetDatabase.GetAssetPath(scene.Asset);
            var sceneList = EditorBuildSettings.scenes.ToList();

            int cnt = sceneList.RemoveAll(x => x.path == scenePath);
            if (cnt > 0)
            {
                EditorBuildSettings.scenes = sceneList.ToArray();
            }            
        }        
    }
}
 