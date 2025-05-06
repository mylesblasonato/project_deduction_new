using System.Collections.Generic;
using Game.Prototype_Code.Inheritance;
using Game.Prototype_Code.Interfaces;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Prototype_Code.Managers
{
    public class ClueManager : MonoUIObject, IClueManager
    {
        public CinemachineVirtualCamera _virtualCamera;
        public Canvas _whiteboardCanvas;
        public RectTransform _clueContainer;
        public GameObject _cluePrefab;
        public RectTransform _linePrefab; // Using RectTransform for UI lines
        public bool _isWhiteboardOpen;
        public float[] _bounds;
        public UltEvents.UltEvent _onOpenWhiteboard;
        public RectTransform _mouseCursor; // Mouse Cursor UI Object (Set in Inspector)

        private bool _isDrawingLine = false;
        private List<Sticky> _stickyNotes = new List<Sticky>();
        private List<(RectTransform, Sticky, Sticky)> _connections = new List<(RectTransform, Sticky, Sticky)>();

        private Sticky _selectedSticky; // Stores the first selected sticky note
        private RectTransform _currentLine; // The line being drawn

        public static ClueManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            //_whiteboardCanvas.enabled = false;
        }

        void Update()
        {   
            if (InputManagerInstance().GetInstance().GetButtonDown("Whiteboard") && !UnityInputManager.Instance.isUIOpen)
            {
                UnityInputManager.Instance.isUIOpen = false;
                OpenWhiteboard(!_isWhiteboardOpen);
                _onOpenWhiteboard?.Invoke();
            }

            DrawSticky();
            HandleRightClickConnections();
            UpdateLines(); // Keeps lines attached to sticky notes
        }

        public void RemoveSticky(Sticky sticky)
        {
            if (_stickyNotes.Contains(sticky))
            {
                RemoveConnections(sticky);
                _stickyNotes.Remove(sticky);
                Destroy(sticky.gameObject);
            }
        }

        public List<Sticky> GetAllStickyNotes()
        {
            return _stickyNotes;
        }

        public void DrawSticky()
        {
            if (UnityInputManager.Instance.GetButtonDown("Zoom"))
            {
                Sticky hoveredSticky = GetHoveredSticky();
                if (hoveredSticky != null)
                {
                    RemoveSticky(hoveredSticky);
                    return;
                }

                if (IsPointerOverUI(_clueContainer.gameObject))
                {
                    // ✅ Convert cursor position to whiteboard local space first
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        _whiteboardCanvas.GetComponent<RectTransform>(),
                        Input.mousePosition,
                        _whiteboardCanvas.worldCamera,
                        out Vector2 localMousePos
                    );

                    Debug.Log($"📌 Cursor Local Pos (Whiteboard Space): {localMousePos}");

                    AddSticky(localMousePos);
                }
            }
        }

        public void OpenWhiteboard(bool open)
        {
            //_whiteboardCanvas.enabled = open;
            _isWhiteboardOpen = open;

            UIManager.Instance._cursor.LockCursor(!open);
            GameManager.Instance.PauseControls(open);

            _onOpenWhiteboard?.Invoke();
        }

        public void AddSticky(Vector2 localWhiteboardPos)
        {
            GameObject clueObject = Instantiate(_cluePrefab, _clueContainer);
            Sticky newSticky = clueObject.GetComponent<Sticky>();
            newSticky.Initialise("New Sticky Note...", Color.white);

            // ✅ Set Pivot to Top-Left
            newSticky._rectTransform.pivot = new Vector2(0, 1);

            // ✅ Convert Whiteboard Local Space → Clue Container Local Space
            Vector3 worldPos = _whiteboardCanvas.transform.TransformPoint(localWhiteboardPos);
            Vector2 containerLocalPos = (Vector2)_clueContainer.InverseTransformPoint(worldPos);

            Debug.Log($"📦 Clue Container Pos (Before Clamping): {containerLocalPos}");

            // ✅ Offset correction: Align top-left corner
            Vector2 size = newSticky._rectTransform.sizeDelta;
            containerLocalPos -= new Vector2(size.x * 0.5f, -size.y * 0.5f);

            // ✅ Ensure Sticky Stays Inside the Whiteboard
            containerLocalPos = ClampToContainer(containerLocalPos);

            Debug.Log($"📦 Final Local Pos (Corrected & Clamped): {containerLocalPos}");

            // ✅ Apply Final Position
            newSticky._rectTransform.anchoredPosition = containerLocalPos;

            _stickyNotes.Add(newSticky);
        }

        private Sticky GetHoveredSticky()
        {
            foreach (Sticky sticky in _stickyNotes)
            {
                if (sticky != null && RectTransformUtility.RectangleContainsScreenPoint(
                        sticky._rectTransform, Input.mousePosition, _whiteboardCanvas.worldCamera))
                {
                    return sticky;
                }
            }
            return null;
        }

        private Vector2 ClampToContainer(Vector2 inputPosition)
        {
            RectTransform containerRect = _clueContainer;

            // ✅ Get container bounds dynamically
            float minX = containerRect.rect.xMin;
            float maxX = containerRect.rect.xMax;

            float minY = containerRect.rect.yMin;
            float maxY = containerRect.rect.yMax;

            // ✅ Clamp the position correctly inside these bounds
            float clampedX = Mathf.Clamp(inputPosition.x, minX, maxX);
            float clampedY = Mathf.Clamp(inputPosition.y, minY, maxY);

            Debug.Log($"📦 Clamping Input Pos: ({inputPosition.x}, {inputPosition.y})");
            Debug.Log($"📦 Clamping Bounds: X({minX} to {maxX}), Y({minY} to {maxY})");

            return new Vector2(clampedX, clampedY);
        }

        private bool IsPointerOverUI(GameObject targetObject)
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            GraphicRaycaster raycaster = _clueContainer.GetComponentInParent<GraphicRaycaster>();

            if (raycaster != null)
            {
                raycaster.Raycast(eventData, results);
                foreach (RaycastResult result in results)
                {
                    if (result.gameObject == targetObject || result.gameObject.transform.IsChildOf(targetObject.transform))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void HandleRightClickConnections()
        {
            if (UnityInputManager.Instance.GetButtonDown("Examine") && !IsPointerOverInputField())
            {
                Sticky hoveredSticky = GetHoveredSticky();
                if (hoveredSticky != null)
                {
                    _selectedSticky = hoveredSticky;
                    StartDrawingLine(_selectedSticky);
                }
            }

            if (_isDrawingLine && _currentLine != null)
            {
                // ✅ Convert mouse position to local coordinates
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _clueContainer, Input.mousePosition, _whiteboardCanvas.worldCamera, out Vector2 localMousePos
                );

                // ✅ Get the center position of the first sticky (starting point)
                Vector2 fromCenter = GetStickyCenterPosition(_selectedSticky);

                // ✅ Update the preview line dynamically from center
                UpdateUILine(_currentLine, fromCenter, localMousePos);
            }

            if (UnityInputManager.Instance.GetButtonDown("Examine") && _isDrawingLine)
            {
                Sticky targetSticky = GetHoveredSticky();
                if (targetSticky != null && targetSticky != _selectedSticky)
                {
                    CompleteConnection(_selectedSticky, targetSticky);
                }
                else
                {
                    Destroy(_currentLine.gameObject);
                }
                _isDrawingLine = false;
                _selectedSticky = null;
            }
        }

        private void StartDrawingLine(Sticky fromSticky)
        {
            _currentLine = Instantiate(_linePrefab, _clueContainer);
            _currentLine.SetAsFirstSibling();

            // ✅ Set the line pivot to (0, 0.5) so it stretches correctly
            _currentLine.pivot = new Vector2(0, 0.5f);

            // ✅ Ensure the line starts at the exact center of the first sticky
            _currentLine.anchoredPosition = GetStickyCenterPosition(fromSticky);

            _isDrawingLine = true;
        }

        private void CompleteConnection(Sticky fromSticky, Sticky toSticky)
        {
            if (_currentLine != null)
            {
                // ✅ Get exact center positions of both sticky notes
                Vector2 fromLocalPos = GetStickyCenterPosition(fromSticky);
                Vector2 toLocalPos = GetStickyCenterPosition(toSticky);

                // ✅ Draw the line perfectly from center to center
                UpdateUILine(_currentLine, fromLocalPos, toLocalPos);

                _connections.Add((_currentLine, fromSticky, toSticky));
            }
        }

        private Vector2 GetStickyCenterPosition(Sticky sticky)
        {
            // Get the world position of the sticky's center
            Vector3 worldPos = sticky._rectTransform.anchoredPosition;

            // Convert world position to local coordinates within the clue container
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _clueContainer, worldPos, _whiteboardCanvas.worldCamera, out Vector2 localCenter
            );
                
            return worldPos;
        }

        private void UpdateUILine(RectTransform line, Vector2 fromPosition, Vector2 toPosition)
        {
            Vector2 direction = toPosition - fromPosition;
            float distance = direction.magnitude;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // ✅ Set pivot to (0, 0.5) so the line starts at 'fromPosition'
            line.pivot = new Vector2(0, 0.5f);

            // ✅ Set the start position at the exact center of the first sticky
            line.anchoredPosition = fromPosition;

            // ✅ Stretch the line correctly
            line.sizeDelta = new Vector2(distance, 3);

            // ✅ Rotate it correctly
            line.localRotation = Quaternion.Euler(0, 0, angle);
        }

        private void UpdateLines()
        {
            foreach (var (line, fromSticky, toSticky) in _connections)
            {
                if (fromSticky != null && toSticky != null)
                {
                    // ✅ Pass the anchored positions of both sticky notes
                    UpdateUILine(line, fromSticky._rectTransform.anchoredPosition, toSticky._rectTransform.anchoredPosition);
                }
            }
        }

        private void RemoveConnections(Sticky sticky)
        {
            for (int i = 0; i < _connections.Count; i++)
            {
                var (line, fromSticky, toSticky) = _connections[i];

                if (fromSticky == sticky || toSticky == sticky)
                {
                    Destroy(line.gameObject);
                    _connections.RemoveAt(i);
                    i--;
                }
            }
        }

        private bool IsPointerOverInputField()
        {
            GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
            return currentSelected != null && currentSelected.GetComponent<UnityEngine.UI.InputField>() != null;
        }
    }
}