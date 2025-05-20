using ArtNotes.PhysicalInteraction.Interfaces;
using UnityEngine;

namespace ArtNotes.PhysicalInteraction
{
    enum InteractionState
    {
        HOLDING,
        INSPECTING,
        FREE
    }

    public class Interactor : MonoBehaviour
    {
        #region delcarations

        [Space] [SerializeField] int _distanceMax = 3;

        HandUI _hand;
        InteractionState _currentState = InteractionState.FREE;
        GameObject _interactable;
        RectTransform _handRect;
        Camera _camera;
        RaycastHit _hit;

        public float LookSpeedMultiply { get; private set; } = 1;

        #endregion

        void Start()
        {
            _camera = Camera.main;
            Cursor.lockState = CursorLockMode.Locked; // TODO displace to GameManager
            _hand = FindAnyObjectByType<HandUI>();

            if (_hand)
            {
                _hand.SetEnableImage(false);
                _handRect = _hand.GetComponent<RectTransform>();
                _handRect.position = new Vector3(Screen.width / 2, Screen.height / 2);
            }
        }

        void Update()
        {
            switch (_currentState)
            {
                case InteractionState.HOLDING:
                    HoldingState();
                    break;
                case InteractionState.INSPECTING:
                    InspectingState();
                    break;
                case InteractionState.FREE:
                    FreeState();
                    break;
                default:
                    break;
            }
        }

        void FreeState()
        {
            GameObject interactableObject;
            GameObject inspectable = null;
            bool interactableFound = false;

            if ((interactableObject = CheckForInteractable()) != null)
            {
                interactableFound = true;
                if (_hand) _hand.SetEnableImage(true);
                if (!_interactable && _hand) _hand.SetTexture(PlayerLooking.HandMode.canUse);

                if (Input.GetButtonDown("Grab"))
                {
                    HandleInteraction(interactableObject, InteractionState.HOLDING);
                    LookSpeedMultiply = _interactable.GetComponent<IInteractable>().GetLookSpeed();
                    _interactable.GetComponent<IInteractable>().InteractStart(_hit);
                    return;
                }
            }
            
            if ((inspectable = CheckForInspectable()) != null)
            {
                interactableFound = true;
                if (Input.GetButtonDown("Fire2"))
                {
                    StartInspecting(inspectable);
                    return;
                }
            }
            if (!interactableFound && _hand) _hand.SetEnableImage(false);
        }

        private void HandleInteraction(GameObject interactableObject, InteractionState newState)
        {
            _interactable = interactableObject;
            _currentState = newState;

            switch (_currentState)
            {
                case InteractionState.INSPECTING:
                    _hand.SetEnableImage(false);
                    break;
                case InteractionState.FREE:
                    if (_hand != null)
                        _hand.SetTexture(_interactable.GetComponent<IInteractable>().GetHandMode());
                    break;
                default:
                    break;
            }
            
        }

        void HoldingState()
        {
            if (_hand)
                _handRect.position =
                    _camera.WorldToScreenPoint(_interactable.GetComponent<IInteractable>().GetHandLocation());

            if (Input.GetButtonUp("Grab")) //  || _interactCurerntDistance < _maxInteractDistance
            {
                _interactable.GetComponent<IInteractable>().InteractEnd();
                _currentState = InteractionState.FREE;
                _interactable = null;
                LookSpeedMultiply = 1;
                if (_hand != null)
                {
                    _hand.SetEnableImage(false);
                    _handRect.position = new Vector3(Screen.width / 2, Screen.height / 2);
                }
            }
        }

        void InspectingState()
        {
            if (_interactable != null)
            {
                float scrollInput = Input.GetAxis("Mouse ScrollWheel");
                if (scrollInput != 0)
                {
                    _interactable.GetComponent<IInspectable>().Zoom(scrollInput);
                }
                //if (_hand) _handRect.position = _camera.WorldToScreenPoint(_interactable.GetComponent<IInspectable>().GetHandLocation());

                if (Input.GetButtonUp("Fire2")) //  || _interactCurerntDistance < _maxInteractDistance
                {
                    _interactable.GetComponent<IInspectable>().EndInspection();
                    GetComponentInParent<Simple_PlayerMovement>().EnableMovement();
                    _currentState = InteractionState.FREE;
                    _interactable = null;
                    LookSpeedMultiply = 1;
                    if (_hand != null)
                    {
                        _hand.SetEnableImage(false);
                        _handRect.position = new Vector3(Screen.width / 2, Screen.height / 2);
                    }

                    return;
                }

                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");
                _interactable.GetComponent<IInspectable>().Rotate(mouseX, mouseY, 2);
            }
        }

        GameObject CheckForInteractable()
        {
            if (Physics.Raycast(_camera.transform.position, _camera.transform.forward, out _hit, _distanceMax))
            {
                if (_hit.transform.TryGetComponent(out IInteractable interactable))
                    return _hit.transform.gameObject.gameObject;
            }

            return null;
        }

        GameObject CheckForInspectable()
        {
            if (Physics.Raycast(_camera.transform.position, _camera.transform.forward, out _hit, _distanceMax))
            {
                if (_hit.transform.TryGetComponent(out IInspectable inspectable))
                    return _hit.transform.gameObject.gameObject;
            }

            return null;
        }

        void StartInspecting(GameObject interactableObject)
        {
            HandleInteraction(interactableObject, InteractionState.INSPECTING);
            if (_hand) _hand.SetEnableImage(true);
            GetComponentInParent<Simple_PlayerMovement>().StopMovement();
            //_interactable.InteractStart(_hit);
            Vector3 forwardDirection = _camera.transform.forward;
            Quaternion rotation = Quaternion.LookRotation(forwardDirection);
            _interactable.GetComponent<IInspectable>().StartInspection( _camera.transform.position, rotation);
            LookSpeedMultiply = 0;
        }

        void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            if (_interactable == null)
                Gizmos.color = Color.white;
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(_interactable.GetComponent<IInteractable>().GetHandLocation(), .1f);
            }

            Gizmos.DrawLine(_camera.transform.position,
                _camera.transform.position + _camera.transform.forward * _distanceMax);
        }
    }
}