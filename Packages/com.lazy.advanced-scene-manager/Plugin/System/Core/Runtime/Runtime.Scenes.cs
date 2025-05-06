using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AdvancedSceneManager.Callbacks.Events;
using AdvancedSceneManager.Models;
using AdvancedSceneManager.Models.Enums;
using AdvancedSceneManager.Utility;
using UnityEngine;

namespace AdvancedSceneManager.Core
{

    partial class Runtime
    {

        private readonly List<Scene> m_openScenes = new();

        /// <summary>Gets the scenes that are open.</summary>
        public IEnumerable<Scene> openScenes => m_openScenes.NonNull();

        #region Checks

        bool IsValid(Scene scene) => scene;
        bool IsClosed(Scene scene) => !openScenes.Contains(scene);
        bool IsOpen(Scene scene) => scene && scene.isOpen;
        bool CanOpen(Scene scene, SceneCollection collection, bool openAllScenes) => openAllScenes || !collection.scenesThatShouldNotAutomaticallyOpen.Contains(scene);

        bool LoadingScreen(Scene scene) => LoadingScreenUtility.IsLoadingScreenOpen(scene);

        bool IsPersistent(Scene scene, SceneCollection closeCollection = null, SceneCollection nextCollection = null) =>
            scene.isPersistent
            || (scene.keepOpenWhenNewCollectionWouldReopen && nextCollection && nextCollection.Contains(scene));

        bool NotPersistent(Scene scene, SceneCollection closeCollection = null, SceneCollection nextCollection = null) =>
            !IsPersistent(scene, closeCollection, nextCollection);

        bool NotPersistent(Scene scene, SceneCollection closeCollection = null) =>
            !IsPersistent(scene, closeCollection);

        bool NotLoadingScreen(Scene scene) =>
            !LoadingScreen(scene);

        #endregion
        #region Open

        public SceneOperation Open(Scene scene) =>
            Open(scenes: scene);

        public SceneOperation OpenAndActivate(Scene scene) =>
            SceneOperation.Queue().OpenAndActivate(scene);

        /// <inheritdoc cref="Open(IEnumerable{Scene})"/>
        public SceneOperation Open(params Scene[] scenes) =>
            Open((IEnumerable<Scene>)scenes);

        /// <summary>Opens the scenes.</summary>
        /// <remarks>Open scenes will not be re-opened, please close it first.</remarks>
        public SceneOperation Open(IEnumerable<Scene> scenes)
        {

            scenes = scenes.
                    NonNull().
                    Where(IsValid).
                    Where(IsClosed);

            if (!scenes.Any())
                return SceneOperation.done;

            if (SceneManager.runtime.currentOperation?.acceptsSubOperations ?? false)
            {
                //User is attempting to open a scene in a open callback, lets make current operation wait for this one
                var operation = SceneOperation.Start().Open(scenes);
                SceneManager.runtime.currentOperation.WaitFor(operation);
                return operation;
            }
            else
                return SceneOperation.Queue().Open(scenes);

        }

        public SceneOperation OpenWithLoadingScreen(Scene scene, Scene loadingScreen) =>
            Open(scene).With(loadingScreen);

        /// <summary>Opens a scene with a loading screen.</summary>
        public SceneOperation OpenWithLoadingScreen(IEnumerable<Scene> scene, Scene loadingScreen) =>
            Open(scene).With(loadingScreen);

        #endregion
        #region Close

        public SceneOperation Close(Scene scene) =>
            Close(scenes: scene);

        /// <inheritdoc cref="Close(IEnumerable{Scene})"/>
        public SceneOperation Close(params Scene[] scenes) =>
            Close((IEnumerable<Scene>)scenes);

        /// <summary>Closes the scenes.</summary>
        /// <remarks>Closes persistent scenes.</remarks>
        public SceneOperation Close(IEnumerable<Scene> scenes) =>
            Close(scenes, skipEmptySceneCheck: false);

        public SceneOperation Close(IEnumerable<Scene> scenes, bool skipEmptySceneCheck = false)
        {

            scenes = scenes.
                NonNull().
                Where(IsValid).
                Where(IsOpen);

            var sidj = scenes.ToList();

            if (!skipEmptySceneCheck && !sidj.Any())
                return SceneOperation.done;

            return SceneOperation.Queue().Close(scenes);

        }

        public SceneOperation CloseWithLoadingScreen(Scene scene, Scene loadingScreen) =>
            Close(scene).With(loadingScreen);

        /// <summary>Opens a scene with a loading screen.</summary>
        public SceneOperation CloseWithLoadingScreen(IEnumerable<Scene> scene, Scene loadingScreen) =>
            Close(scene).With(loadingScreen);

        /// <summary>Closes all scenes and collections.</summary>
        public SceneOperation CloseAll(bool exceptLoadingScreens = true, bool exceptUnimported = true, params Scene[] except)
        {

            var scenes = openScenes;
            if (exceptLoadingScreens)
                scenes = scenes.Where(s => !s.isLoadingScreen && !except.Contains(s));

            if (SceneManager.settings.project.reverseUnloadOrderOnCollectionClose)
                scenes = scenes.Reverse();

            var operation = Close(scenes, skipEmptySceneCheck: true).UntrackAllCollectionsCallback().RegisterCallback<LoadingScreenClosePhaseEvent>(e => UntrackPreload(), When.Before);

            if (!exceptUnimported)
                operation.RegisterCallback<SceneClosePhaseEvent>(e => e.WaitFor(CloseUnimportedScenes), When.After);

            return operation;

            IEnumerator CloseUnimportedScenes()
            {

                var scenes = SceneUtility.GetAllOpenUnityScenes().
                    Where(s => !s.ASMScene() && !FallbackSceneUtility.IsFallbackScene(s)).
                    ToArray();

                foreach (var scene in scenes)
                    yield return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);

            }

        }

