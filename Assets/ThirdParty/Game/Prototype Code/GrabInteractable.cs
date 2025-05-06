using Game.Prototype_Code.Inheritance;
using UltEvents;
using UnityEngine;

namespace Game.Prototype_Code
{
    public class GrabInteractable : Interactable
    {
        public bool IsHoldingObject => heldObject != null;
        public UltEvent OnGrabObject;

        public Transform holdPoint;
        public Transform heldObject;
        public ThrowObject dropObject;
        private Transform originalParent;
        private Camera _playerCamera;

        private void Start()
        {
            _playerCamera = Camera.main; // Assign the player camera
            originalParent = transform.parent;
        }

        public Transform GetHeldObject() => heldObject;

        public override void Interact(Transform raycastObject)
        {
            if (heldObject == null && raycastObject != null)
            {
                Grab(raycastObject);
            }
        }

        private void Grab(Transform objectToGrab)
        {
            if (objectToGrab != null)
            {
                heldObject = objectToGrab; // Set the held object
            }

            dropObject = heldObject.GetChild(1).GetComponent<ThrowObject>();

            if (dropObject != null)
            {
                dropObject.StartHolding(heldObject, this); // ✅ Starts hold tracking in DropObject
            }

            ObjectAttachTo(objectToGrab, holdPoint);

            //heldObject.GetComponent<Collider>().isTrigger = true;
            heldObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionY;
            holdPoint.GetComponent<HingeJoint>().connectedBody = heldObject.GetComponent<Rigidbody>();

            Debug.Log($"🖐 Grabbed {objectToGrab.name}");
            OnGrabObject?.Invoke(); // ✅ Calls any events linked to grabbing
        }

        private void ObjectAttachTo(Transform objectToGrab, Transform arm)
        {
            objectToGrab.parent = arm;
            objectToGrab.localPosition = objectToGrab.GetChild(1).GetComponent<GrabOffset>().posOffset;
            objectToGrab.localRotation = Quaternion.identity;
            objectToGrab.localScale = objectToGrab.GetChild(1).GetComponent<GrabOffset>().sizeOffset;
        }
    }
}