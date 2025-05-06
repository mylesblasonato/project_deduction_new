using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace LoadingScreenKit.Components.ProgressBars
{
    [UxmlElement]
    public abstract partial class ProgressBar : VisualElement
    {
        public static readonly string UssClassName = "progress-bar_container";
        public static readonly string PreviewUssClassName = "progress-bar_preview-bar";
        public static readonly string BarUssClassName = "progress-bar_progress-bar";

        [UxmlAttribute, SerializeField]
        private bool enablePreview = true;

        [SerializeField]
        private float _value;

        public virtual float Value
        {
            get => _value;
            set => SetValue(ref _value, value);
        }

        [SerializeField]
        private float _previewValue;

        public virtual float PreviewValue
        {
            get => _previewValue;
            set => SetValue(ref _previewValue, value);
        }

        protected readonly VisualElement PreviewBarElement;
        protected readonly VisualElement ProgressBarElement;

        protected virtual float PreviewPosition
        {
            get => PreviewBarElement.resolvedStyle.left;
            set => PreviewBarElement.style.left = value - layout.width;
        }

        protected virtual float ProgressPosition
        {
            get => ProgressBarElement.resolvedStyle.left;
            set => ProgressBarElement.style.left = value - layout.width;
        }

        public ProgressBar()
        {
            AddToClassList(UssClassName);

            // Initialize Preview Bar
            PreviewBarElement = CreateBar("PreviewBar", PreviewUssClassName);

            // Initialize Progress Bar
            ProgressBarElement = CreateBar("ProgressBar", BarUssClassName);

            RegisterCallback<GeometryChangedEvent>(_ => UpdateProgress());
        }

        private VisualElement CreateBar(string name, string className)
        {
            var bar = new VisualElement { name = name };
            bar.AddToClassList(className);
            hierarchy.Add(bar);
            return bar;
        }

        private void SetValue(ref float field, float newValue)
        {
            newValue = Mathf.Clamp01(newValue);

            if (EqualityComparer<float>.Default.Equals(field, newValue))
                return;

            field = newValue;

            UpdateProgress();
        }

        private void UpdateProgress()
        {
            float totalWidth = layout.width;

            if (float.IsNaN(totalWidth))
            {
                ResetBarPositions();
                return;
            }

            float prevPos = (enablePreview && PreviewValue > 0) ? Mathf.Lerp(0, totalWidth, PreviewValue) : -2;
            float progPos = (Value > 0) ? Mathf.Lerp(0, totalWidth, Value) : -2;

            PreviewPosition = prevPos;
            ProgressPosition = progPos;
        }

        private void ResetBarPositions()
        {
            PreviewPosition = -2;
            ProgressPosition = -2;
        }

        public void ResetBar() => ResetBarPositions();

    }
}
