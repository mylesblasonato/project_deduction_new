using UnityEngine;

namespace ArtNotes.PhysicalInteraction
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioExecutor : Executor
    {
        [SerializeField] bool 	_playDuringChange,
        						_editVolumeBySignal,
        						_playWhenSignalPositive,
        						_playWhenSignalZero;
        [SerializeField] float _multiply = 1;

        float _currentSignal, _lastSignal;
        AudioSource _source;

        void Awake() => _source = GetComponent<AudioSource>();

        void FixedUpdate()
        {
            if (!_playDuringChange) return;

            if (_lastSignal != _currentSignal && !_source.isPlaying) _source.Play();
            else if (_lastSignal == _currentSignal && _source.isPlaying) _source.Stop();
            _lastSignal = _currentSignal;
        }

        public override void Execute(float signal)
        {
            if (!_playDuringChange)
            {
                if (signal > 0 && !_source.isPlaying) _source.Play();
                else if (signal <= 0 && _source.isPlaying) _source.Stop();
            }
            else
                _currentSignal = signal;

            if (_editVolumeBySignal && signal >= 0 && _source.isPlaying) _source.volume = signal * _multiply;
            
			if (_playWhenSignalPositive && signal >= 1) _source.Play();
            if (_playWhenSignalZero && signal < 1) _source.Play();
        }
    }
}