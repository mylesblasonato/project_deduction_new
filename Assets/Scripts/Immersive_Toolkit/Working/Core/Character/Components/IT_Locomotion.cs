using System;

public class IT_Locomotion : IIT_Locomotion
{
    private float _speed;
    public float Speed
    {
        get { return _speed; }
        set { _speed = value; }
    }

    private float[] _movementAxis;
    public float[] MovementAxis
    {
        get { return _movementAxis; }
        set { _movementAxis = value; }
    }

    private float _speedMultiplier = 1;
    public float SpeedMultiplier
    {
        get { return _speedMultiplier; }
        set { _speedMultiplier = value; }
    }
    
    private bool _IsAllowed;
    public bool IsAllowed
    {
        get { return _IsAllowed; }
        set { _IsAllowed = value; }
    }

    public bool InputHasBeenReceived
    {
        get
        {
            // Check deadzone
            if (Math.Abs(_inputManager.GetAxis(_characterController.Controls[0])) >= 0 || 
                Math.Abs(_inputManager.GetAxis(_characterController.Controls[1])) >= 0)
            {
                // Set the movement axis to the input axis
                MovementAxis = new float[] { 
                    _inputManager.GetAxis(_characterController.Controls[0]), 
                    _inputManager.GetAxis(_characterController.Controls[1]) };
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    private ICharacterController _characterController;
    private IInputManager _inputManager;

    public IT_Locomotion(ICharacterController cc, IInputManager im)
    {
        _characterController = cc;
        _inputManager = im;
    }

    public void SetSpeedMultiplierOf(float multiplier)
    {
        // Set the sprint multiplier, typically called from another script or input handler
        SpeedMultiplier = multiplier;
    }

    public void ExecuteWithTheFollowing(float speed)
    {
        Speed = speed;
        CharacterProcessor.UpdateMovePosition(_characterController, this);
    }
}