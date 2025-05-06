using UnityEngine;

namespace ArtNotes.PhysicalInteraction
{
    [RequireComponent(typeof(HingeJoint))]
    public class Valve : InteractableObject
    {
        [SerializeField] int _fullRotations, _force;
        [SerializeField] bool _startFromFull = false;

        bool _interact = false;
        float _currentAngle, _targetDistance, _lastEulerZ;
        Transform _camTransform;
        Rigidbody _rb;
        HingeJoint _joint;

        float _currentRot => _currentAngle / 360;
        Vector3 _targetPos => _camTransform.position + _camTransform.forward * _targetDistance;

        private void Start()
        {
            _camTransform = Camera.main.transform;
            _rb = GetComponent<Rigidbody>();
            _joint = GetComponent<HingeJoint>();

            if (_startFromFull)
            {
                _currentAngle = _fullRotations * 360;
                if (_mainExecutor != null) _mainExecutor.Execute(_currentRot);
            }
        }

        private void FixedUpdate()
        {
            if (_interact)
            {
                Vector3 force = (_targetPos - HitPos).normalized * _force;
                _rb.AddForceAtPosition(force, HitPos, ForceMode.Force);
            }

            if (_joint.useLimits && _currentRot < _fullRotations - .1f && _currentRot > .1f)
                _joint.useLimits = false;
            else if (!_joint.useLimits && (_currentRot > _fullRotations - .1f || _currentRot < .1f))
                _joint.useLimits = true;

            var euler = transform.localEulerAngles.y;
            float deltaRot = euler - _lastEulerZ; // up 40 -> 50 ; -140 -> -130
            if (Mathf.Abs(deltaRot) < 300 && deltaRot != 0)
            {
                _currentAngle += deltaRot;
                if (_mainExecutor != null) _mainExecutor.Execute(_currentRot);
            }
            _lastEulerZ = euler;
        }

        public override void InteractStart(RaycastHit hit)
        {
            base.InteractStart(hit);
            _targetDistance = hit.distance;
            _interact = true;
        }
        public override void InteractEnd()
        {
            base.InteractEnd();
            _interact = false;
        }
    }
}
