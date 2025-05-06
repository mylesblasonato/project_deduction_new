using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace LoadingScreenKit.Components.Spinners
{
    [UxmlElement]
    public partial class TexturedArcSpinner : SpinnerElement
    {
        public static readonly string ussClassName = "textured-arc-spinner_container";
        public static readonly string spinnerUssClassName = "textured-arc-spinner_spinner";

        private static readonly Vertex[] vertices = new Vertex[3];
        private static readonly ushort[] indices = { 0, 1, 2 };

        [UxmlAttribute, SerializeField]
        private Texture2D texture;

        [UxmlAttribute, SerializeField]
        private Color tint = Color.white;

        [UxmlAttribute, SerializeField]
        private bool useStepRotation = false;

        [UxmlAttribute, SerializeField, Range(1, 360)]
        private float stepAngle = 45f;

        private float currentAngle;
        private float accumulatedAngle = 0f; // Keeps track of the accumulated angle

        private VisualElement spinner;

        public TexturedArcSpinner()
        {
            AddToClassList(ussClassName);

            spinner = new VisualElement { name = "SpinnerElement" };
            spinner.AddToClassList(spinnerUssClassName);
            hierarchy.Add(spinner);

            spinner.generateVisualContent += OnGenerateVisualContent;
        }

        protected override void OnUpdate(TimerState state)
        {
            if (!enabledSelf) return;

            UpdateRotation(state);

            spinner.style.rotate = new Rotate(currentAngle);

            MarkDirtyRepaint();
        }

        private void UpdateRotation(TimerState state)
        {
            if (useStepRotation)
            {
                UpdateStepRotation(state);
            }
            else
            {
                UpdateSmoothRotation(state);
            }
        }

        private void UpdateSmoothRotation(TimerState state)
        {
            // Smooth rotation logic (if step rotation is not enabled)
            float anglePerSecond = 360f / rotationDuration;
            float deltaTimeInSeconds = state.deltaTime / 1000f;
            currentAngle += (direction == ArcDirection.Clockwise ? 1 : -1) * anglePerSecond * deltaTimeInSeconds;
            currentAngle %= 360f;
        }

        private void UpdateStepRotation(TimerState state)
        {
            // Step-based rotation logic
            float deltaTimeInSeconds = state.deltaTime / 1000f;
            float anglePerSecond = 360f / rotationDuration;
            float angleChange = anglePerSecond * deltaTimeInSeconds;

            float directionMultiplier = direction == ArcDirection.Clockwise ? 1f : -1f;

            // Accumulate the angle change
            accumulatedAngle += angleChange * directionMultiplier;

            // Apply step rotation if stepAngle is set
            if (stepAngle > 0f)
            {
                ApplyStepRotation();
            }

            // Ensure the angle stays within 0-360 degrees
            currentAngle = Mathf.Repeat(currentAngle, 360f);

            // Reset accumulated angle to prevent drift
            accumulatedAngle = Mathf.Repeat(accumulatedAngle, stepAngle);
        }

        private void ApplyStepRotation()
        {
            // Determine the number of full steps and apply to the current angle
            int fullSteps = Mathf.FloorToInt(accumulatedAngle / stepAngle);

            if (fullSteps != 0)
            {
                // Apply full steps to current angle
                currentAngle += fullSteps * stepAngle;
                currentAngle = Mathf.Repeat(currentAngle, 360f);

                // Update remaining angle that didn’t complete a full step
                accumulatedAngle -= fullSteps * stepAngle;
            }
        }

        #region Rendering

        private void OnGenerateVisualContent(MeshGenerationContext ctx)
        {
            float size = contentRect.width; // Uniform size (width = height)
            MeshWriteData mesh = ctx.Allocate(vertices.Length, indices.Length, texture);

            SetVertex(0, new Vector3(0, 0, Vertex.nearZ), new Vector2(0, 1));                       // Top-left
            SetVertex(1, new Vector3(size, 0, Vertex.nearZ), new Vector2(1, 1));                    // Top-right
            SetVertex(2, new Vector3(size / 2, size / 2, Vertex.nearZ), new Vector2(0.5f, 0.5f));   // Center

            mesh.SetAllVertices(vertices);
            mesh.SetAllIndices(indices);
        }

        private void SetVertex(int index, Vector3 position, Vector2 uv)
        {
            vertices[index].position = position;
            vertices[index].tint = tint;
            vertices[index].uv = uv;
        }

        #endregion
    }
}
