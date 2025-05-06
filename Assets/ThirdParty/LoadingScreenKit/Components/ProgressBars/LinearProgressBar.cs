using LoadingScreenKit.Interfaces;
using UnityEngine;
using UnityEngine.UIElements;

namespace LoadingScreenKit.Components.ProgressBars
{
    [UxmlElement]
    public partial class LinearProgressBar : ProgressBar, IControllerListener
    {
        [UxmlAttribute, Tooltip("0 to disable smoothing.")]
        [SerializeField] private float smoothing = 5f;

        private IVisualElementScheduledItem scheduler;

        private float targetValue;
        public float TargetValue
        {
            get => targetValue;
            set
            {
                targetValue = value;
                if (targetValue < Value)
                    ResetBar();
            }
        }

        public LinearProgressBar()
        {
            RegisterCallbackOnce<AttachToPanelEvent>(OnAttach);
            RegisterCallbackOnce<DetachFromPanelEvent>(OnDetach);
        }

        private void OnAttach(AttachToPanelEvent evt)
        {
            if (evt.target != this) return;
            RegisterCallback<ChangeEvent<float>>(OnChangeValue);

            scheduler = schedule.Execute(UpdateValue).Every(16).StartingIn(0);
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            if (evt.target != this) return;
            UnregisterCallback<ChangeEvent<float>>(OnChangeValue);

            scheduler?.Pause();
            scheduler = null;
        }

        private void OnChangeValue(ChangeEvent<float> evt)
        {
            if (evt.target != this) return;
            TargetValue = Mathf.Clamp01(evt.newValue);
        }

        private void UpdateValue(TimerState state)
        {
            PreviewValue = TargetValue;
            Value = smoothing > 0
                ? Mathf.MoveTowards(Value, TargetValue, smoothing / 1000 * state.deltaTime)
                : TargetValue;
        }
    }
}
