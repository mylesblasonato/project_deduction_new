using UnityEngine;

namespace ArtNotes.PhysicalInteraction
{
    public class TransformExecutor : Executor
    {
        [SerializeField] bool _editPosition, _editRotation, _editScale;
        [SerializeField] Vector3 _editVector;

        Vector3 _startPosition, _startRotation, _startScale;

        void Awake()
        {
            if (_editPosition) _startPosition = transform.localPosition;
            if (_editRotation) _startRotation = transform.localEulerAngles;
            if (_editScale) _startScale = transform.localScale;
        }

        public override void Execute(float signal)
        {
            if (_editPosition) transform.localPosition =    _startPosition  + _editVector * signal;
            if (_editRotation) transform.localEulerAngles = _startRotation  + _editVector * signal;
            if (_editScale) transform.localScale =          _startScale     + _editVector * signal;
        }
    }
}
