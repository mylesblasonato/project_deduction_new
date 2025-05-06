using System;
using System.Collections.Generic;
using System.Linq;
using AdvancedSceneManager.Loading;
using AdvancedSceneManager.Utility;
using LoadingScreenKit.Scripts;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

namespace LoadingScreenKit.Components.Controls
{
    [UxmlElement]
    public partial class SimpleProgressController : ValueController<float>, ILoadProgressListener
    {
        [UxmlAttribute]
        [SerializeField] protected bool trackUnloadingScenes;

        [SerializeField]
        private string[] includeTypes;

        private readonly HashSet<Type> allowedTypes = new();

        [UxmlAttribute]
        [TypePropertyDrawer(typeof(ILoadProgressData))]
        public string[] IncludeTypes
        {
            get => includeTypes;
            set
            {
                includeTypes = value ?? Array.Empty<string>();
                CacheAllowedTypes();
            }
        }

        public SimpleProgressController()
        {
            RegisterCallbackOnce<AttachToPanelEvent>(OnAttach);
            RegisterCallbackOnce<DetachFromPanelEvent>(OnDetach);
        }

        #region Unity Event Handlers

        private void OnAttach(AttachToPanelEvent evt)
        {
            if (evt.target != this) return;

            LoadingScreenUtility.RegisterLoadProgressListener(this);
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            if (evt.target != this) return;

            LoadingScreenUtility.UnregisterLoadProgressListener(this);
        }

        #endregion

        #region Progress Handling

        public void OnProgressChanged(ILoadProgressData progress)
        {
            if (!enabledSelf || !enabledInHierarchy || progress == null || !allowedTypes.Contains(progress.GetType()))
                return;

            if (progress is SceneLoadProgressData sceneLoad &&
                sceneLoad.operationKind == SceneOperationKind.Unload && !trackUnloadingScenes)
                return;

            value = progress.value;
        }

        #endregion

        #region Helpers

        private void CacheAllowedTypes()
        {
            allowedTypes.Clear();

            if (includeTypes == null || includeTypes.Length == 0)
                return;

            foreach (var typeName in includeTypes.Select(UnityWebRequest.UnEscapeURL))
            {
                var type = Type.GetType(typeName);
                if (type != null)
                    allowedTypes.Add(type);
            }
        }

        protected override bool AreValuesEqual(float newValue, float oldValue) =>
            Mathf.Approximately(newValue, oldValue);

        #endregion
    }
}
