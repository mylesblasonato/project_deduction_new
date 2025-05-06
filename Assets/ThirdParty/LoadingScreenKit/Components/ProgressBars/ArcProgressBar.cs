using System;
using AdvancedSceneManager.Core;
using AdvancedSceneManager.Models;
using LoadingScreenKit.Interfaces;
using UnityEngine;
using UnityEngine.UIElements;

namespace LoadingScreenKit.Components.ProgressBars
{
    [UxmlElement]
    public partial class ArcProgressBar : VisualElement, IControllerListener
    {
        public static readonly string ussClassName = "arc-progress-bar";

        #region Attributes

        [UxmlAttribute]
        public Color color = Color.white;

        [UxmlAttribute]
        [Range(0f, 360f)]
        public float startAngle = 0f;

        [UxmlAttribute]
        [Range(0f, 360f)]
        public float length = 360f;

        [UxmlAttribute]
        public float lineWidth = 5f;

        [UxmlAttribute]
        public LineCap lineCap = LineCap.Round;

        [UxmlAttribute]
        public ArcDirection direction = ArcDirection.Clockwise; // Reuse type for animation direction aswell

        [UxmlAttribute]
        [SerializeField, Tooltip("0 to disable")]
        private float smoothing = 0.8f;

        [SerializeField]
        private float _value;

        [UxmlAttribute]
        public float Value { 
            get => _value; 
            set {
                var newValue = Mathf.Clamp01(value);

                if (Mathf.Approximately(_value, newValue))
                    return;

                _value = newValue;

                MarkDirtyRepaint();            
            } 
        }

        #endregion

        private IVisualElementScheduledItem scheduler;
        private float targetValue;

        public ArcProgressBar()
        {
            AddToClassList(ussClassName);

            RegisterCallback<AttachToPanelEvent>(OnAttach);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);

            generateVisualContent += OnGenerateVisualContent;
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

            scheduler = schedule.Execute(OnUpdate).Every(16).StartingIn(0);
        }

        // Since we deal with 0-1 here, I do /1000 so it's bit better numbers
        private void OnUpdate(TimerState state) =>
            Value = (smoothing > 0) ? Mathf.MoveTowards(Value, targetValue, smoothing / 1000 * state.deltaTime) : targetValue;

        private void OnValueChange(ChangeEvent<float> evt)
        {
            if (evt.target != this) return;

            targetValue = evt.newValue;
        }

        private void OnGenerateVisualContent(MeshGenerationContext context)
        {
            var painter = context.painter2D;

            painter.strokeColor = color;
            painter.lineWidth = lineWidth;
            painter.lineCap = lineCap;

            // Calculate start and end angles
            Angle _startAngle = Angle.Degrees(startAngle);
            float arcLength = Mathf.Lerp(0, length, Value);
            float endAngleDegrees = startAngle + arcLength;
            Angle _endAngle = Angle.Degrees(endAngleDegrees);

            painter.BeginPath();

            if (Mathf.Approximately(arcLength, 360f))
            {
                // Special case: full circle, If this is not added, a 360 deg will not render properly, so we split any value:1, 360 into 2 halfs
                painter.Arc(contentRect.center, (contentRect.size.x / 2) - (lineWidth / 2), _startAngle, Angle.Degrees(startAngle + 180), direction);
                painter.Arc(contentRect.center, (contentRect.size.x / 2) - (lineWidth / 2), Angle.Degrees(startAngle + 180), _endAngle, direction);
            }
            else
            {
                // Regular arc
                painter.Arc(contentRect.center, (contentRect.size.x / 2) - (lineWidth / 2), _startAngle, _endAngle, direction);
            }

            painter.Stroke();
        }
    }
}