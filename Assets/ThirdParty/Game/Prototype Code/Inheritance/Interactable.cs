using UnityEngine;

namespace Game.Prototype_Code.Inheritance
{
    public class Interactable : MonoBehaviour
    {
        public Transform _heldItem; 

        public virtual void Interact(Transform raycastObject)
        {

        }
    }
}