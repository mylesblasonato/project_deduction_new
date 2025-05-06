using Immersive_Toolkit.Working;
using Unity.Cinemachine;
using UnityEngine;

namespace Game.Prototype_Code
{
    public class CinemachineZoom : MonoBehaviour
    {
        [Header("Cinemachine Settings")]
        public CinemachineVirtualCamera virtualCamera; // Assign the Cinemachine camera
        public LookAround _lookAround;

        [Header("Zoom Settings")]
        public float minZoom = 20f;   // Minimum FOV
        public float maxZoom = 60f;   // Maximum FOV
        public float zoomSpeed = 5f;  // Speed of zooming (for smooth interpolation)

        public string _animatorProperty;

        private float targetFov; // The zoom level we interpolate toward

        private bool _isZoomedIn = false;

        private void Start()
        {
            if (virtualCamera == null)
            {
                Debug.LogError("❌ CinemachineZoom: No CinemachineVirtualCamera assigned!");
                return;
            }

            // Set the initial FOV
            targetFov = virtualCamera.m_Lens.FieldOfView;
        }

        private void Update()
        {
            // Smoothly interpolate toward target FOV
            virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(virtualCamera.m_Lens.FieldOfView, targetFov, Time.deltaTime * zoomSpeed);
        }

        /// <summary>
        /// Zooms in by a given amount.
        /// </summary>
        public void ZoomIn(float amount)
        {
            _isZoomedIn = true;
            targetFov = Mathf.Clamp(targetFov - amount, minZoom, maxZoom);
        }

        /// <summary>
        /// Zooms out by a given amount.
        /// </summary>
        public void ZoomOut(float amount)
        {
            _isZoomedIn = false;
            targetFov = Mathf.Clamp(targetFov + amount, minZoom, maxZoom);
        }

        /// <summary>
        /// Toggle Zoom
        /// </summary>
        public void ToggleZoom(float amount)
        {
            if (_isZoomedIn)
            {
                targetFov = Mathf.Clamp(targetFov + amount, minZoom, maxZoom);
                _isZoomedIn = false;
                //virtualCamera.enabled = true;
                //UIManager.Instance._cursor.LockCursor(false);
            }
            else
            {
                targetFov = Mathf.Clamp(targetFov - amount, minZoom, maxZoom);
                _isZoomedIn = true;
                // virtualCamera.enabled = false;
                //UIManager.Instance._cursor.LockCursor(true);
            }
        }
    }
}