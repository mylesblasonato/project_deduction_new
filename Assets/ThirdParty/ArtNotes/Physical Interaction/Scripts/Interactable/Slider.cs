using UnityEngine;

namespace ArtNotes.PhysicalInteraction
{
	public class Slider : InteractableObject
	{
		[SerializeField] bool _verticalInput, _startFromMin, _changeStartValueToo;
		[SerializeField] float _minSignal, _maxSignal, _maxDistance, _inputMultiply = 1;
		
		float _currentDistance;
		bool _isMove, _isMinBelow;
		Vector3 _minPos;
		Vector3 _maxPos => _minPos + transform.forward * _maxDistance;
		// y(x) = (x-x0)/(xMax-x0) * (yMax-y0) + y0   like as   y(x)=kx+b
		float _currentSignal => _currentDistance / _maxDistance * (_maxSignal - _minSignal) + _minSignal;

        void Start()
        {
			if (_startFromMin)
            {
				_minPos = transform.position;
				_currentDistance = 0;
			}
			else
            {
				_minPos = transform.position - transform.forward * _maxDistance;
				_currentDistance = _maxDistance;

				if (_changeStartValueToo)
                {
					var dif = _maxSignal - _minSignal;
					_maxSignal -= dif;
					_minSignal -= dif;
				}
			}
			if (_mainExecutor != null) _mainExecutor.Execute(_currentSignal);
		}

        void Update()
		{
			if (!_isMove) return;

			int minBelowKoeff = _isMinBelow ? 1 : -1;
			float add;
			if (_verticalInput)
				add = Input.GetAxis("Mouse Y");
			else
				add = Input.GetAxis("Mouse X");

			add *= _inputMultiply * minBelowKoeff;
			_currentDistance += add;
			
			_currentDistance = Mathf.Clamp(_currentDistance, 0, _maxDistance);
			transform.position = _minPos + transform.forward * _currentDistance;
			if (_mainExecutor != null) _mainExecutor.Execute(_currentSignal);
		}
		
		public override void InteractStart(RaycastHit hit)
		{
			base.InteractStart(hit);
			_isMinBelow = isMinPosBelowMax(_minPos, _maxPos, _verticalInput);
			_isMove = true;
		}
		public override void InteractEnd()
		{
			base.InteractEnd();
			_isMove = false;
		}
		
		internal override void OnDrawGizmosSelected()
		{
			base.OnDrawGizmosSelected();
			Gizmos.color = Color.green;

			if (_startFromMin)
				Gizmos.DrawLine(transform.position, transform.position + transform.forward * _maxDistance);
			else
				Gizmos.DrawLine(transform.position, transform.position - transform.forward * _maxDistance);
		}
	}
}
