using Game.Prototype_Code.Inheritance;
using UnityEngine;

namespace Game.Prototype_Code.Controllers
{
    public class UI_CursorController : MonoUIObject
    {
        public RectTransform _uiCursor; // The UI element to move
        public Canvas _canvas;          // The Canvas containing the UI element

        private bool _isHidden = false;
        private bool _isLocked = false;

        void Update()
        {
            UpdateCursorPosition();
        }

        public void UpdateCursorPosition()
        {
            if (_uiCursor == null || _canvas == null)
                return;

            // Get the mouse position in screen space
            UnityEngine.Vector2 mousePosition = InputManagerInstance().GetMousePosition();

            // Convert the mouse position to the local position of the RectTransform
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.transform as RectTransform, // The parent RectTransform (Canvas)
                mousePosition,                      // The screen-space mouse position
                _canvas.worldCamera,                // The camera rendering the Canvas
                out UnityEngine.Vector2 localPoint              // Output local point in the Canvas
            );

            // Update the curosor's position
            _uiCursor.localPosition = localPoint;
        }

        public override void HideCursor(bool hide)
        {
            Cursor.visible = !hide;
            _isHidden = hide;
        }

        public override void LockCursor(bool lockCursor)
        {
            Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
            _isLocked = lockCursor;
        }
    }
}
