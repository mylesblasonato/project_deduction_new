using System;
using System.Collections.Generic;
using System.Linq;
using AdvancedSceneManager.Core;
using AdvancedSceneManager.Loading;
using AdvancedSceneManager.Utility;
using LoadingScreenKit.Scripts;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

namespace LoadingScreenKit.Components.Controls
{
    [UxmlElement]
    public partial class TotalProgressController : ValueController<float>, ILoadProgressListener
    {
        [UxmlAttribute]
        private bool trackUnloadingScenes;

        private string[] includeTypes;
        private readonly HashSet<Type> allowedTypes = new();
        private readonly HashSet<SceneOperation> activeSceneOperations = new();
        private readonly Dictionary<Type, float> progressByType = new();

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

        public TotalProgressController()
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

            if (progress is SceneLoadProgressData sceneProgress)
            {
                if (sceneProgress.scene.isSpecial)
                    return;

                bool isLoadOperation = sceneProgress.operationKind == SceneOperationKind.Load;
                bool isUnloadOperation = sceneProgress.operationKind == SceneOperationKind.Unload && trackUnloadingScenes;

                if (isLoadOperation || isUnloadOperation)
                {
                    activeSceneOperations.Add(sceneProgress.operation);
                }
            }

            progressByType[progress.GetType()] = CalculateProgress(progress);
            value = CalculateTotalProgress();
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

        private float CalculateProgress(ILoadProgressData progress) => progress switch
        {
            SceneLoadProgressData => CalculateSceneProgress(),
            _ => progress.value
        };

        private float CalculateSceneProgress()
        {
            float totalProgress = activeSceneOperations
                .Where(operation => operation != null)
                .Sum(operation => operation.progressScope.totalProgress * operation.progressScope.operationCount);

            int totalOperations = activeSceneOperations
                .Where(operation => operation != null)
                .Sum(operation => operation.progressScope.operationCount);

            return totalOperations > 0 ? totalProgress / totalOperations : 0f;
        }

        private float CalculateTotalProgress() =>
            progressByType.Any() ? progressByType.Values.Average() : 0f;

        protected override bool AreValuesEqual(float newValue, float oldValue) =>
            Mathf.Approximately(newValue, oldValue);

        #endregion
    }
}
