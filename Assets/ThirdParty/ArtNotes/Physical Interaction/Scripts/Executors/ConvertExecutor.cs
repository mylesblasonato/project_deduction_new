using System.Collections;
using UnityEngine;

namespace ArtNotes.PhysicalInteraction
{
    public class ConvertExecutor : Executor
    {
    	[SerializeField] bool _inertOutputSignal;
    	[SerializeField] float _inertSpeed, _delay, _multiply = 1;
    	[SerializeField] Executor[]	_outputExecutors;
    	
		float	_targetSignal,
				_currentSignal,
                _lastCurrentSignal;
		
		void FixedUpdate()
		{
			if (!_inertOutputSignal) return;

            if (_currentSignal < _targetSignal - _inertSpeed * Time.fixedDeltaTime)
                _currentSignal += _inertSpeed * Time.fixedDeltaTime;
            if (_currentSignal > _targetSignal + _inertSpeed * Time.fixedDeltaTime)
                _currentSignal -= _inertSpeed * Time.fixedDeltaTime;

            foreach (var e in _outputExecutors) e.Execute(_currentSignal * _multiply);
        }

        public override void Execute(float signal) => StartCoroutine(execute(signal));

        IEnumerator execute(float signal)
        {
            if (_delay > 0) yield return new WaitForSeconds(_delay);
            else yield return new WaitForEndOfFrame();

            if (_inertOutputSignal)
            {
                _targetSignal = signal * _multiply;
                _lastCurrentSignal = _targetSignal;
            }
        	else
        		foreach(var e in _outputExecutors) e.Execute(signal * _multiply);
        }

        void OnDrawGizmosSelected()
        {
            if (_outputExecutors.Length == 0) return;
            Gizmos.color = Color.blue;
            foreach (var e in _outputExecutors)
                if (e != null)
                    Gizmos.DrawLine(transform.position, e.transform.position);
        }
    }
}
