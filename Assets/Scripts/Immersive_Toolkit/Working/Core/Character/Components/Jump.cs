using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump : MonoBehaviour, IJump
{
    public Rigidbody _rb;
    [HideInInspector] public Crouch _crouchScript;              // Reference to the player's jump script

    public float _jumpForce = 5.0f;           // Force applied upwards
    public LayerMask _groundLayer;            // Layer that represents the ground
    public Transform _groundCheck;            // A point representing where to check if the player is grounded
    public float _groundDistance = 0.2f;      // Distance to check for the ground

    private bool _isGrounded;
    private bool _isJumping;                  // To track if the player has initiated a jump

    private void Update()
    {
        GroundCheck();
    }

    private void FixedUpdate()
    {
        PlayerJump();
    }

    public void GroundCheck()
    {
        // Continuously check if the player is on the ground
        _isGrounded = Physics.CheckSphere(_groundCheck.position, _groundDistance, _groundLayer);
    }

    public Transform GetGroundCheck() 
    { 
        return _groundCheck; 
    }

    public void Execute(bool isJumping)
    {
        // Detect jump key press and if the player is grounded
        if (isJumping && _isGrounded)
        {
            _isJumping = true;
        }
    }

    public bool IsJumping()
    {
        return _isJumping;
    }

    public bool IsGrounded()
    {
        return _isGrounded;
    }
    private void PlayerJump()
    {
        if (!_isJumping) return;
        // Apply an upward force to simulate a jump
        _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        _isJumping = false;
    }
}
