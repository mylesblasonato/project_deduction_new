using System;
using LoadingScreenKit.Interfaces;
using UnityEngine;
using UnityEngine.UIElements;

namespace LoadingScreenKit.Components.Texts
{
    [UxmlElement]
    public partial class ProgressValueText : Label, IControllerListener
    {
        [UxmlAttribute, SerializeField]
        private string textFormat = "{0}%";

        [UxmlAttribute, SerializeField]
        private float lowValue = 0f;

        [UxmlAttribute, SerializeField]
        private float highValue = 100f;

        [UxmlAttribute, SerializeField]
        private int decimalPlaces = 0;

        [Header("Increment by N every MS")]
        [UxmlAttribute, SerializeField]
        private bool useIncrementing = true;

        [UxmlAttribute, SerializeField]
        private float incrementRate = 0.1f;

        [UxmlAttribute, SerializeField]
        private long updateRateMs = 16L;


        private IVisualElementScheduledItem scheduler;

        private float _targetValue;
        private float TargetValue
        {
            get => _targetValue;
            set
            {
                _targetValue = Mathf.Lerp(lowValue, highValue, value);

                // Reset Value if TargetValue is lower
                if (_targetValue < Value)
                    Value = lowValue;
            }
        }

        private float _value;
        private float Value
        {
            get => _value;
            set
            {
                _value = value;
                text = String.Format(textFormat, value.ToString($"F{decimalPlaces}"));
            }
        }

        public ProgressValueText()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttach);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);

            TargetValue = lowValue;
            Value = lowValue;
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            if (evt.target != this) return;
            UnregisterCallback<ChangeEvent<float>>(OnValueChange);

            scheduler?.Pause();
            scheduler = null;
        }

        private void OnAttach(AttachToPanelEvent evt)
        {
            if (evt.target != this) return;
            RegisterCallback<ChangeEvent<float>>(OnValueChange);
            scheduler = schedule.Execute(OnUpdate).Every(updateRateMs).StartingIn(0);
        }

        private void OnUpdate(TimerState state)
        {
            if (!useIncrementing || Mathf.Approximately(Value, TargetValue))
            {
                Value = TargetValue;
                return;
            }

            // Increment Value towards TargetValue
            Value = Mathf.Min(Value + incrementRate, TargetValue);
        }

        private void OnValueChange(ChangeEvent<float> evt)
        {
            if (evt.target != this || !enabledInHierarchy || !enabledSelf )
                return;

            TargetValue = evt.newValue;
        }
    }
}
