using UnityEngine;

public interface ICharacterController
{
    public Character CharacterData { get; set; }
    public string[] Controls { get; set; }
    public IInputManager InputManager { get; set; }
}