using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace LoadingScreenKit.Components.Spinners
{
    [UxmlElement]
    public partial class ArcSpinner : SpinnerElement
    {
        public static readonly string ussClassName = "arc-spinner";


        // This is currently required due to unity bug with converting curves...
        [UxmlAttribute]
        public AnimationCurve RotationCurve { get; set; } = new();

        [UxmlAttribute, SerializeField]
        private float LineWidth = 5f;

        [UxmlAttribute, SerializeField]
        private LineCap lineCap = LineCap.Round;

        [SerializeField]
        private Vector2 minMax = new(10, 200);

        [UxmlAttribute]
        public Vector2 MinMax
        {
            get => minMax;
            set
            {
                minMax.x = Mathf.Clamp(value.x, 0f, 360f);
                minMax.y = Mathf.Clamp(value.y, 0f, 360f);
            }
        }

        [UxmlAttribute]
        [SerializeField, Range(0, 360)]
        private float startAngle = 0f;

        [UxmlAttribute]
        [SerializeField]
        private List<Color> colors = new()
        {
            new(0.274f, 0.549f, 0.980f),
            new(0.902f, 0.196f, 0.235f),
            new(1.000f, 0.784f, 0.176f),
            new(0.118f, 0.627f, 0.373f),
            new(0.274f, 0.549f, 0.980f)
        };

        [UxmlAttribute]
        [SerializeField]
        private float colorChangeSpeed = 0.3f;

        [UxmlAttribute]
        [SerializeField]
        private float lengthChangeSpeed = 1.8f;

        private float colorTransitionProgress = 0f;
        private Color currentColor;

        private float currentAngle = 0f;
        private float currentLength = 50f;

        public ArcSpinner()
        {
            AddToClassList(ussClassName);

            generateVisualContent += OnGenerateVisualContent;

            currentAngle = startAngle;
            currentLength = MinMax.x;
            rotationElapsedTime = 0f;
        }

        protected override void OnUpdate(TimerState state)
        {
            if (!enabledSelf) return;

            if (RotationCurve == null || RotationCurve.length == 0) return;

            UpdateRotation(state);
            UpdateColor(state);
            UpdateLength(state);

            MarkDirtyRepaint();
        }

        private float rotationElapsedTime = 0f;

        // wrap any angle into the 0 - 360 range.
        private float WrapAngle(float angle)
        {
            angle %= 360f;
            if (angle < 0f) angle += 360f;
            return angle;
        }

        private void UpdateRotation(TimerState state)
        {
            // If rotationDuration is 0, we default to startAngle to avoid division by zero.
            if (rotationDuration <= 0f)
            {
                currentAngle = WrapAngle(startAngle);
                return;
            }

            rotationElapsedTime += state.deltaTime / 1000f;
            rotationElapsedTime %= rotationDuration;

            float normalizedTime = rotationElapsedTime / rotationDuration;

            float progress = RotationCurve.Evaluate(normalizedTime);

            float angleOffset = progress * 360f;

            currentAngle = direction == ArcDirection.Clockwise ? startAngle + angleOffset : startAngle - angleOffset;

            currentAngle = WrapAngle(currentAngle);
        }


        private void UpdateColor(TimerState state)
        {
            // Update color transition progress and ensure it stays between 0 and 1
            colorTransitionProgress += state.deltaTime * colorChangeSpeed / 1000f;
            colorTransitionProgress %= 1f; // Wrap around when progress exceeds 1

            // Calculate the blended index as a floating-point value
            float blendedIndex = colorTransitionProgress * colors.Count;

            // Get the two adjacent colors to blend between
            int colorIndexA = Mathf.FloorToInt(blendedIndex) % colors.Count;
            int colorIndexB = (colorIndexA + 1) % colors.Count;

            // Calculate the interpolation factor between the two colors
            float blendFactor = blendedIndex - colorIndexA;

            currentColor = Color.Lerp(colors[colorIndexA], colors[colorIndexB], blendFactor);
        }

        private float elapsedTime = 0f;
        private void UpdateLength(TimerState state)
        {
            elapsedTime += lengthChangeSpeed * state.deltaTime / 1000f;
            elapsedTime %= 2f;

            float pingPongValue = Mathf.PingPong(elapsedTime, 1f);

            currentLength = direction == ArcDirection.Clockwise
                ? Mathf.Lerp(MinMax.x, MinMax.y, pingPongValue)
                : -Mathf.Lerp(MinMax.x, MinMax.y, pingPongValue);
        }

        private void OnGenerateVisualContent(MeshGenerationContext ctx)
        {
            var painter = ctx.painter2D;

            // Set stroke properties
            painter.strokeColor = currentColor; // Use the first color for the arc
            painter.lineWidth = LineWidth;
            painter.lineCap = lineCap;

            Angle startAngle = Angle.Degrees(currentAngle);
            Angle endAngle = Angle.Degrees((currentAngle + currentLength) % 360f);

            // Draw the rotating arc
            painter.BeginPath();
            painter.Arc(contentRect.center, (contentRect.size.x / 2) - (LineWidth / 2), startAngle, endAngle, direction);
            painter.Stroke();
        }
    }


    //[Serializable]
    //public class AnimationCurveWrapper
    //{
    //    [SerializeField]
    //    private AnimationCurve curve;

    //    public AnimationCurveWrapper() =>
    //        curve = AnimationCurve.Linear(0, 0, 1, 1);

    //    public AnimationCurveWrapper(AnimationCurve curve) =>
    //        this.curve = curve ?? AnimationCurve.Linear(0, 0, 1, 1);

    //    public AnimationCurve Curve
    //    {
    //        get => curve;
    //        set => curve = value ?? AnimationCurve.Linear(0, 0, 1, 1);
    //    }
    //}

}
