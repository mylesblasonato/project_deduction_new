using System.Collections.Generic;
using UnityEngine;

public class UnityInputManager : MonoBehaviour, IInputManager
{
    public static UnityInputManager Instance { get; private set; } // Static instance of the singleton

    [HideInInspector] public bool isUIOpen = false, isControlsOn = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy duplicate instance
            return;
        }

        Instance = this;  // Assign the instance
    }

    public IInputManager GetInstance()
    {
        return Instance;
    }

    public float GetAxis(string axis)
    {
        if (!isControlsOn || isUIOpen) return 0f;
        return Input.GetAxis(axis);
    }

    public float GetAxisRaw(string rawAxis)
    {
        return Input.GetAxisRaw(rawAxis);
    }

    public UnityEngine.Vector2 GetMousePosition()
    {
        return Input.mousePosition;
    }

    public bool GetButton(string button)
    {
        if (!isControlsOn || isUIOpen) return false;
        return Input.GetButton(button);
    }
    public bool GetButtonDown(string button)
    {
        if (!isControlsOn || isUIOpen) return false;
        return Input.GetButtonDown(button);
    }

    public bool GetButtonUp(string button)
    {
        if (!isControlsOn || isUIOpen) return false;
        return Input.GetButtonUp(button);
    }
}
