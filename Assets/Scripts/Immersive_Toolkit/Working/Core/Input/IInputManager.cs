using UnityEngine;

public interface IInputManager
{
    public IInputManager GetInstance();
    public float GetAxis(string axis);
    public float GetAxisRaw(string rawAxis);
    public UnityEngine.Vector2 GetMousePosition();
    public bool GetButton(string button);
    public bool GetButtonDown(string button);
    public bool GetButtonUp(string button);
}

