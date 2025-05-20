using System.Collections;
using ArtNotes.PhysicalInteraction;
using ArtNotes.PhysicalInteraction.Interfaces;
using UnityEngine;

namespace ThirdParty.ArtNotes.Physical_Interaction.Scripts.Interactable
{
    public class InspectableObject : MonoBehaviour, IInspectable
    {
        public Vector3 _startingPosition { get; set; }
        public Quaternion _startingRotation { get; set; }
        public Quaternion _previousRotation { get; set; }
        public Quaternion _newRotation { get; set; }

        private Quaternion _playerForwardRotation;
        private Vector3 _playerPosition;

        private float _currentDistance;
        private float _minDistance = 0.2f;
        private float _maxDistance = 2;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
     
        }

        public void SetObjectToInspectLocation()
        {
            throw new System.NotImplementedException();
        }

        public void StartInspection(Vector3 playerPosition, Quaternion playerFacingRotation)
        {
            _startingPosition = transform.position;
            _startingRotation = transform.rotation;
            print(_startingPosition);
            _playerForwardRotation = playerFacingRotation;
            _playerPosition = playerPosition;
            //transform.position = playerPosition + playerFacingRotation * Vector3.forward * distanceFromPlayer;
            transform.position = (playerPosition + transform.position) / 2;
            _currentDistance = Vector3.Distance(transform.position, _playerPosition);
            GetComponent<Collider>().enabled = false;
            GetComponent<Rigidbody>().useGravity = false;
        }

        public void EndInspection()
        {
            print(transform.position);
            transform.rotation = _startingRotation;
            transform.position = _startingPosition;

            //StartCoroutine(MoveTowardsPosition(_startingPosition, _startingRotation));
            GetComponent<Collider>().enabled = true;
            GetComponent<Rigidbody>().useGravity = true;
            print(_startingPosition);
        }

        IEnumerator MoveTowardsPosition(Vector3 targetPosition, Quaternion TargetRotation)
        {
            transform.rotation = TargetRotation;
            while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, 0.1f * Time.deltaTime);
            }
            
            transform.position = targetPosition;
            yield return new WaitForSeconds(0.1f);
        }

        public void Rotate(float xRotation, float yRotation, float rotationSpeed)
        {
            print("Rotate " + xRotation + ", " + yRotation + ", " + rotationSpeed);
            // Rotate the interactable object based on mouse movement
            transform.Rotate(Vector3.up, xRotation * rotationSpeed, Space.Self);
            transform.Rotate(Vector3.right, -yRotation * rotationSpeed, Space.Self);
        }

        public void Zoom(float zoomAmount)
        {
            if (zoomAmount != 0)
            {
                _currentDistance -= zoomAmount;

                // Clamp the current distance between the minimum and maximum distances
                _currentDistance = Mathf.Clamp(_currentDistance, _minDistance, _maxDistance);

                // Update the object's position accordingly
                transform.position = _playerPosition + _playerForwardRotation * Vector3.forward * _currentDistance;
            }
        }
    }
}