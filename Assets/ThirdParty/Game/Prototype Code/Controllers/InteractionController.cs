using Game.Prototype_Code.Managers;
using UltEvents;
using UnityEngine;

namespace Game.Prototype_Code.Controllers
{
    public class InteractionController : MonoBehaviour
    {
        [Header("Interaction Settings")]
        public float _interactionRange = 5f; // Range of the raycast
        public Transform _raycastHitObject, _holdPoint;

        public UltEvent OnRightClickDown, OnRightClickRelease;

        private Camera _playerCamera;

        private UseInteractable _useInteractable;
        private GrabInteractable _grabInteractable;
        private InspectInteractable _inspectInteractable;
        private bool _throwing;
        private bool _inspecting;
        public bool _holding;

        private void Start()
        {
            _playerCamera = Camera.main; // Assign main camera

            _useInteractable = GetComponent<UseInteractable>();
            _grabInteractable = GetComponent<GrabInteractable>();
            _inspectInteractable = GetComponent<InspectInteractable>();
        }

        void Update()
        {
            if (UnityInputManager.Instance.GetButtonDown("Interact"))
            {
                if(!_holding)
                    _raycastHitObject = Raycast();

                if (_raycastHitObject.GetComponentInChildren<UseableObject>() != null)
                    Interact();
            }

            if (UnityInputManager.Instance.GetButtonDown("Grab") && !UnityInputManager.Instance.isUIOpen && !ClueManager.Instance._isWhiteboardOpen)
            {
                if (!_grabInteractable.GetHeldObject())
                {
                    _raycastHitObject = Raycast();
                    if (_raycastHitObject != null && _raycastHitObject.CompareTag("Interactable"))
                    {
                        if (_raycastHitObject.GetChild(1).GetComponent<GrabOffset>() != null)
                            _grabInteractable.Interact(_raycastHitObject);
                    }
                }
                else
                {
                    _inspectInteractable.Interact(_raycastHitObject);
                    _inspectInteractable.StopInteract();
                    _grabInteractable.GetHeldObject().GetChild(1).GetComponent<ThrowObject>().StartHolding(_grabInteractable.GetHeldObject(), _grabInteractable);
                    _throwing = true;
                }
            }

            if (UnityInputManager.Instance.GetButtonUp("Grab") && !UnityInputManager.Instance.isUIOpen && !ClueManager.Instance._isWhiteboardOpen && _throwing)
            {
                _grabInteractable.GetHeldObject().GetChild(1).GetComponent<ThrowObject>().ReleaseHeldObject(_grabInteractable.GetHeldObject());
                _throwing = false;
            }

            if (UnityInputManager.Instance.GetButtonDown("Grab") && !UnityInputManager.Instance.isUIOpen && !ClueManager.Instance._isWhiteboardOpen)
            {
                if (_grabInteractable.GetHeldObject())
                {
                    Inspect();
                }
            }

            if (UnityInputManager.Instance.GetButtonUp("Examine"))
            {
                if (_grabInteractable.GetHeldObject())
                {
                    StopInspect();
                }
                else
                {
                    if (!UnityInputManager.Instance.isUIOpen && !ClueManager.Instance._isWhiteboardOpen)
                        OnRightClickRelease?.Invoke();
                }
            }

            if (!_grabInteractable.GetHeldObject())
            {
                if (UnityInputManager.Instance.GetButton("Examine") && !UnityInputManager.Instance.isUIOpen && !ClueManager.Instance._isWhiteboardOpen)
                {
                    OnRightClickDown?.Invoke();
                }
            }
        }

        public void ShowHeldObject()
        {
            _raycastHitObject.GetComponentInChildren<MeshRenderer>().enabled = true;
        }

        public void DropHeldObject()
        {
            _raycastHitObject = null;
            _holding = false;
        }

        public void HoldObject()
        {
            _holding = true;
        }

        private Transform Raycast()
        {
            // Raycast from the center of the screen
            Ray ray = new Ray(_playerCamera.transform.position, _playerCamera.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, _interactionRange))
            {
                return hit.transform;
            }

            return null;
        }

        private void Interact()
        {
            _useInteractable.Interact(_raycastHitObject);
        }

        private void TryGrab()
        {
            if (!_grabInteractable.GetHeldObject())
            {
                if (_raycastHitObject.GetComponent<GrabOffset>() != null)
                    _grabInteractable.Interact(_raycastHitObject);
            }
        }

        public void Inspect()
        {
            if (_holding)
            {
                _inspectInteractable._inspecting = true;
                _inspectInteractable.Interact(_raycastHitObject);
            }
        }

        public void StopInspect()
        {
            if (_grabInteractable.GetHeldObject())
            {
                _inspectInteractable.StopInteract();
            }
        }
    }
}