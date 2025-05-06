using UnityEngine;

public class Simple_PlayerMovement : MonoBehaviour
{
    [SerializeField] Rigidbody _body;
    [SerializeField] Vector3 _checkSpherePos;
    [SerializeField] float _checkSphereRadius = .2f, _jumpForce, _speed = 6;

    [Header("Crouch Settings")] [SerializeField]
    Transform _cameraTransform; // Reference to player camera's transform

    [SerializeField] CapsuleCollider _capsuleCollider; // Collider to modify when crouching
    [SerializeField] float _crouchHeight = 1.0f; // Height when crouching
    [SerializeField] float _standingHeight = 2.0f; // Normal standing height
    [SerializeField] float _crouchSpeed = 2.0f; // Speed of transitioning between crouch and stand
    [SerializeField] float _cameraCrouchHeightReduction = 0.5f; // Camera height offset for crouching

    private float _originalCameraHeight;
    private float _targetCameraHeight;
    private bool _isCrouching = false;
    Transform _transformBody;

    void Start()
    {
        _transformBody = _body.transform;
        _body.freezeRotation = true;

        // Initialize crouch-specific parameters
        _originalCameraHeight = _cameraTransform.localPosition.y;
        _targetCameraHeight = _originalCameraHeight;
    }

    void Update()
    {
        // Jump Logic
        if (!_isCrouching)
        {
            if (Input.GetButtonDown("Jump") &&
                Physics.CheckSphere(transform.position + _checkSpherePos, _checkSphereRadius))
                _body.AddForce(Vector3.up * _jumpForce);
        }
        else
        {
            if (Input.GetButtonDown("Jump"))
                _body.AddForce(Vector3.up * _jumpForce);
        }

        // Crouch toggle
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.C)) // Change the key binding as necessary
            ToggleCrouch();
    }

    void FixedUpdate()
    {
        // Movement Logic
        Vector3 horizontalVelocity = Input.GetAxis("Vertical") * _transformBody.forward +
                                     Input.GetAxis("Horizontal") * _transformBody.right;
        horizontalVelocity *= _speed;
        _body.linearVelocity = horizontalVelocity + _transformBody.up * _body.linearVelocity.y;

        // Smoothly update crouch state
        UpdateCrouch();
    }

    void ToggleCrouch()
    {
        _isCrouching = !_isCrouching;
        // Update target camera height based on crouch state
        _targetCameraHeight =
            _isCrouching ? _originalCameraHeight - _cameraCrouchHeightReduction : _originalCameraHeight;
    }

    void UpdateCrouch()
    {
        // Adjust capsule height
        float targetHeight = _isCrouching ? _crouchHeight : _standingHeight;
        _capsuleCollider.height = Mathf.Lerp(_capsuleCollider.height, targetHeight, Time.deltaTime * _crouchSpeed);

        // Smoothly adjust camera height
        Vector3 cameraPosition = _cameraTransform.localPosition;
        cameraPosition.y = Mathf.Lerp(cameraPosition.y, _targetCameraHeight, Time.deltaTime * _crouchSpeed);
        _cameraTransform.localPosition = cameraPosition;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position + _checkSpherePos, _checkSphereRadius);
    }
}