using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sprint : MonoBehaviour, ISprint
{
    public float _sprintMultiplier = 2f;   // Multiplier to apply to speed when sprinting
    public float _acceleration = 0.5f;         // Time to reach full sprint speed

    private IIT_Locomotion _playerMovement; // Reference to the PlayerMovement script
    private float _currentMultiplier = 1f; // Current speed multiplier
    private float _targetMultiplier = 1f;  // Target speed multiplier

    private bool _isSprinting = false;

    void Start()
    {
        InitialiseReferences();
    }

    private void InitialiseReferences()
    {
        _playerMovement = GetComponent<IIT_Locomotion>();
    }

    public void Execute(bool isSprinting)
    {
        if (_playerMovement != null)
        {
            // Check if the sprint key is pressed to update target multiplier and sprinting status
            if (isSprinting)
            {
                _targetMultiplier = _sprintMultiplier;
                _isSprinting = true;  // Player is sprinting when the sprint key is held down
            }
            else
            {
                _targetMultiplier = 1f;
                _isSprinting = false; // Player is not sprinting when the sprint key is not held down
            }

            // Smoothly interpolate the current multiplier towards the target multiplier
            _currentMultiplier = Mathf.Lerp(_currentMultiplier, _targetMultiplier, Time.deltaTime / _acceleration);
            _playerMovement.SetSpeedMultiplierOf(_currentMultiplier);
        }
    }

    public bool IsSprinting()
    {
        return _isSprinting;
    }
}
