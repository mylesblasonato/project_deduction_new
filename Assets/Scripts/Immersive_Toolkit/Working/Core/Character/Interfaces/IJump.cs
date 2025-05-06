using UnityEngine;

public interface IJump
{
    public void Execute(bool isJumping);
    public void GroundCheck();
    public Transform GetGroundCheck();
    public bool IsJumping();
    public bool IsGrounded();


}