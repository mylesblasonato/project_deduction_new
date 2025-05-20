using UnityEngine;

namespace ArtNotes.PhysicalInteraction.Interfaces
{
    public interface IInspectable
    {
        public Vector3 _startingPosition { get; set; }
        public Quaternion _startingRotation { get; set; }
        Quaternion _previousRotation { get; set; }
        Quaternion _newRotation { get; set; }
        void SetObjectToInspectLocation();
        void StartInspection( Vector3 Location, Quaternion FacingRotation);
        public void EndInspection();
        void Rotate(float xRotation, float yRotation, float rotationSpeed);
        void Zoom(float zoomUpdate);
    }
}