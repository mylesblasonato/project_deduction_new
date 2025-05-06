using UnityEngine;

namespace ArtNotes.PhysicalInteraction
{
	public class Lever : InteractableObject
    {
        #region declarations
        [SerializeField] Transform _lever;
        [SerializeField] int 	_rotationSpeed = 10,
        						_minEulerX = -20, 
        						_maxEulerX = 20;
        [SerializeField] bool 	_startFromMinEulerX = true,
        						_verticalInput = true,
        						_isDiscrete = true;
        [Header("Non discrete lever")]
        [SerializeField] float _minSignal;
        [SerializeField] float _maxSignal;

        bool 	_isMove, 
        		_isMinPosBelow;
        float 	_currentEulerX, 
        		_middleEulerX, 
        		_lastPos;
        float _current01 => (_currentEulerX - _minEulerX) / (_maxEulerX - _minEulerX);
        float _currentSignal => _minSignal + _current01 * (_maxSignal - _minSignal);
        Vector3 _minPos => transform.position - transform.forward * .3f;
        Vector3 _maxPos => transform.position + transform.forward * .3f;
        #endregion

        void Start()
        {
            _middleEulerX = Mathf.Lerp(_minEulerX, _maxEulerX, .5f);
            if (_startFromMinEulerX) _currentEulerX = _minEulerX;
            else _currentEulerX = _maxEulerX;
        }

        void Update()
        {
            if (_isMove)
            {
            	float add = _isMinPosBelow ? 1 : -1;
                if (_verticalInput)
                	add *= Input.GetAxis("Mouse Y");
                else
                	add *= Input.GetAxis("Mouse X");
                
                _currentEulerX += add * Time.deltaTime * _rotationSpeed;
            }
            else
            {
            	if (_isDiscrete)
            	{
            		if (_currentEulerX < _middleEulerX) _currentEulerX -= Time.fixedDeltaTime * _rotationSpeed;
                	else _currentEulerX += Time.fixedDeltaTime * _rotationSpeed;
            	}
            }

            _currentEulerX = Mathf.Clamp(_currentEulerX, _minEulerX, _maxEulerX);
            var euler = _lever.localEulerAngles;
            _lever.localEulerAngles = new Vector3(_currentEulerX, euler.y, euler.z);

            if (_mainExecutor == null) return;
            if (_currentEulerX != _lastPos)
            {
            	if (_isDiscrete)
            	{
            		if (_currentEulerX == _maxEulerX) _mainExecutor.Execute(1);
                	else if (_currentEulerX == _minEulerX) _mainExecutor.Execute(0);
            	}
            	else _mainExecutor.Execute(_currentSignal);
                _lastPos = _currentEulerX;
            }
        }

        public override void InteractStart(RaycastHit hit)
        {
            base.InteractStart(hit);
            _isMinPosBelow = isMinPosBelowMax(_minPos, _maxPos, _verticalInput);
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
        	Gizmos.DrawWireSphere(_minPos, .1f);
        	Gizmos.DrawWireSphere(_maxPos, .1f);
            Gizmos.DrawWireSphere(HitPos, .1f);
        }
    }
}
