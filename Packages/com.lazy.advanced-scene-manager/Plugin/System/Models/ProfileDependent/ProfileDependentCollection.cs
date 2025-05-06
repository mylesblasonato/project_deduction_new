using AdvancedSceneManager.Core;
using AdvancedSceneManager.Models.Interfaces;
using AdvancedSceneManager.Utility;
using UnityEngine;

namespace AdvancedSceneManager.Models.Utility
{

    /// <summary>Represents a <see cref="SceneCollection"/> that changes depending on active <see cref="Profile"/>.</summary>
    [CreateAssetMenu(menuName = "Advanced Scene Manager/Profile dependent collection", order = SceneUtility.basePriority + 100)]
    public class ProfileDependentCollection : ProfileDependent<SceneCollection>,
        IOpenable

    {

        public static implicit operator SceneCollection(ProfileDependentCollection instance) =>
            instance.GetModel(out var scene) ? scene : null;

        /// <summary>Gets the currently active collection.</summary>
        public SceneCollection collection => GetModel();

        public bool isOpen => collection.isOpen;
        public bool isQueued => collection.isQueued;

        public SceneOperation Open() => DoAction(c => c.Open());
        public SceneOperation Open(bool openAll) => DoAction(c => c.Open(openAll));
        public void _Open() => SpamCheck.EventMethods.Execute(() => Open());

        public SceneOperation OpenAdditive() => DoAction(c => c.OpenAdditive());
        public SceneOperation OpenAdditive(bool openAll) => DoAction(c => c.OpenAdditive(openAll));
        public void _OpenAdditive() => SpamCheck.EventMethods.Execute(() => OpenAdditive());

        public SceneOperation Reopen() => DoAction(c => c.Reopen());
        public SceneOperation Reopen(bool openAll) => DoAction(c => c.Reopen(openAll));
        public void _Reopen() => DoAction(c => c.Reopen());

        public SceneOperation Preload() => DoAction(c => c.Preload());
        public SceneOperation Preload(bool openAll) => DoAction(c => c.Preload(openAll));
        public void _Preload() => SpamCheck.EventMethods.Execute(() => Preload());

        public SceneOperation PreloadAdditive() => DoAction(c => c.PreloadAdditive());
        public SceneOperation PreloadAdditive(bool openAll) => DoAction(c => c.PreloadAdditive(openAll));
        public void _PreloadAdditive() => SpamCheck.EventMethods.Execute(() => PreloadAdditive());

        public SceneOperation ToggleOpen() => DoAction(c => c.ToggleOpen());
        public SceneOperation ToggleOpen(bool openAll) => DoAction(c => c.ToggleOpen(openAll));
        public void _ToggleOpen() => SpamCheck.EventMethods.Execute(() => ToggleOpen());

        public SceneOperation Close() => DoAction(c => c.Close());
        public void _Close() => SpamCheck.EventMethods.Execute(() => Close());

    }

}
