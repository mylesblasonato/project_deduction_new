#if UNITY_EDITOR

using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;

namespace AdvancedSceneManager.Models.Utility
{

    [InitializeOnLoad]
    class AfterUpdateUtility
    {

        static AfterUpdateUtility() =>
            SceneManager.OnInitialized(() =>
            {
                UnzipSamplesFolder();
                UpdateASMDefaultsCollection();
            });

        static void UnzipSamplesFolder()
        {
            //Stupidly enough, unity won't include hidden folders in .unitypackage. So I guess we're doing this, its dumb, but it works.

            var folder = SceneManager.package.folder + "/Samples~";

            if (!File.Exists(folder + ".zip"))
                return;

            if (Directory.Exists(folder))
                Directory.Delete(folder, recursive: true);

            ZipFile.ExtractToDirectory(folder + ".zip", folder);
            File.Delete(folder + ".zip");
            if (File.Exists(folder + ".zip.meta")) File.Delete(folder + ".zip.meta");

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        }

        static void UpdateASMDefaultsCollection()
        {

            foreach (var profile in SceneManager.assets.profiles)
            {
                var collection = profile.dynamicCollections.FirstOrDefault(c => c.path == "Packages/com.lazy.advanced-scene-manager/Default scenes");
                if (collection is not null)
                {
                    profile.AddDefaultASMScenes();
                    profile.Delete(collection);
                    profile.Save();
                }
            }

        }

    }

}
#endif
