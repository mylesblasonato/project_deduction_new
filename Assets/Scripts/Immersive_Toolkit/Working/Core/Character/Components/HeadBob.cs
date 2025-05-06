using UnityEngine;

public class HeadBob : MonoBehaviour, IHeadBob
{
    public Transform _playerCamera;
    public float _walkingBobbingSpeed = 0.18f; // Normal walking bob speed
    public float _walkingBobbingAmount = 0.2f; // Normal walking bob amount (vertical)
    public float _sprintingBobbingAmount = 0.35f; // Sprinting bob amount (vertical)
    public float _sprintingBobbingSpeed = 0.22f; // Sprinting bob speed
    public float _lateralSwayAmount = 0.05f; // Lateral sway amount
    public Sprint _playerSprint; // Reference to the player's sprint script
    public Jump _jumpScript; // Reference to the player's jump script

    private float _defaultPosY;
    private float _timer = 0;
    private bool _isSprinting = false;
    private bool _wasSprinting = false; // Tracks if sprinting state changed
    private float _currentBobSpeed;
    private float _currentBobAmount;
    private float _targetBobSpeed;
    private float _targetBobAmount;

    void Start()
    {
        _defaultPosY = _playerCamera.transform.localPosition.y;
        _currentBobSpeed = _walkingBobbingSpeed;
        _currentBobAmount = _walkingBobbingAmount;
        _targetBobSpeed = _walkingBobbingSpeed;
        _targetBobAmount = _walkingBobbingAmount;
        _wasSprinting = _isSprinting;
    }

    public void Execute(float x, float y)
    {
        // Stop head bob if the player is in the air
        if (_jumpScript != null && !_jumpScript.IsGrounded()) return;

        // Check if the player is sprinting
        _isSprinting = _playerSprint != null && _playerSprint.IsSprinting();
        if (_isSprinting != _wasSprinting)
        {
            // Detect change in sprinting state
            _targetBobSpeed = _isSprinting ? _sprintingBobbingSpeed : _walkingBobbingSpeed;
            _targetBobAmount = _isSprinting ? _sprintingBobbingAmount : _walkingBobbingAmount;
            _wasSprinting = _isSprinting;
        }

        // Smoothly interpolate the bobbing speed and amount to target values
        _currentBobSpeed = Mathf.Lerp(_currentBobSpeed, _targetBobSpeed, Time.deltaTime * 5);
        _currentBobAmount = Mathf.Lerp(_currentBobAmount, _targetBobAmount, Time.deltaTime * 5);

        float horizontal = x;
        float vertical = y;
        bool isMoving = Mathf.Abs(horizontal) > 0.0f || Mathf.Abs(vertical) > 0.0f;

        if (isMoving)
        {
            ApplyHeadBob();
        }
        else
        {
            ResetBob();
        }
    }

    private void ApplyHeadBob()
    {
        _timer += _currentBobSpeed * Time.deltaTime;
        float waveSlice = Mathf.Sin(_timer);
        float horizontalWaveSlice = Mathf.Cos(_timer); // For lateral sway

        if (_timer > Mathf.PI * 2)
        {
            _timer = 0; // Reset the timer at the end of each cycle
        }

        float translateChange = waveSlice * _currentBobAmount;
        float lateralChange = horizontalWaveSlice * _lateralSwayAmount;

        _playerCamera.transform.localPosition = new Vector3(
            _playerCamera.transform.localPosition.x + lateralChange,
            _defaultPosY + translateChange,
            _playerCamera.transform.localPosition.z);
    }

    private void ResetBob()
    {
        // Smoothly reset the bobbing effect to the default position
        _timer = 0;
        _playerCamera.transform.localPosition = new Vector3(
            _playerCamera.transform.localPosition.x,
            Mathf.Lerp(_playerCamera.transform.localPosition.y, _defaultPosY, Time.deltaTime * 10),
            _playerCamera.transform.localPosition.z);
    }
}