using Game.Prototype_Code.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Prototype_Code.Inheritance
{
    public class Sticky : MonoBehaviour, IPointerDownHandler, IDragHandler, ISticky
    {
        public RectTransform _rectTransform;    // Main note rect
        public RectTransform _resizeHandle;    // Separate resize handle UI element
        public TextMeshProUGUI _text;
        public Image _background;

        private Vector2 _dragOffset;
        private bool _isResizing;
        private bool _isDragging;
        private Vector2 _originalSize;
        private Vector2 _resizeStartPos;
        private Vector2 _originalAnchoredPosition;

        public void Initialise(string text, Color backgroundColor)
        {
            _text.text = text;
            _background.color = backgroundColor;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                // ✅ If clicking the resize handle, enter resize mode
                if (RectTransformUtility.RectangleContainsScreenPoint(_resizeHandle, eventData.position, eventData.pressEventCamera))
                {
                    _isResizing = true;
                    _isDragging = false;
                    _resizeStartPos = eventData.position;
                    _originalSize = _rectTransform.sizeDelta;
                    _originalAnchoredPosition = _rectTransform.anchoredPosition;

                    // ✅ Set pivot to top-left so resizing grows correctly
                    _rectTransform.pivot = new Vector2(0, 1);
                }
                else
                {
                    // ✅ Enter **dragging mode**
                    _isResizing = false;
                    _isDragging = true;

                    // ✅ Convert click position to local UI position
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        _rectTransform.parent as RectTransform, eventData.position, eventData.pressEventCamera, out _dragOffset
                    );

                    // ✅ Adjust offset relative to the sticky note's position
                    _dragOffset = _rectTransform.anchoredPosition - _dragOffset;
                }
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_isResizing)
            {
                ResizeSticky(eventData);
            }
            else if (_isDragging)
            {
                MoveSticky(eventData);
            }
        }

        private void ResizeSticky(PointerEventData eventData)
        {
            if (!_isResizing) return;

            Vector2 delta = eventData.position - _resizeStartPos;

            // ✅ Ensure resizing happens from the **top-left corner**
            Vector2 newSize = new Vector2(
                Mathf.Max(100, _originalSize.x + delta.x),  // Width increases right
                Mathf.Max(50, _originalSize.y - delta.y)   // Height increases downward
            );

            // ✅ Apply the new size
            _rectTransform.sizeDelta = newSize;

            // ✅ Keep the **Y position locked** (removes the unwanted movement)
            _rectTransform.anchoredPosition = new Vector2(
                _originalAnchoredPosition.x,  // Lock X position
                _originalAnchoredPosition.y   // Prevent Y movement
            );
        }

        private void MoveSticky(PointerEventData eventData)
        {
            if (!_isDragging) return;

            // ✅ Convert mouse position to local position within parent UI space
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rectTransform.parent as RectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localMousePos
            );

            // ✅ Apply position with offset
            _rectTransform.anchoredPosition = localMousePos + _dragOffset;
        }

        public void Collect()
        {
            throw new System.NotImplementedException();
        }
    }
}