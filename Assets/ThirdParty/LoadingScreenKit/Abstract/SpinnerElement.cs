using UnityEngine;
using UnityEngine.UIElements;

namespace LoadingScreenKit.Components.Spinners
{
    [UxmlElement]
    public abstract partial class SpinnerElement : VisualElement
    {
        [UxmlAttribute]
        [SerializeField, Tooltip("1 is 1s full rotation.")]
        protected float rotationDuration = 0.8f;

        [UxmlAttribute]
        public ArcDirection direction = ArcDirection.Clockwise; // Reuse type for animation direction aswell


        public SpinnerElement()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttach_Internal);
            RegisterCallback<DetachFromPanelEvent>(OnDetach_Internal);
        }


        private IVisualElementScheduledItem scheduler;

        private void OnAttach_Internal(AttachToPanelEvent evt) => 
            scheduler = schedule.Execute(OnUpdate).Every(16).StartingIn(0);

        private void OnDetach_Internal(DetachFromPanelEvent evt)
        {
            scheduler?.Pause();
            scheduler = null;
        }

        protected virtual void OnUpdate(TimerState state) { }
    }
}