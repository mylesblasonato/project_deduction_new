using UnityEngine;

namespace ArtNotes.PhysicalInteraction
{
    public class PlayerLooking : MonoBehaviour
    {
    	public enum HandMode { canUse, grab, door, button}
    	
        [SerializeField] Transform _cameraTransform, _body;
        [SerializeField] Interactor _interactor;
        [SerializeField] float _sensitivity = 100;

        void Update()
        {
            Vector2 rotationDirection = Input.GetAxis("Mouse X") * Vector2.right + Input.GetAxis("Mouse Y") * Vector2.up;
            rotationDirection *= _sensitivity * Time.deltaTime * _interactor.LookSpeedMultiply;

            _cameraTransform.Rotate(_cameraTransform.right, -rotationDirection.y, Space.World);
            float xCameraRotation = Mathf.Clamp(_cameraTransform.rotation.eulerAngles.x, 0, 360);
            
            _cameraTransform.localRotation = Quaternion.Euler(xCameraRotation, 
                _cameraTransform.rotation.y, 
                _cameraTransform.rotation.z);

            _body.Rotate(_body.up, rotationDirection.x);
        }
    }
}