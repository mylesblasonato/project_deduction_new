using Game.Prototype_Code.Inheritance;
using UnityEngine;

namespace Game.Prototype_Code
{
    public class InspectableObject : Interactable
    {
        public Vector3 positionOffset;
        public Vector3 originalPosition;
        public bool _isInspecting;

        private void Start()
        {
            originalPosition = transform.position;
        }
    }
}
