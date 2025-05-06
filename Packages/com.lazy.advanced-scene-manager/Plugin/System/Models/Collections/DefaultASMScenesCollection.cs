using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using AdvancedSceneManager.Utility;
using UnityEngine;
using System.IO;
using AdvancedSceneManager.Models.Interfaces;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AdvancedSceneManager.Models
{

    [Serializable]
    public class DefaultASMScenesCollection : ISceneCollection
    {

        const string defaultCollectionDescription =
            "ASM contains some default scenes that you may use or take inspiration from. " +
            "The scenes are provided as a UPM sample, you may use the button below, or use the package manager, to import it.";

        [SerializeField] internal string m_id = GuidReferenceUtility.GenerateID();

        public bool isImported;

        public Scene this[int index] => scenes.ElementAtOrDefault(index);

        public IEnumerable<Scene> scenes => SceneManager.assets.defaults.Enumerate();
        public IEnumerable<string> scenePaths => scenes.Select(s => s.path);
        public string title => "ASM Defaults";
        public string description => defaultCollectionDescription;
        public int count => scenes.Count();
        public string id => m_id;

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new(propertyName));

        public IEnumerator<Scene> GetEnumerator() =>
            scenes.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        public bool Contains(Scene scene) =>
            scenes.Contains(scene);

#if UNITY_EDITOR

        /// <summary>Imports the sample containing the default scenes.</summary>
        public static void ImportScenes()
        {

            var folder = SceneManager.package.folder + "/Samples~/Default ASM Scenes";

            var destinationFolder = $"Assets/Samples/Advanced Scene Manager/{SceneManager.package.version}/Default ASM scenes";

            List<string> failed = new();
            if (!AssetDatabase.DeleteAssets(SceneManager.assets.defaults.Enumerate().Select(s => s.path).ToArray(), failed))
                Debug.LogError("Could not delete the following scenes:\n\n" + string.Join("\n", failed));

            AssetDatabase.DeleteAsset(destinationFolder);
            AssetDatabase.Refresh();

            EditorApplication.delayCall += () =>
            {
                Directory.GetParent(destinationFolder).Create();
                FileUtil.CopyFileOrDirectory(folder, destinationFolder);
                AssetDatabase.Refresh();
            };

        }

        /// <summary>Removes the default scenes.</summary>
        public static void Unimport()
        {

            List<string> failed = new();
            if (!AssetDatabase.DeleteAssets(SceneManager.assets.defaults.Enumerate().Select(s => s.path).ToArray(), failed))
                Debug.LogError("Could not delete the following scenes:\n\n" + string.Join("\n", failed));

            if (Profile.current)
                Profile.current.RemoveDefaultASMScenes();

        }

#endif

    }

#if UNITY_EDITOR
    class DefaultASMScenesPostProcessor : AssetPostprocessor
    {

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            Import(importedAssets);
            Unimport(deletedAssets);
        }

        static void Import(string[] importedAssets)
        {

            var paths = importedAssets.Where(path => path.StartsWith("Assets/Samples/Advanced Scene Manager") && path.EndsWith(".unity"));
            if (!paths.Any())
                return;

            var defaultASMScenes = SceneManager.assets.defaults.Enumerate();
            foreach (var path in paths.ToArray())
            {
                //Scenes might already be imported if updating from 2.5 -> 2.6.
                //In order to not break existing references, lets grab those and point them towards the newly imported scene assets instead.

                var name = Path.GetFileNameWithoutExtension(path);
                var scene = defaultASMScenes.Find(name);
                var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);

                if (scene && asset)
                {
                    scene.path = path;
                    scene.sceneAsset = asset;
                    scene.m_sceneAssetGUID = AssetDatabase.GUIDFromAssetPath(path).ToString();
                }
            }

            //Import any scenes that wasn't imported prior (there is a check for duplicate imports)
            var scenes = SceneUtility.Import(paths.ToArray());

            foreach (var scene in scenes)
            {
                scene.m_isDefaultASMScene = true;
                scene.Save();
            }

            if (Profile.current)
                Profile.current.AddDefaultASMScenes();

        }

        static void Unimport(string[] deletedAssets)
        {

            var paths = deletedAssets.Where(path => path.StartsWith("Assets/Samples/Advanced Scene Manager") && path.EndsWith(".unity"));
            if (!paths.Any())
                return;

            if (Profile.current)
            {
                EditorApplication.delayCall += () => Profile.current.OnPropertyChanged(nameof(Profile.defaultASMScenes));
            }

        }

    }
#endif

}
