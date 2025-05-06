using UnityEngine;

public class MouseSway : MonoBehaviour, IMouseSway
{
    public float _swayAmount = 1.0f;       // Amount of sway
    public float _swaySpeed = 2.0f;        // Speed of sway adjustment
    public float _maxSwayAmount = 1.5f;    // Maximum sway distance in units
    public Transform _otherTransform;

    private Vector3 _defaultPosition;      // Default position of the game object

    void Start()
    {
        // Store the original position of the object
        _defaultPosition = _otherTransform.localPosition;
    }

    public void Execute(float x, float y)
    {
        // Get the mouse input
        float mouseX = x;
        float mouseY = y;

        // Calculate the sway offset based on mouse input
        float swayX = Mathf.Clamp(mouseX * _swayAmount, -_maxSwayAmount, _maxSwayAmount);
        float swayY = Mathf.Clamp(mouseY * _swayAmount, -_maxSwayAmount, _maxSwayAmount);

        // Target position is offset based on mouse movement
        Vector3 targetPosition = new Vector3(
            _defaultPosition.x + swayX,
            _defaultPosition.y + swayY,
            _defaultPosition.z
        );

        // Smoothly interpolate to the target position
        _otherTransform.localPosition = Vector3.Lerp(_otherTransform.localPosition, targetPosition, Time.deltaTime * _swaySpeed);
    }
}