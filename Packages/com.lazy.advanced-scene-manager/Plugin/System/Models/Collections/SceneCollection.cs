using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using AdvancedSceneManager.Core;
using AdvancedSceneManager.Models.Enums;
using UnityEngine;
using AdvancedSceneManager.Utility;
using UnityEngine.Events;
using AdvancedSceneManager.Models.Interfaces;
using UnityEngine.Serialization;

#if INPUTSYSTEM && ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.Utilities;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AdvancedSceneManager.Models
{

    /// <summary>Represents a collection of scenes, that should open at the same time.</summary>
    /// <remarks>Only one collection can be fully open at a time; additive collections are supported but not considered fully open.</remarks>
    public class SceneCollection : ASMModel,
        IEquatable<SceneCollection>,
        ISceneCollection,
        IEditableCollection,
        IOpenableCollection,
        IFindable,
        ILockable
    {

        #region ISceneCollection

        [SerializeField] internal List<Scene> m_scenes = new();
        [SerializeField] protected string m_description;

        public int count =>
            m_scenes.Count;

        public Scene this[int index] =>
            m_scenes.ElementAtOrDefault(index);

        public string title =>
            m_title;

        [HideInInspector]
        public string description
        {
            get => m_description;
            set => m_description = value;
        }

        /// <summary>Gets both <see cref="scenes"/> and <see cref="loadingScreen"/>.</summary>
        /// <remarks><see langword="null"/> is filtered out.</remarks>
        public IEnumerable<Scene> allScenes =>
            m_scenes.
            Concat(new[] { loadingScreen }).
            Where(s => s);

        /// <summary>Gets if this collection has any scenes.</summary>
        public bool hasScenes => m_scenes.Where(s => s).Any();

        public IEnumerable<string> scenePaths =>
            m_scenes?.Select(s => s.path) ?? Enumerable.Empty<string>();

        public IEnumerable<Scene> scenes =>
            m_scenes ?? Enumerable.Empty<Scene>();

        public IEnumerator<Scene> GetEnumerator() =>
            m_scenes?.GetEnumerator() ?? Enumerable.Empty<Scene>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        public bool Contains(Scene scene) =>
            scenes.Contains(scene);

        #endregion

        #region IEditableCollection

        List<Scene> IEditableCollection.sceneList => m_scenes;

        #endregion

        #region IOpenableCollection

        /// <summary>Gets if this collection is open.</summary>
        public override bool isOpen => SceneManager.runtime.IsOpen(this);

        /// <summary>Gets if this collection is opened additively.</summary>
        public bool isOpenAdditive => SceneManager.runtime.IsOpen(this, additive: true);

        /// <summary>Gets if this collection is opened additively.</summary>
        public bool isOpenNonAdditive => SceneManager.runtime.IsOpen(this, additive: false);

        /// <inheritdoc cref="Runtime.IsQueued(SceneCollection)"/>
        public override bool isQueued => SceneManager.runtime.IsQueued(this);


        /// <summary>Opens this collection.</summary>
        public override SceneOperation Open() => Open(openAll: false);

        /// <summary>Opens this collection.</summary>
        public SceneOperation Open(bool openAll) => SceneManager.runtime.Open(this, openAll);


        /// <summary>Closes this collection.</summary>
        public override SceneOperation Close() => SceneManager.runtime.Close(this);


        /// <summary>Reopens this collection.</summary>
        public override SceneOperation Reopen() => Reopen(openAll: false);

        /// <summary>Reopens this collection.</summary>
        public SceneOperation Reopen(bool openAll) => SceneManager.runtime.Reopen(this, openAll);


        /// <summary>Toggles this collection open or closed.</summary>
        public override SceneOperation ToggleOpen() => ToggleOpen(openAll: false);

        /// <summary>Toggles this collection open and closed.</summary>
        public SceneOperation ToggleOpen(bool openAll = false) => SceneManager.runtime.ToggleOpen(this, openAll);


        /// <summary>Opens this collection as additive.</summary>
        public SceneOperation OpenAdditive(bool openAll = false) => SceneManager.runtime.OpenAdditive(this, openAll);
        public void _OpenAdditive() => SpamCheck.EventMethods.Execute(() => OpenAdditive());

        #endregion

        #region IPreloadable

        /// <summary>Gets if this collection is currently preloaded.</summary>
        public override bool isPreloaded => SceneManager.runtime.preloadedCollection == this;

        /// <summary>Preloads this collection.</summary>
        public override SceneOperation Preload() => Preload(openAll: false);

        /// <summary>Preloads this collection.</summary>
        public SceneOperation Preload(bool openAll = false) => SceneManager.runtime.Preload(this, openAll);

        /// <summary>Preloads this collection as additive.</summary>
        public SceneOperation PreloadAdditive(bool openAll = false) => SceneManager.runtime.PreloadAdditive(this, openAll);
        public void _PreloadAdditive() => SceneManager.runtime.PreloadAdditive(this);

        #endregion

        #region IFindable

        /// <summary>Gets: <code>t:AdvancedSceneManager.Models.SceneCollection</code> Used in <see cref="AssetDatabase.FindAssets(string)"/>.</summary>
        public readonly static string AssetSearchString = "t:" + typeof(SceneCollection).FullName;

        /// <summary>Matches this collection against the query string.</summary>
        public override bool IsMatch(string q) =>
            base.IsMatch(q) || title == q;

        /// <summary>Finds a collection based on its title or id.</summary>
        public static SceneCollection Find(string q, bool activeProfile = true) =>
            activeProfile
            ? Profile.current.collections.FirstOrDefault(c => c && c.IsMatch(q))
            : SceneManager.assets.collections.Find(q);

        /// <summary>Finds a collection based on its title or id.</summary>
        public static bool TryFind(string q, out SceneCollection collection, bool activeProfile = true) =>
          collection =
            (activeProfile
            ? Profile.current.collections.FirstOrDefault(c => c && c.IsMatch(q))
            : SceneManager.assets.collections.Find(q));

        #endregion

        #region ILockable

        [SerializeField] private bool m_isLocked;
        [SerializeField] private string m_lockMessage;

        /// <summary>Gets if this collection is locked.</summary>
        public bool isLocked
        {
            get => m_isLocked;
            set { m_isLocked = value; OnPropertyChanged(); }
        }

        /// <summary>Gets or sets the message to be displayed when unlocking this collection.</summary>
        public string lockMessage
        {
            get => m_lockMessage;
            set { m_lockMessage = value; OnPropertyChanged(); }
        }

        #endregion

        #region INotifyPropertyChanged

#if UNITY_EDITOR

        public override void OnValidate()
        {
            EditorApplication.delayCall += Editor.Utility.BuildUtility.UpdateSceneList;
            base.OnValidate();
        }

#endif

        public override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {

            base.OnPropertyChanged(propertyName);

            if (propertyName == nameof(startupOption))
                NotifyStartup();

        }

        #endregion

        #region User data

        [SerializeField, FormerlySerializedAs("m_extraData")] private ScriptableObject m_userData;

        /// <summary>The extra data that is associated with this collection.</summary>
        /// <remarks>Use <see cref="UserData{T}"/> to cast it to the desired type.</remarks>
        public ScriptableObject userData
        {
            get => m_userData;
            set { m_userData = value; OnPropertyChanged(); }
        }

        /// <summary>Casts and returns <see cref="userData"/> as the specified type. Returns null if invalid type.</summary>
        public T UserData<T>() where T : ScriptableObject =>
            (T)userData;

        #endregion

        #region Name / title

        [SerializeField] internal string m_title = "New Collection";
        [SerializeField] internal string m_prefix;

#if UNITY_EDITOR

        /// <summary>Gets if name should be prefixed with <see cref="m_prefix"/>.</summary>
        protected virtual bool UsePrefix { get; } = true;

        internal void SetTitleAfterCreation(string prefix, string title)
        {
            m_title = title;
            ((ScriptableObject)this).name = $"{(UsePrefix ? prefix : "")}{title}";
        }

        internal void Rename(string newName, string prefix)
        {
            m_prefix = prefix;
            Rename(newName);
        }

        internal override void Rename(string newName)
        {

            if (string.IsNullOrEmpty(newName))
                return;

            if (m_prefix == null && FindProfile(out var p))
                m_prefix = p.prefix;

            m_title = newName;

            if (!string.IsNullOrEmpty(m_prefix) && newName.StartsWith(m_prefix))
            {
                // If the name starts with the prefix, check if it's exactly the same as before renaming.
                bool isUserInput = m_title != null && newName == m_title;

                if (!isUserInput)
                    newName = newName.Substring(m_prefix.Length);
            }

            var prefix = UsePrefix ? m_prefix : "";
            newName = $"{prefix}{newName}";

            while (newName.EndsWith(".asset"))
                newName = newName.Remove(name.Length - ".asset".Length);

            base.Rename(newName);

        }

#endif

        #endregion

        #region Input bindings

        [SerializeField] internal InputBinding[] m_inputBindings = Array.Empty<InputBinding>();
        [SerializeField] private Scene[] m_ignoreInputBindingsForScenes;

        /// <summary>Gets or sets the input bindings for this collection.</summary>
        public InputBinding[] inputBindings
        {
            get => m_inputBindings;
            set { m_inputBindings = value; OnPropertyChanged(); }
        }

        /// <summary>Specifies scenes where input bindings should be ignored while open (for this collection).</summary>
        public Scene[] ignoreInputBindingsForScenes
        {
            get => m_ignoreInputBindingsForScenes;
            set { m_ignoreInputBindingsForScenes = value; OnPropertyChanged(); }
        }

        #endregion

        #region Startup

        [SerializeField] private CollectionStartupOption m_startupOption = CollectionStartupOption.Auto;

        void NotifyStartup()
        {

            if (FindProfile(out var profile))
                foreach (var collection in profile.collections.Cast<ISceneCollection>())
                    collection.OnPropertyChanged(nameof(isStartupCollection));

        }

        /// <summary>Gets if this is a startup collection.</summary>
        /// <remarks>Only available in editor.</remarks>
        public bool isStartupCollection => isIncluded && FindProfile(out var profile) && profile.IsStartupCollection(this);

        /// <summary>Specifies startup option.</summary>
        public CollectionStartupOption startupOption
        {
            get => m_startupOption;
            set { m_startupOption = value; OnPropertyChanged(); }
        }

        #endregion

        #region Loading screen

        [SerializeField] private LoadingScreenUsage m_loadingScreenUsage = LoadingScreenUsage.UseDefault;
        [SerializeField] private Scene m_loadingScreen;

        /// <summary>The loading screen that is associated with this collection.</summary>
        public Scene loadingScreen
        {
            get => m_loadingScreen;
            set { m_loadingScreen = value; OnPropertyChanged(); }
        }

        /// <summary>Gets effective loading screen depending on <see cref="loadingScreenUsage"/>.</summary>
        public Scene effectiveLoadingScreen
        {
            get
            {
                if (loadingScreenUsage == LoadingScreenUsage.Override)
                    return loadingScreen;
                else if (loadingScreenUsage == LoadingScreenUsage.UseDefault)
                    return Profile.current.loadingScene;
                else
                    return null;
            }
        }

        /// <summary>Specifies what loading screen to use.</summary>
        public LoadingScreenUsage loadingScreenUsage
        {
            get => m_loadingScreenUsage;
            set { m_loadingScreenUsage = value; OnPropertyChanged(); }
        }

        #endregion

        #region Open properties

        [SerializeField] private Scene m_activeScene;
        [SerializeField] private bool m_setActiveSceneWhenOpenedAsAdditive;
        [SerializeField] private bool m_unloadUnusedAssets = true;
        [SerializeField] private bool m_openAsPersistent = false;
        [SerializeField] private List<Scene> m_scenesThatShouldNotAutomaticallyOpen = new();
        [SerializeField] private LoadPriority m_loadPriority = LoadPriority.Auto;

        /// <summary>Specifies the scene that should be activated after collection is opened.</summary>
        public Scene activeScene
        {
            get => m_activeScene;
            set { m_activeScene = value; OnPropertyChanged(); }
        }

        /// <summary>Specifies whatever this collection should be opened as persistent.</summary>
        public bool openAsPersistent
        {
            get => m_openAsPersistent;
            set { m_openAsPersistent = value; OnPropertyChanged(); }
        }

        /// <summary>Calls <see cref="Resources.UnloadUnusedAssets"/> after collection is opened or closed.</summary>
        public bool unloadUnusedAssets
        {
            get => m_unloadUnusedAssets;
            set { m_unloadUnusedAssets = value; OnPropertyChanged(); }
        }

        /// <summary>Specifies scenes that should not open automatically.</summary>
        public List<Scene> scenesThatShouldNotAutomaticallyOpen =>
            m_scenesThatShouldNotAutomaticallyOpen;

        /// <summary>Gets the scenes that should open automatically when collection is opened (when in openAll param is <see langword="false"/>).</summary>
        public IEnumerable<Scene> scenesToAutomaticallyOpen =>
            scenes.NonNull().Except(scenesThatShouldNotAutomaticallyOpen);

        /// <summary>Specifies whatever <see cref="activeScene"/> should be set, when collection is opened as additive.</summary>
        public bool setActiveSceneWhenOpenedAsAdditive
        {
            get => m_setActiveSceneWhenOpenedAsAdditive;
            set { m_setActiveSceneWhenOpenedAsAdditive = value; OnPropertyChanged(); }
        }

        /// <summary>Specifies the <see cref="LoadPriority"/> to use when opening this collection.</summary>
        public LoadPriority loadPriority
        {
            get => m_loadPriority;
            set { m_loadPriority = value; OnPropertyChanged(); }
        }

        /// <summary>Gets or sets whatever the scene should automatically open, when this collection is open. Default is <see langword="true"/>.</summary>
        public bool ShouldAutomaticallyOpenScene(Scene scene, bool? value = null)
        {

            if (value.HasValue)
            {

                scenesThatShouldNotAutomaticallyOpen.Remove(scene);

                if (!value.Value)
                    scenesThatShouldNotAutomaticallyOpen.Add(scene);

                Save();

                OnPropertyChanged(nameof(scenesThatShouldNotAutomaticallyOpen));

            }

            return !scenesThatShouldNotAutomaticallyOpen.Contains(scene);

        }

        #endregion

        #region Profile

        /// <summary>Find the <see cref="Profile"/> that this collection is associated with.</summary>
        public bool FindProfile(out Profile profile) =>
            profile = FindProfile();

        /// <summary>Find the <see cref="Profile"/> that this collection is associated with.</summary>
        public Profile FindProfile() =>
            SceneManager.assets.profiles.FirstOrDefault(p => p && p.Contains(this, true));

        #endregion

        #region Equality

        public override bool Equals(object obj) => obj is SceneCollection other && Equals(other);

        public bool Equals(SceneCollection other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;

            return id == other.id;
        }

        public override int GetHashCode()
        {
            return id != null ? id.GetHashCode() : GetInstanceID();
        }

        public static bool operator ==(SceneCollection left, SceneCollection right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;

            return left.Equals(right);
        }

        public static bool operator !=(SceneCollection left, SceneCollection right)
        {
            return !(left == right);
        }

        #endregion

        #region Events

        [SerializeField] Events m_events = new();

        /// <summary>Gets the unity events for this scene.</summary>
        public Events events => m_events;

        [Serializable]
        public struct Events
        {

            /// <summary>Occurs when this collection is opened.</summary>
            public UnityEvent<SceneCollection> OnOpen;

            /// <summary>Occurs when this collection is closed.</summary>
            public UnityEvent<SceneCollection> OnClose;

        }

        #endregion

        #region Is included

        [SerializeField] private bool m_isIncluded = true;

        /// <summary>Gets whatever this collection should be included in build.</summary>
        public bool isIncluded
        {
            get => SceneManager.settings.project.allowExcludingCollectionsFromBuild ? m_isIncluded : true;
            set { m_isIncluded = value; OnPropertyChanged(); }
        }

        #endregion

        public override string ToString() =>
            title;

    }

}
