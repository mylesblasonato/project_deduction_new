using Game.Prototype_Code.Controllers;
using Game.Prototype_Code.Inheritance;
using Game.Prototype_Code.Managers;
using UltEvents;
using UnityEngine;

namespace Game.Prototype_Code
{
    public class UseInteractable : Interactable
    {
        public ReadController _readController;
        public UltEvent _onUse;

        public override void Interact(Transform raycastObject)
        {      
            Debug.Log($"Interacted with {raycastObject.gameObject.name}");

            // Trigger the event with the hit object
            raycastObject.GetComponentInChildren<MeshRenderer>().enabled = false;
            Note interactableObject = raycastObject.gameObject.GetComponentInChildren<Note>();
            if (interactableObject == null) return;   
            UnityInputManager.Instance.isUIOpen = true;
            UIManager.Instance._cursor.LockCursor(false);
            GameManager.Instance.PauseControls(true);
            raycastObject.gameObject.GetComponentInChildren<InspectableObject>()._isInspecting = false;
            _readController.Read(interactableObject);
            GetComponent<InspectInteractable>()._holdingItem = false;
            GetComponent<InspectInteractable>()._inspecting = false;
            GetComponent<InspectInteractable>().StopInteract();      
            _onUse?.Invoke();
        }
    }
}