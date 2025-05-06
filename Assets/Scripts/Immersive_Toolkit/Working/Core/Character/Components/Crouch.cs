using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crouch : MonoBehaviour, ICrouch
{
    public float _crouchHeight = 1.0f;           // Height of the collider when crouching
    public float _standingHeight = 2.0f;         // Normal height of the collider
    public float _crouchSpeed = 2.0f;            // Speed of transitioning to crouch
    public Transform _cameraTransform;           // Player's camera transform
    public CapsuleCollider _capsuleCollider;
    
    [HideInInspector] public float _cameraCrouchHeightReduction = 0.5f; // Height reduction for the camera when crouching
    [HideInInspector] public IJump _jumpController;                 // Jump controller component

    private float _originalCameraHeight;
    private float _originalGCHeight;
    private bool _isCrouching;

    void Start()
    {
        Initialise();
    }

    private void Update()
    {
        UpdateCrouch(_isCrouching);
    }

    private void Initialise()
    {
        _jumpController = GetComponent<IJump>();

        _originalCameraHeight = _cameraTransform.localPosition.y;
        _originalGCHeight = _jumpController.GetGroundCheck().transform.position.y;
    }

    public void Execute(bool isCrouching)
    {
        _isCrouching = !_isCrouching; // Toggle crouch state
    }

    private void UpdateCrouch(bool isCrouching)
    {
        float targetHeight = isCrouching ? _crouchHeight : _standingHeight;
        _capsuleCollider.height = Mathf.Lerp(_capsuleCollider.height, targetHeight, Time.deltaTime * _crouchSpeed);

        float targetCameraHeight = isCrouching ? _originalCameraHeight - _cameraCrouchHeightReduction : _originalCameraHeight;
        Vector3 cameraPosition = _cameraTransform.localPosition;
        cameraPosition.y =  targetCameraHeight;
        _cameraTransform.localPosition = cameraPosition;

        // Dynamically adjust the ground check position
        if (_jumpController.GetGroundCheck() != null)
        {
            if (!isCrouching)
            {
                _jumpController.GetGroundCheck().position = new Vector3(
                    _jumpController.GetGroundCheck().position.x,
                    transform.position.y - (_capsuleCollider.height / 2), // Adjust this offset to fit the model's base
                    _jumpController.GetGroundCheck().position.z);
            }
            else
            {
                _jumpController.GetGroundCheck().position = new Vector3(
                        _jumpController.GetGroundCheck().position.x,
                        transform.position.y - 0.7f,
                        _jumpController.GetGroundCheck().position.z);
            }
        }
    }
    public bool IsCrouching()
    {
        return _isCrouching;
    }
}
