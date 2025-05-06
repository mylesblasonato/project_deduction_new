using Game.Prototype_Code.Controllers;
using Game.Prototype_Code.Interfaces;
using UnityEngine;

namespace Game.Prototype_Code.Managers
{
    public class UIManager : MonoBehaviour, IUIManager
    {
        public UI_CursorController _cursor;

        public static UIManager Instance { get; private set; } // Static instance of the singleton

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); // Destroy duplicate instance
                return;
            }

            Instance = this;  // Assign the instance
        }

        void Update()
        {
            if (ClueManager.Instance._whiteboardCanvas.enabled || UnityInputManager.Instance.isUIOpen)
            {
                _cursor.HideCursor(true);
            }
            else
            {
                _cursor.HideCursor(true);
            }
        }

        public void HideCursor(bool hide)
        {
            _cursor.HideCursor(hide);
        }

        public void LockCursor(bool lockCursor)
        {
            _cursor.LockCursor(lockCursor);
        }
    }
}
