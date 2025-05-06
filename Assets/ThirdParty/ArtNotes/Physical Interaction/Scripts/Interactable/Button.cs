using System.Collections;
using UnityEngine;

namespace ArtNotes.PhysicalInteraction
{
    public class Button : InteractableObject
    {
        [SerializeField] Transform _visualButton;
        [SerializeField] float _clickDistance = .1f;
        [SerializeField] bool _onlyOneSwitchOn, _startFromOn, _onlySwitchingOn;
        
        bool _state;
        Vector3 _startPos; // => _visualButton.position;
        Vector3 _minPos; // => _startPos - _visualButton.up * _clickDistance;

        void Start()
        {
            _state = _startFromOn;
            _startPos = _visualButton.localPosition;
            _minPos = _visualButton.localPosition - new Vector3(0,1,0) * _clickDistance;
            //_mainExecutor?.Execute(_state ? 1 : 0);
        }

        public override void InteractStart(RaycastHit hit)
        {
        	StopAllCoroutines();
            StartCoroutine(switchOn());
        }
        public override void InteractEnd()
        {
            base.InteractEnd();
        	StopAllCoroutines();
        	StartCoroutine(switchOff());
        	
        	if (_onlyOneSwitchOn && _state) return;
            if (!_onlySwitchingOn) _state = !_state;
            else if (_state == false) _state = true;
            _mainExecutor?.Execute(_state ? 1 : 0);
        }

        IEnumerator switchOn()
        {
            for (int i = 0; i < 10; i++)
            {
                _visualButton.localPosition = Vector3.Lerp(_visualButton.localPosition, _minPos, .5f);
                yield return new WaitForFixedUpdate();
            }
        }
        IEnumerator switchOff()
        {
            for (int i = 0; i < 10; i++)
            {
                _visualButton.localPosition = Vector3.Lerp(_visualButton.localPosition, _startPos, .5f);
                yield return new WaitForFixedUpdate();
            }
        }
        
        internal override void OnDrawGizmosSelected()
        {
        	base.OnDrawGizmosSelected();
        	Gizmos.color = Color.green;
        	Gizmos.DrawLine(_visualButton.position, _visualButton.position - _visualButton.forward * _clickDistance);
        }
    }
}
