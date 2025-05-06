using Unity.Cinemachine;
using UnityEngine;

namespace Immersive_Toolkit.Working
{
    public class LookAround : MonoBehaviour, ILookAround
    {
        public CinemachineVirtualCamera _cinemachineCamera; // Assign this in the inspector
        public float _rotationSpeed = 300f;
        public bool _lockCursorAtStart;

        [HideInInspector] public float _originalRotatioinSpeed = 0;

        private CinemachinePOV _povComponent;

        void Awake()
        {
            _originalRotatioinSpeed = _rotationSpeed;
            LockCursor(_lockCursorAtStart);

            InitialiseReferences();
        }

        private void InitialiseReferences()
        {
            // Get the POV component from the Cinemachine Virtual Camera
            _povComponent = _cinemachineCamera.GetCinemachineComponent<CinemachinePOV>();
        }

        private void LockCursor(bool locked)
        {
            if (locked)
                Cursor.lockState = CursorLockMode.Locked;
            else
                Cursor.lockState = CursorLockMode.None;
        }

        public void Execute(float x, float y)
        {
            if (_povComponent != null)
            {
                // Get the current Y rotation from the Cinemachine POV
                float cameraYRotation = _povComponent.m_HorizontalAxis.Value;

                // Set POV parameters
                _povComponent.m_HorizontalAxis.m_InputAxisValue = x;
                _povComponent.m_VerticalAxis.m_InputAxisValue = y;

                _povComponent.m_HorizontalAxis.m_MaxSpeed = _rotationSpeed;
                _povComponent.m_VerticalAxis.m_MaxSpeed = _rotationSpeed;

                // Apply this Y rotation to the transform of the GameObject this script is attached to
                transform.rotation = Quaternion.Euler(0, cameraYRotation, 0);
            }
        }
    }
}