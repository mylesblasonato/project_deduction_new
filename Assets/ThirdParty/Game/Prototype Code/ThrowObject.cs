using UltEvents;
using UnityEngine;

namespace Game.Prototype_Code
{
    public class ThrowObject : MonoBehaviour
    {
        private Transform _interactablesObject;
        private Rigidbody _heldObjectRigidbody;
        private float _holdTime;
        private bool _isHolding;
        private GrabInteractable _grabInteractable;
        private Camera _playerCamera;

        [Header("Throw Settings")]
        public float _maxHoldTime = 2f; // Maximum time to charge the throw
        public float _maxThrowForce = 15f; // Maximum force applied to the object
        public float _throwOffset = 0.2f;
        public Transform _inspectTransform;

        public UltEvent _OnThrow;

        private void Start()
        {
            _interactablesObject = transform.parent;
            _playerCamera = Camera.main; // Ensure we have a reference to the main camera
        }

        public void StartHolding(Transform heldObject, GrabInteractable grabInteractable)
        {
            _grabInteractable = grabInteractable;

            if (heldObject == null) return;

            _isHolding = true;
            _holdTime = 0f; // Reset hold time
            _heldObjectRigidbody = heldObject.GetComponent<Rigidbody>();

            if (_heldObjectRigidbody)
            {
                _heldObjectRigidbody.transform.GetComponent<Collider>().isTrigger = false;
            }

            Debug.Log($"🖐 Started holding {heldObject.name}");
        }

        public void ReleaseHeldObject(Transform heldObject)
        {
            if (heldObject == null) return;

            _isHolding = false;
            heldObject.parent = _interactablesObject;    

            if (_heldObjectRigidbody)
            {
                _inspectTransform.GetComponent<HingeJoint>().connectedBody = null;
                _heldObjectRigidbody.constraints = RigidbodyConstraints.None;
                //_heldObjectRigidbody.isKinematic = false; // Enable physics

                // **Calculate throw force based on hold time**
                float throwStrength = Mathf.Clamp01(_holdTime / _maxHoldTime);
                float finalThrowForce = throwStrength * _maxThrowForce;

                // **Get the accurate throw direction from hand to mouse cursor**
                //Vector3 throwDirection = GetThrowDirection(_grabInteractable.holdPoint.position);
                Vector3 throwDirection = _inspectTransform.transform.forward;

                _heldObjectRigidbody.AddForce(throwDirection * finalThrowForce, ForceMode.Impulse);
                Debug.Log($"🟢 Released {heldObject.name} toward {throwDirection} with force: {finalThrowForce}");
            
                // ✅ Reset references in GrabInteractable
                if (_grabInteractable != null)
                {
                    _grabInteractable.heldObject.parent = _inspectTransform;
                    _grabInteractable.heldObject.transform.localPosition = new Vector3(0, 0, _grabInteractable.heldObject.transform.localPosition.z);
                    heldObject.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Default");
                    _OnThrow?.Invoke();
                    _grabInteractable.heldObject.parent = null;
                    _grabInteractable.heldObject.localScale = _grabInteractable.heldObject.GetChild(1).GetComponent<GrabOffset>().originalSize;
                    _grabInteractable.heldObject = null;
                    _grabInteractable.dropObject = null;             
                }
            }

            _holdTime = 0f; // **Reset hold time AFTER releasing**
        }

        private Vector3 GetThrowDirection(Vector3 handPosition)
        {
            if (_playerCamera == null) return transform.forward;

            Ray ray = _playerCamera.ScreenPointToRay(UnityInputManager.Instance.GetMousePosition());
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f)) // ✅ Raycast to detect target point
            {
                Vector3 targetPoint = hit.point;

                // ✅ Calculate throw direction from the held object's position to the target
                Vector3 throwDirection = (targetPoint - handPosition).normalized;

                // ✅ Apply manual left offset (relative to the camera's right direction)
                Vector3 leftOffset = -_playerCamera.transform.right * _throwOffset; // Adjust value if needed
                throwDirection += leftOffset;

                return throwDirection.normalized;
            }

            return _playerCamera.transform.forward; // Default if nothing is hit
        }

        private void Update()
        {
            if (_isHolding)
            {
                _holdTime += Time.deltaTime;
                _holdTime = Mathf.Clamp(_holdTime, 0f, _maxHoldTime); // **Accumulate hold time**
            }
        }
    }
}