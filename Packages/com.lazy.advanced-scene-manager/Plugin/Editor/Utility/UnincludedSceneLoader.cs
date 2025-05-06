using System.Collections;
using System.Linq;
using AdvancedSceneManager.Core;
using AdvancedSceneManager.Editor.Utility;
using AdvancedSceneManager.Loading;
using AdvancedSceneManager.Models;
using AdvancedSceneManager.Utility;
using UnityEngine;
using Scene = AdvancedSceneManager.Models.Scene;
using sceneManager = UnityEngine.SceneManagement.SceneManager;

namespace AdvancedSceneManager.Editor
{

    class UnincludedSceneLoader : SceneLoader
    {

        public override bool isGlobal => true;

        public override bool CanHandleScene(Scene scene) =>
            !scene.isIncludedInBuilds;

        public override IEnumerator LoadScene(Scene scene, SceneLoadArgs e)
        {

            if (!Profile.current)
                yield break;

            var didAdd = false;
            if (!Profile.current.standaloneScenes.Contains(scene))
            {
                didAdd = true;
                Profile.current.standaloneScenes.Add(scene);
                BuildUtility.UpdateSceneList(true);
            }

#if UNITY_6000_1_OR_NEWER

            if (didAdd)
                Debug.LogWarning($"The scene '{scene.path}' was not included in build. It has been added to standalone.");

            var async = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(scene.path, new(UnityEngine.SceneManagement.LoadSceneMode.Additive));

            if (e.reportProgress)
                yield return async.ReportProgress(SceneOperationKind.Unload, e.operation, scene);

            yield return async;

            var openedScene = e.GetOpenedScene();
            while (!openedScene.IsValid())
            {
                openedScene = e.GetOpenedScene();
                yield return null;
            }

            e.SetCompleted(openedScene);
            yield return null;

#else
            if (didAdd)
                Debug.LogError($"The scene '{scene.path}' could not be opened, as was not included in build. It has been added to standalone, but play mode must be restarted to update build scene list.");
            e.SetCompletedWithoutScene();
#endif

        }

        public override IEnumerator UnloadScene(Scene scene, SceneUnloadArgs e)
        {

            var async = sceneManager.UnloadSceneAsync(scene.internalScene.Value);

            if (e.reportProgress)
                yield return async.ReportProgress(SceneOperationKind.Unload, e.operation, scene);

            yield return async;

            e.SetCompleted();

        }

    }

}
