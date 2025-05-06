using Game.Prototype_Code.Controllers;
using Game.Prototype_Code.Inheritance;
using Game.Prototype_Code.Managers;
using UnityEngine;

namespace Game.Prototype_Code
{
    public class InspectInteractable : Interactable
    {
        public float rotationSpeed = 5f; // Speed of rotation
        public Transform inspectContainer; // Parent container for inspection
        public bool _holdingItem = false;
        public Transform _raycastObject;
        public bool _inspecting = false;

        private InteractionController _interactionController;
        private Transform _heldObject;

        private void Start()
        {
            _interactionController = GetComponent<InteractionController>();
        }

        private void Update()
        {
            Inspecting();
        }

        private void Inspecting()
        {
            if (_inspecting)
            {
                float mouseX = UnityInputManager.Instance.GetAxisRaw("Mouse X");
                float mouseY = UnityInputManager.Instance.GetAxisRaw("Mouse Y");

                // Rotate within the inspect container instead of the object itself
                _heldObject.transform.Rotate(Vector3.up, -mouseX * rotationSpeed, Space.World); // Horizontal rotation
                _heldObject.transform.Rotate(Vector3.right, mouseY * rotationSpeed, Space.Self); // Vertical rotation

                Debug.Log("Inspecting");
            }
        }

        public void InspectHeldObject(Transform obj)
        {
            obj.parent = inspectContainer;
            obj.transform.localPosition = new Vector3(
                obj.transform.localPosition.x + obj.GetComponentInChildren<InspectableObject>().positionOffset.x,
                obj.transform.localPosition.y + obj.GetComponentInChildren<InspectableObject>().positionOffset.y,
                obj.transform.localPosition.z + obj.GetComponentInChildren<InspectableObject>().positionOffset.z);
            _heldObject = obj;
            _inspecting = true;
            _heldObject.GetComponentInChildren<InspectableObject>()._isInspecting = true;
            GameManager.Instance.PauseControls(true);

            inspectContainer.GetComponent<Rigidbody>().isKinematic = true;
            inspectContainer.GetComponent<HingeJoint>().connectedBody = null;

        }

        public void LookAtObject(Transform obj)
        {
            Debug.Log("Looking at object...");
            // Implement camera zoom-in logic here
        }

        public override void Interact(Transform raycastObject)
        {
            _raycastObject = raycastObject;
            if (raycastObject)
            {
                _holdingItem = true;
                InspectHeldObject(raycastObject);
            }
            else
            {
                _holdingItem = false;
                LookAtObject(raycastObject.transform);
            }
        }

        public void StopInteract()
        {
            if (_holdingItem)
            {
                _heldObject.parent = _interactionController._holdPoint;
                GameManager.Instance.PauseControls(false);
                _raycastObject.transform.localScale = _raycastObject.GetComponentInChildren<GrabOffset>().sizeOffset;
                _raycastObject.transform.localPosition = _raycastObject.GetComponentInChildren<GrabOffset>().posOffset;
                _inspecting = false;
                _heldObject.GetComponentInChildren<InspectableObject>()._isInspecting = false;

                inspectContainer.GetComponent<Rigidbody>().isKinematic = false;
                inspectContainer.GetComponent<HingeJoint>().connectedBody = _heldObject.GetComponent<Rigidbody>();
            }
        }

        public bool GetInspecting() { return _inspecting; }
    }
}