        #endregion
        #region Preload

        /// <summary>Preloads the specified scene, to be displayed at a later time. See also: <see cref="FinishPreload()"/>, <see cref="CancelPreload"/>.</summary>
        /// <remarks>Scene must be closed beforehand.</remarks>
        public SceneOperation Preload(Scene scene, Action onPreloaded = null) =>
            Preload(onPreloaded: (s) => onPreloaded?.Invoke(), new[] { scene });

        /// <summary>Preloads the specified scenes, to be displayed at a later time. See also: <see cref="FinishPreload()"/>, <see cref="CancelPreload"/>.</summary>
        /// <remarks>Scene must be closed beforehand.</remarks>
        public SceneOperation Preload(Action<Scene> onPreloaded = null, params Scene[] scenes) =>
            Preload(scenes: scenes, onPreloaded);

        /// <summary>Preloads the specified scenes, to be displayed at a later time. See also: <see cref="FinishPreload()"/>, <see cref="CancelPreload"/>.</summary>
        /// <remarks>Scene must be closed beforehand.</remarks>
        public SceneOperation Preload(params Scene[] scenes) =>
            Preload(scenes, onPreloaded: null);

        #endregion
        #region Toggle

        /// <summary>Toggles the open state of this scene.</summary>
        public SceneOperation ToggleOpen(Scene scene) =>
            IsOpen(scene)
            ? Close(scene)
            : Open(scene);

        #endregion
        #region Active

        /// <summary>Gets the active scene.</summary>
        /// <remarks>Returns <see langword="null"/> if the active scene is not imported.</remarks>
        public Scene activeScene =>
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().ASMScene();

        [Obsolete]
        public void SetActive(Scene scene) =>
            SetActive(scene);

        /// <summary>Sets the scene as active.</summary>
        /// <remarks>No effect if not open.</remarks>
        public void Activate(Scene scene)
        {

            if (!scene || !scene.isOpen)
                return;

            if (scene.internalScene.HasValue && scene.internalScene.Value.isLoaded)
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene.internalScene.Value);
            else
                Debug.LogError("Could not set active scene since internalScene not valid.");

        }

        #endregion
        #region Reopen

        public SceneOperation Reopen(Scene scene)
        {

            if (IsClosed(scene))
                return SceneOperation.done;

            return SceneOperation.Queue().Close(scene).Open(scene);

        }

        /// <inheritdoc cref="Reopen(Scene)"/>
        public SceneOperation Reopen(IEnumerable<Scene> scene)
        {

            scene = scene.
                NonNull().
                Where(IsValid).
                Where(IsOpen);

            if (!scene.Any())
                return SceneOperation.done;

            return SceneOperation.Queue().Close(scene).Open(scene);

        }

        #endregion

        #region SceneState

        /// <summary>Gets the current state of the scene.</summary>
        public SceneState GetState(Scene scene)
        {

            if (!scene)
                return SceneState.Unknown;

            if (!scene.internalScene.HasValue)
                return SceneState.NotOpen;

            if (FallbackSceneUtility.IsFallbackScene(scene.internalScene.Value))
                throw new InvalidOperationException("Fallback scene is tracked by a Scene, this should not happen, something went wrong somewhere.");

            var isPreloaded = scene.internalScene.HasValue && !scene.internalScene.Value.isLoaded;
            var isOpen = openScenes.Contains(scene);
            var isQueued =
                QueueUtility<SceneOperation>.queue.Any(o => o.open?.Contains(scene) ?? false) ||
                QueueUtility<SceneOperation>.running.Any(o => o.open?.Contains(scene) ?? false);

            var isOpening = SceneOperation.currentLoadingScene == scene;
            var isPreloading = preloadedScenes.Contains(scene) || (SceneOperation.currentLoadingScene == scene && SceneOperation.isCurrentLoadingScenePreload);

            if (isPreloaded) return SceneState.Preloaded;
            else if (isPreloading) return SceneState.Preloading;
            else if (isOpen) return SceneState.Open;
            else if (isOpening) return SceneState.Opening;
            else if (isQueued) return SceneState.Queued;
            else return SceneState.NotOpen;

        }

        #endregion
        #region Unimported scenes

        /// <summary>Retrieves the list of unimported scenes that are currently open.</summary>
        public IEnumerable<UnityEngine.SceneManagement.Scene> unimportedScenes =>
            SceneUtility.GetAllOpenUnityScenes().
            Where(s => !FallbackSceneUtility.IsFallbackScene(s)).
            Where(s => !s.ASMScene());

        /// <summary>Closes all open scenes that are unimported.</summary>
        public IEnumerator CloseUnimportedScenes()
        {

            foreach (var scene in unimportedScenes.ToArray())
                yield return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene.path);

        }

        #endregion

    }

}
