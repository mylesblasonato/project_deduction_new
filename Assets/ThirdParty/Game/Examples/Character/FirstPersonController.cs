using Immersive_Toolkit.Editor.Runtime;
using UnityEngine;

public class FirstPersonController : MonoBehaviour, ICharacterController
{
    public Character _character;
    public Character CharacterData 
    { 
        get => _character; 
        set => _character = value; 
    }

    public string[] _controls;
    public string[] Controls
    {
        get => _controls;
        set => _controls = value;
    }

    private IInputManager _inputManager;
    public IInputManager InputManager { get => _inputManager; set => _inputManager = value; }

    private IT_Locomotion _locomotion;

    public void Awake()
    {
        InputManager = ObjectProcessor.FindObjectInTheScene<IInputManager>();

        // Add components to this controller - must pass in IInputManager as a parameter
        _locomotion = new IT_Locomotion(this, InputManager);
    }

    public void Update()
    {
        if (ObjectProcessor.HasFoundObject(InputManager) && _locomotion.InputHasBeenReceived)
            _locomotion.IsAllowed = true;
    }

    public void FixedUpdate()
    {
        if (_locomotion.IsAllowed)
            _locomotion.ExecuteWithTheFollowing(_character.LocomotionSpeed);
    }
}