using System;
using System.Linq;
using LoadingScreenKit.Interfaces;
using UnityEngine;
using UnityEngine.UIElements;

namespace LoadingScreenKit.Components.ProgressBars
{
    [UxmlElement]
    public partial class TexturedArcProgressBar : VisualElement, IControllerListener
    {
        public static readonly string ussClassName = "textured-arc-progress-bar_container";
        public static readonly string spinner_ussClassName = "textured-arc-progress-bar_spinner";

        [UxmlAttribute, SerializeField]
        private Texture2D texture;

        [UxmlAttribute, SerializeField]
        private Color tint = Color.white;

        [UxmlAttribute, Tooltip("Specifies the smoothing speed for progress updates. Set to a value greater than 0 to enable smoothing.")]
        [SerializeField] protected float smoothing = 0.05f;

        private float targetValue;

        private float _value;
        public float Value
        {
            get => _value;
            set
            {
                var newValue = Mathf.Clamp01(value);

                if (Mathf.Approximately(_value, newValue))
                    return;

                _value = newValue;

                spinner.MarkDirtyRepaint();
            }
        }

        private IVisualElementScheduledItem scheduler;
        private readonly VisualElement spinner;

        public TexturedArcProgressBar()
        {
            AddToClassList(ussClassName);

            spinner = new();
            spinner.AddToClassList(spinner_ussClassName);
            spinner.generateVisualContent += OnGenerateVisualContent;

            hierarchy.Add(spinner);

            RegisterCallback<AttachToPanelEvent>(OnAttach);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            if (evt.target != this) return;
            UnregisterCallback<ChangeEvent<float>>(OnChangeValue);

            scheduler?.Pause();
            scheduler = null;
        }

        private void OnAttach(AttachToPanelEvent evt)
        {
            if (evt.target != this) return;
            RegisterCallback<ChangeEvent<float>>(OnChangeValue);

            scheduler = schedule.Execute(UpdateValue).Every(16).StartingIn(0);
        }

        private void OnChangeValue(ChangeEvent<float> evt)
        {
            if (evt.target != this) return;
            targetValue = Mathf.Clamp01(evt.newValue);
        }

        private void UpdateValue(TimerState state) =>
            Value = (smoothing > 0) ? Mathf.MoveTowards(Value, targetValue, smoothing / 1000 * state.deltaTime) : targetValue;

        #region Rendering

        private void OnGenerateVisualContent(MeshGenerationContext ctx)
        {
            if (texture == null)
                return;

            float size = contentRect.width; // we will make it uniform

            if (size < 0.01f) return; // too small

            float angle = Value * 360f;  // Convert the value (0-1) into degrees (0-360)

            if (Mathf.Approximately(angle, 0f))
            {
                return; // no need to render any
            }

            if (Mathf.Approximately(angle, 360f))
            {
                // Since it's full, we can skip all extra verts, just make a rect
                RenderFullRect(size, ctx);
                return;
            }

            RenderMesh(size, angle, ctx);
        }


        private void RenderFullRect(float size, MeshGenerationContext ctx)
        {
            Vector3[] vertices = new Vector3[4]
            {
                new (0, 0, 0),  // bottom-left
                new (1, 0, 0),  // bottom-right
                new (0, 1, 0),  // top-left
                new (1, 1, 0)   // top-right
            };

            Vertex[] vertexArray = new Vertex[4];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertexArray[i] = new Vertex
                {
                    position = vertices[i] * size,
                    uv = InvertUV(vertices[i]),
                    tint = tint
                };
            }

            ushort[] indices = new ushort[6] { 0, 1, 2, 2, 1, 3 };

            // finalize
            MeshWriteData mesh = ctx.Allocate(4, 6, texture);

            mesh.SetAllVertices(vertexArray);
            mesh.SetAllIndices(indices);
        }

        private void RenderMesh(float size, float angle, MeshGenerationContext ctx)
        {
            int vertices_count = Mathf.FloorToInt(angle / 45f) + 1;
            var Z = Vertex.nearZ;

            Vector3[] points = new Vector3[8]
            {
                new (0.5f, 0, Z),   // top
                new (1, 0, Z),      // top-right
                new (1, 0.5f, Z),   // right
                new (1, 1, Z),      // bot-right
                new (0.5f, 1, Z),   // bot
                new (0, 1, Z),      // bot-left
                new (0, 0.5f, Z),   // left
                new (0, 0, Z),      // top-left
            };

            Vector3 center = new(0.5f, 0.5f, Z);

            Vector3[] pointsToRender = new Vector3[vertices_count];
            for (int i = 0; i < vertices_count; i++)
            {
                pointsToRender[i] = points[i];
            }

            // Setup points and uv

            Vector3[] targets = new Vector3[1 + pointsToRender.Length];

            // Add the center as the first element
            targets[0] = center;

            // Copy the elements of pointsToRender into the new array
            Array.Copy(pointsToRender, 0, targets, 1, pointsToRender.Length);

            Vertex[] vertices = BuildVert(targets, size);

            // Add the dynamic point, unless we are on a point

            if (angle % 45 != 0)
            {
                Vertex dynamicVertex = GetDynamic(angle, points[vertices_count - 1], points[vertices_count % 8], size);

                vertices = vertices.Concat(new Vertex[] { dynamicVertex }).ToArray();
            }

            // Setup indices

            ushort[] indices = BuildIndices(vertices.Length);

            //finalize
            MeshWriteData mesh = ctx.Allocate(vertices.Length, indices.Length, texture);

            mesh.SetAllVertices(vertices);
            mesh.SetAllIndices(indices);
        }

        private Vertex GetDynamic(float angle, Vector3 start, Vector3 end, float size)
        {
            var normalized = NormalizeAngleToStep(angle);
            Vector3 lerpd = Vector3.Lerp(start, end, normalized);

            return new Vertex
            {
                position = lerpd * size,
                uv = InvertUV(lerpd),
                tint = tint
            };
        }

        private ushort[] BuildIndices(int targetCount)
        {
            // center will always be index 0
            ushort center = 0;

            // The number of triangles is (n - 2), where n is the number of points (excluding center)
            int numTriangles = targetCount - 2;

            ushort[] indices = new ushort[3 * numTriangles];

            for (ushort i = 1; i < targetCount - 1; i++)
            {
                // Each triangle: (center, i, i+1)
                int index = (i - 1) * 3;

                indices[index] = center;
                indices[index + 1] = i;
                indices[index + 2] = (ushort)(i + 1);
            }

            return indices;
        }

        private Vertex[] BuildVert(Vector3[] targets, float size)
        {
            Vertex[] vertices = new Vertex[targets.Length];

            for (int i = 0; i < targets.Length; i++)
            {
                vertices[i].position = targets[i] * size;
                vertices[i].tint = tint;
                vertices[i].uv = InvertUV(targets[i]);
            }

            return vertices;
        }


        float NormalizeAngleToStep(float angle)
        {
            var stepSize = 45;
            // Find the starting point of the step interval
            int stepNumber = (int)(angle / stepSize);
            float stepStart = stepNumber * stepSize;
            float stepEnd = stepStart + stepSize;

            // Normalize the angle within the step range
            return (angle - stepStart) / (stepEnd - stepStart);
        }

        private Vector2 InvertUV(Vector2 uv) => new(1 - uv.x, 1 - uv.y);


        #endregion
    }
}