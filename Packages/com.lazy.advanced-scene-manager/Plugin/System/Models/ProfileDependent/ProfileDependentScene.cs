using System;
using AdvancedSceneManager.Core;
using AdvancedSceneManager.Models.Interfaces;
using AdvancedSceneManager.Utility;
using UnityEngine;

namespace AdvancedSceneManager.Models.Utility
{

    /// <summary>Represents a <see cref="Scene"/> that changes depending on active <see cref="Profile"/>.</summary>
    [CreateAssetMenu(menuName = "Advanced Scene Manager/Profile dependent scene", order = SceneUtility.basePriority + 100)]
    public class ProfileDependentScene : ProfileDependent<Scene>,
        IOpenable
    {

        public static implicit operator Scene(ProfileDependentScene instance) =>
            instance.GetModel(out var scene) ? scene : null;

        /// <summary>Gets the currently active scene.</summary>
        public Scene scene => GetModel();

        public bool isOpen => scene.isOpen;
        public bool isQueued => scene.isQueued;

        public SceneOperation Open() => DoAction(s => s.Open());
        public void _Open() => Open();

        public SceneOperation Reopen() => DoAction(s => s.Reopen());
        public void _Reopen() => Reopen();

        public SceneOperation OpenAndActivate() => DoAction(s => s.OpenAndActivate());
        public void _OpenAndActivate() => OpenAndActivate();

        public void _ToggleOpenState() => ToggleOpen();
        public SceneOperation ToggleOpen() => DoAction(s => s.ToggleOpen());
        public void _ToggleOpen() => ToggleOpen();

        public SceneOperation Close() => DoAction(s => s.Close());
        public void _Close() => Close();

        public SceneOperation Preload(Action onPreloaded = null) => DoAction(s => s.Preload(onPreloaded));
        public void _Preload() => Preload();

        public SceneOperation FinishPreload() => DoAction(s => s.FinishPreload());
        public void _FinishPreload() => FinishPreload();

        public SceneOperation CancelPreload() => DoAction(s => s.CancelPreload());
        public void _CancelPreload() => CancelPreload();

        public SceneOperation OpenWithLoadingScreen(Scene loadingScreen) => DoAction(s => s.OpenWithLoadingScreen(loadingScreen));
        public void _OpenWithLoadingScreen(Scene loadingScene) => OpenWithLoadingScreen(loadingScene);

        public SceneOperation CloseWithLoadingScreen(Scene loadingScreen) => DoAction(s => s.CloseWithLoadingScreen(loadingScreen));
        public void _CloseWithLoadingScreen(Scene loadingScene) => CloseWithLoadingScreen(loadingScene);

        public void Activate() => DoAction(s => s.Activate());
        public void _Activate() => Activate();

    }

}
