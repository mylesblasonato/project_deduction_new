using AdvancedSceneManager.Callbacks.Events;
using AdvancedSceneManager.Callbacks.Events.Utility;
using AdvancedSceneManager.Utility;
using UnityEngine;
using UnityEngine.UIElements;

namespace LoadingScreenKit.Components.Controls
{
    [UxmlElement]
    public partial class DisableOnEventController : VisualController
    {
        [UxmlAttribute]
        [SerializeField]
        private SerializableASMEventCallbackType Callbacks;

        readonly string eventKey = GuidReferenceUtility.GenerateID();

        public DisableOnEventController()
        {
            RegisterCallbackOnce<AttachToPanelEvent>(OnAttach);
            RegisterCallbackOnce<DetachFromPanelEvent>(OnDetach);
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            if (evt.target != this) return;

            Callbacks?.UnregisterGlobalCallback(eventKey);
        }

        private void OnAttach(AttachToPanelEvent evt)
        {
            if (!Application.isPlaying || evt.target != this) return;

            SetEnabled(true);

            Callbacks?.RegisterGlobalCallback(eventKey, OnEvent);
        }

        private void OnEvent(SceneOperationEventBase evt)
        {
            SetEnabled(false);
        }
    }
}