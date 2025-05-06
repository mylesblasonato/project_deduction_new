using UnityEngine;

namespace Game.Prototype_Code
{
    public class Billboard : MonoBehaviour
    {
        Transform _camTransform;

        private void Awake()
        {
            _camTransform = Camera.main.transform;
        }

        // Update is called once per frame
        void LateUpdate()
        {
            transform.forward = -_camTransform.forward;
        }
    }
}
