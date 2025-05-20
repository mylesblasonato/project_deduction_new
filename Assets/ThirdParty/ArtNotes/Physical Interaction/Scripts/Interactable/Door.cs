using ArtNotes.PhysicalInteraction;
using ThirdParty.ArtNotes.Physical_Interaction.Scripts.Executors;
using UnityEngine;

namespace ThirdParty.ArtNotes.Physical_Interaction.Scripts.Interactable
{
    [RequireComponent(typeof(HingeJoint))]
    public class Door : InteractableObject
    {
        [SerializeField] Executor _closeDoorExecutor, _lockedExecutor;
        [SerializeField] int _force = 9;
        [SerializeField] bool _closedWhenMinLimits = true, _doorLocked = false;
        [SerializeField] string _axis = "Mouse X";
        [SerializeField] bool _isDoubleSided = true;

        Rigidbody _rigidbody;
        Transform _camera;
        HingeJoint _hinge;
        float _startLimitsMax, _startLimitsMin;
        bool _isMove = false, _lastClosed;
        Vector3 _direction;

        bool _closed()
        {
            if (_closedWhenMinLimits)
                return (int)_hinge.limits.min == (int)_hinge.angle;
            else
                return (int)_hinge.limits.max == (int)_hinge.angle;
        }

        void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _camera = Camera.main.transform;
            _hinge = GetComponent<HingeJoint>();
            _startLimitsMax = _hinge.limits.max;
            _startLimitsMin = _hinge.limits.min;

            if (!_isDoubleSided)
                _direction = HitPos;
                
            _lastClosed = !_closed();
            checkClosed();
        }

        void Update()
        {
            if (_isMove) return;

            checkClosed();
        }

        void FixedUpdate()
        {
            if (!_closed())
                if (_mainExecutor) _mainExecutor?.Execute(_hinge.angle);

            if (!_isMove) return;

            _hinge.limits = new JointLimits() { min = _startLimitsMin, max = _startLimitsMax };

            if (!_isDoubleSided)
                _direction = transform.parent.forward;

            //if (_isDoubleSided)
                //_direction = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")).Normalize();

            _rigidbody.AddForceAtPosition(_camera.forward * Input.GetAxis("Mouse Y") * _force, HitPos);
            _rigidbody.AddForceAtPosition(_camera.right * Input.GetAxis("Mouse X") * _force, HitPos);
        }

        public override void InteractStart(RaycastHit hit)
        {
            #region base InteractStart without StartExecutor
            if (_constHit)
            {
                _hitLocal = transform.position +
                    transform.right * _constHitPos.x +
                    transform.up * _constHitPos.y +
                    transform.forward * _constHitPos.z;
                _hitLocal = transform.InverseTransformPoint(_hitLocal);
            }
            else
                _hitLocal = transform.InverseTransformPoint(hit.point);
            #endregion

            if (_doorLocked)
            {
                _lockedExecutor?.Execute(0);
            }
            else
            {
                _startExecutor?.Execute(1);
                _isMove = true;
            }
        }
        public override void InteractEnd()
        {
            base.InteractEnd();
            _isMove = false;
        }

        public void SetLocked(bool locked) => _doorLocked = locked;

        void checkClosed()
        {
            bool closed = _closed();

            if (_lastClosed == closed) return;

            if (closed)
                if (_closedWhenMinLimits)
                    _hinge.limits = new JointLimits() { min = _startLimitsMin, max = _startLimitsMin };
                else
                    _hinge.limits = new JointLimits() { min = _startLimitsMax, max = _startLimitsMax };
            else
                _hinge.limits = new JointLimits() { min = _startLimitsMin, max = _startLimitsMax };

            if (_closeDoorExecutor != null)
                _closeDoorExecutor.Execute(closed ? 1 : 0);

            _lastClosed = closed;
        }
    }
}
