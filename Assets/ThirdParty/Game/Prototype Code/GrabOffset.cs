using UnityEngine;

namespace Game.Prototype_Code
{
    public class GrabOffset : MonoBehaviour
    {
        public Vector3 originalPosition;
        public Vector3 posOffset;
        public Vector3 sizeOffset;
        public Vector3 originalSize;
        public Vector3 rotationOffset;
        public Vector3 originalRotation;

        private void Start()
        {
            originalPosition = transform.position;
            originalSize = Vector3.one;
            originalRotation = transform.eulerAngles;
        }
    }
}