using System;
using System.Linq;
using AdvancedSceneManager.Core;
using AdvancedSceneManager.Models.Interfaces;
using AdvancedSceneManager.Models.Internal;
using AdvancedSceneManager.Utility;
using UnityEngine;

namespace AdvancedSceneManager.Models
{

    /// <summary>Represents scene helper. Contains functions for opening / closing collections and scenes from <see cref="UnityEngine.Events.UnityEvent"/>.</summary>
    [AddComponentMenu("")]
    public class ASMSceneHelper : ScriptableObject,
        IOpenableCollection<SceneCollection>,
        IOpenableScene<Scene>
    {

        /// <inheritdoc cref="Object.name"/>
        public new string name => base.name; //Prevent renaming from UnityEvent

        /// <summary>Gets a reference to scene helper.</summary>
        public static ASMSceneHelper instance => Assets.sceneHelper;

        #region IOpenableCollection

        SceneOperation IOpenable<SceneCollection>.Open(SceneCollection collection) => collection.Open();
        public void Open(SceneCollection collection) => collection.Open();
        public SceneOperation Open(SceneCollection collection, bool openAll = false) => collection.Open(openAll);
        public void _Open(SceneCollection collection) => SpamCheck.EventMethods.Execute(() => collection.Open());

        SceneOperation IOpenable<SceneCollection>.Reopen(SceneCollection collection) => collection.Reopen();
        public void Reopen(SceneCollection collection) => collection.Reopen();
        public void _Reopen(SceneCollection collection) => SpamCheck.EventMethods.Execute(() => collection.Reopen());

        public SceneOperation OpenAdditive(SceneCollection collection, bool openAll = false) => collection.OpenAdditive(openAll);
        public void OpenAdditive(SceneCollection collection) => collection.Open();
        public void _OpenAdditive(SceneCollection collection) => SpamCheck.EventMethods.Execute(() => collection.OpenAdditive());

        public SceneOperation Preload(SceneCollection collection, bool openAll = false) => collection.Preload(openAll);
        public void _Preload(SceneCollection collection) => SpamCheck.EventMethods.Execute(() => Preload(collection));

        public SceneOperation PreloadAdditive(SceneCollection collection, bool openAll = false) => collection.PreloadAdditive(openAll);
        public void _PreloadAdditive(SceneCollection collection) => SpamCheck.EventMethods.Execute(() => PreloadAdditive(collection));

        public SceneOperation ToggleOpen(SceneCollection collection, bool openAll = false) => collection.ToggleOpen(openAll);
        public SceneOperation ToggleOpen(SceneCollection collection) => collection.ToggleOpen();
        public void _ToggleOpen(SceneCollection collection) => SpamCheck.EventMethods.Execute(() => collection.ToggleOpen());

        public SceneOperation Close(SceneCollection collection) => collection.Close();
        public void _Close(SceneCollection collection) => SpamCheck.EventMethods.Execute(() => collection.Close());

        #endregion
        #region IOpenableScene

        public SceneOperation Open(Scene scene) => scene.Open();
        public void _Open(Scene scene) => SpamCheck.EventMethods.Execute(() => Open(scene));

        public SceneOperation Reopen(Scene scene) => scene.Reopen();
        public void _Reopen(Scene scene) => SpamCheck.EventMethods.Execute(() => Reopen(scene));

        public SceneOperation OpenAndActivate(Scene scene) => scene.OpenAndActivate();
        public void _OpenAndActivate(Scene scene) => SpamCheck.EventMethods.Execute(() => OpenAndActivate(scene));

        public SceneOperation ToggleOpenState(Scene scene) => scene.ToggleOpen();
        public SceneOperation ToggleOpen(Scene scene) => scene.ToggleOpen();
        public void _ToggleOpen(Scene scene) => SpamCheck.EventMethods.Execute(() => ToggleOpenState(scene));

        public SceneOperation Close(Scene scene) => scene.Close();
        public void _Close(Scene scene) => SpamCheck.EventMethods.Execute(() => Close(scene));

        public SceneOperation Preload(Scene scene, Action onPreloaded = null) => scene.Preload(onPreloaded);
        public void _Preload(Scene scene) => SpamCheck.EventMethods.Execute(() => Preload(scene));

        public void Activate(Scene scene) => scene.Activate();
        public void _Activate(Scene scene) => SpamCheck.EventMethods.Execute(() => Activate(scene));

        public SceneOperation OpenWithLoadingScreen(Scene scene, Scene loadingScene) => scene.OpenWithLoadingScreen(loadingScene);
        public SceneOperation CloseWithLoadingScreen(Scene scene, Scene loadingScene) => scene.CloseWithLoadingScreen(loadingScene);

        #endregion
        #region Custom

        /// <summary>Open all scenes that starts with the specified name.</summary>
        public void OpenWhereNameStartsWith(string name) =>
            SpamCheck.EventMethods.Execute(() => SceneManager.runtime.Open(SceneManager.assets.scenes.Where(s => s.name.StartsWith(name) && s.isIncludedInBuilds).ToArray()));

        /// <inheritdoc cref="App.Quit"/>
        public void Quit() => SceneManager.app.Quit();

        /// <inheritdoc cref="App.Restart"/>
        public void Restart() => SpamCheck.EventMethods.Execute(() => SceneManager.app.Restart());

        /// <summary>Re-opens <see cref="Runtime.openCollection"/>.</summary>
        public void RestartCollection() => SpamCheck.EventMethods.Execute(() => SceneManager.openCollection.Open());

        public SceneOperation FinishPreload() => SceneManager.runtime.FinishPreload();
        public void _FinishPreload() => SpamCheck.EventMethods.Execute(() => FinishPreload());

        public SceneOperation CancelPreload() => SceneManager.runtime.CancelPreload();
        public void _CancelPreload() => SpamCheck.EventMethods.Execute(() => CancelPreload());

        #endregion

    }

}
