using UnityEngine;

namespace ArtNotes.PhysicalInteraction
{
    [RequireComponent(typeof(Rigidbody))]
    public class Box : InteractableObject
    {
        [SerializeField] float _maxForce = 50f; // Maximum force to push the object
        [SerializeField] float _maxTorque = 15f; // Maximum torque to rotate the object
        [SerializeField] float _moveSmoothness = 10f; // Smoothness factor to control force application
        [SerializeField] Collider _connectedTrigger;
        [SerializeField] bool _onlyOneTimeConnect;

        bool _oneTimeConnected, _avoidTriggerFix;
        bool _isMove;
        Rigidbody _rb;
        Transform _camTransform;
        float _targetDistanceToCam;

        Vector3 _targetPos => _camTransform.position + _camTransform.forward * _targetDistanceToCam;

        void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _camTransform = Camera.main.transform;

            if (_connectedTrigger != null && !_connectedTrigger.isTrigger)
                _connectedTrigger.isTrigger = true;
        }

        void FixedUpdate()
        {
            if (!_isMove) return;

            // Calculate the direction and distance to the target position
            Vector3 toTarget = _targetPos - _rb.position;

            // Apply force to move the object toward the target position
            Vector3 force = toTarget.normalized * _maxForce;
            _rb.AddForce(Vector3.Lerp(Vector3.zero, force, Time.fixedDeltaTime * _moveSmoothness), ForceMode.Force);

            // Rotate the object to face the target
            Quaternion targetRotation = Quaternion.LookRotation(_camTransform.forward);
            Quaternion deltaRotation = targetRotation * Quaternion.Inverse(_rb.rotation);
            deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);

            // Limit excessive rotational torque
            angle = Mathf.Clamp(angle, -_maxTorque, _maxTorque);
            _rb.AddTorque(axis * (angle * Mathf.Deg2Rad), ForceMode.Force);
        }

        public override void InteractStart(RaycastHit hit)
        {
            base.InteractStart(hit);
            if (_onlyOneTimeConnect && _oneTimeConnected) return;

            if (_rb.isKinematic)
            {
                _avoidTriggerFix = true;
                _rb.isKinematic = false; // Allow physical interaction
                if (_mainExecutor != null) _mainExecutor.Execute(0);
            }

            _targetDistanceToCam = hit.distance; // Set the desired distance to the camera
            _isMove = true; // Enable movement logic
        }

        public override void InteractEnd()
        {
            base.InteractEnd();

            // Allow natural physics and velocity to handle object after interaction
            _isMove = false;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other != _connectedTrigger) return;
            if (_avoidTriggerFix)
            {
                _avoidTriggerFix = false;
                return;
            }

            _rb.isKinematic = true; // Make the rigidbody static on connecting to the trigger
            InteractEnd(); // End interaction logic

            transform.position = other.transform.position; // Snap position to trigger
            transform.rotation = other.transform.rotation; // Snap rotation to trigger

            if (_mainExecutor != null) _mainExecutor.Execute(1);
            if (_onlyOneTimeConnect) _oneTimeConnected = true; // Prevent re-use if limited to one-time connection
        }
    }
}