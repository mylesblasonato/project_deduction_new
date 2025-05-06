using System.Reflection;
using UnityEngine;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;

#if UNITY_EDITOR
using AdvancedSceneManager.Editor.Utility;
using UnityEditor.Build.Reporting;
using UnityEditor.Build;
using UnityEditor.MPE;
using UnityEditor;
#endif

namespace AdvancedSceneManager.Utility
{

    #region Build

#if UNITY_EDITOR

    class ASMScriptableSingletonBuildStep : IPreprocessBuildWithReport
    {

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {

            // Force-load all ASMScriptableSingleton<> instances
            var singletonTypes = TypeCache.GetTypesDerivedFrom(typeof(ASMScriptableSingleton<>));
            foreach (var type in singletonTypes)
            {
                if (type.IsAbstract || type.ContainsGenericParameters)
                    continue;

                var instanceProp = type.GetProperty("instance", BindingFlags.Public | BindingFlags.Static);
                instanceProp?.GetValue(null); // Force-load instance
            }

            // Now that they're loaded, FindObjectsOfTypeAll should find them
            var allSingletons = Resources
                .FindObjectsOfTypeAll<ScriptableObject>()
                .Where(obj =>
                    obj.GetType().IsSubclassOfRawGeneric(typeof(ASMScriptableSingleton<>)) &&
                    obj.GetType().GetCustomAttribute<ASMFilePathAttribute>() != null &&
                    !AssetDatabase.Contains(obj) &&
                    !IsEditorOnly(obj));

            foreach (var obj in allSingletons)
                Move(obj);

        }

        private static bool IsEditorOnly(ScriptableObject obj)
        {
            var prop = obj.GetType().GetProperty("editorOnly", BindingFlags.Public | BindingFlags.Instance);
            return prop != null && prop.GetValue(obj) is true;
        }

        const string Folder = "Assets/ASMBuild";

        static void Move(ScriptableObject obj)
        {

            if (!obj)
                return;

            if (AssetDatabase.Contains(obj))
                return;

            if (Application.isBatchMode)
                Debug.Log($"#UCB Preparing '{obj.name}' for build.");

            var resourcesPath = obj.GetType().GetCustomAttribute<ASMFilePathAttribute>().path;

            var path = $"{Folder}/Resources/{resourcesPath}";

            obj.hideFlags = HideFlags.None;
            var s = Directory.GetParent(path).FullName.ConvertToUnixPath();

            AssetDatabaseUtility.CreateFolder(s);
            AssetDatabase.CreateAsset(obj, path);

            if (Application.isBatchMode)
            {
                var o = Resources.Load(resourcesPath, obj.GetType());
                if (o)
                    Debug.Log($"#UCB '{obj.name}' successfully prepared for build.");
                else
                    Debug.LogError($"#UCB Could not prepare '{obj.name}' for build. Unknown error.");
            }

        }

        static ASMScriptableSingletonBuildStep()
        {
            BuildUtility.postBuild += _ => Cleanup();
            EditorApplication.update += Cleanup;
        }

        static void Cleanup()
        {

            if (EditorApplication.isCompiling)
                return;

            if (AssetDatabase.IsValidFolder(Folder))
                AssetDatabase.DeleteAsset(Folder);

            EditorApplication.update -= Cleanup;

        }

    }

#endif

    #endregion
    #region FilePath

    /// <summary>A <see cref="FilePathAttribute"/> that supports build.</summary>
    public class ASMFilePathAttribute
#if UNITY_EDITOR
        : FilePathAttribute
#else
        : System.Attribute
#endif
    {

        /// <summary>The path to the associated <see cref="ScriptableSingleton{T}"/>.</summary>
        public string path { get; }
        public ASMFilePathAttribute(string relativePath)
#if UNITY_EDITOR
            : base(relativePath, Location.ProjectFolder)
#endif
        {
            path = relativePath;
        }

    }

    #endregion
    #region ScriptableSingleton

    /// <summary>A <see cref="ScriptableSingleton{T}"/> that supports build.</summary>
    public abstract class ASMScriptableSingleton<T>
#if UNITY_EDITOR
        : ScriptableSingleton<T>, INotifyPropertyChanged
#else
        : ScriptableObject, INotifyPropertyChanged
#endif
        where T : ASMScriptableSingleton<T>
    {

        #region Build step

        /// <summary>Specifies that build support will not be applied to this <see cref="ScriptableSingleton{T}"/>.</summary>
        public virtual bool editorOnly { get; }

        #endregion
        #region Instance

#if !UNITY_EDITOR

            public static T instance => GetInstance();

            static T m_instance;
            static T GetInstance()
            {

                if (!m_instance)
                    m_instance = Resources.Load<T>(typeof(T).GetCustomAttribute<ASMFilePathAttribute>().path.Replace(".asset", ""));

                return m_instance;

            }

#endif

        #endregion
        #region SerializedObject

#if UNITY_EDITOR

        SerializedObject m_serializedObject;

        /// <summary>Gets a cached <see cref="SerializedObject"/> for this <see cref="ScriptableSingleton{T}"/>.</summary>
        /// <remarks>Only available in editor.</remarks>
        public SerializedObject serializedObject => m_serializedObject ??= new(this);

#endif

        #endregion
        #region Save

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
             PropertyChanged?.Invoke(this, new(propertyName));

#if UNITY_EDITOR

        public virtual void OnValidate()
        {

            if (EditorApplication.isUpdating || ProcessService.level != ProcessLevel.Main)
                return;

            Save();
            OnPropertyChanged("");
            SceneImportUtility.Notify();

        }

#endif

        /// <summary>Saves the singleton to disk after a delay.</summary>
        /// <remarks>Can be called outside of editor, but has no effect.</remarks>
        public virtual void Save()
        {
#if UNITY_EDITOR
            EditorApplication.delayCall -= SaveNow;
            EditorApplication.delayCall += SaveNow;
#endif
        }

        /// <summary>Saves the singleton to disk.</summary>
        /// <remarks>Can be called outside of editor, but has no effect.</remarks>
        public void SaveNow()
        {
#if UNITY_EDITOR

            if (ProcessService.level != ProcessLevel.Main)
                return;

            if (!this)
                return;

            if (EditorUtility.IsPersistent(this))
            {
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssetIfDirty(this);
            }
            else
                base.Save(true);
#endif
        }

        protected virtual void OnDisable()
        {
#if UNITY_EDITOR
            if (EditorUtility.IsDirty(this))
                SaveNow();
#endif
        }

        #endregion

    }

    #endregion

}