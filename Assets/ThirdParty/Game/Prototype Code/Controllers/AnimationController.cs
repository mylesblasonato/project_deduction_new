using UnityEngine;

namespace Game.Prototype_Code.Controllers
{
    public class AnimationController : MonoBehaviour
    {
        public Transform targetTransform;  // The transform to animate
        public AnimationCurve movementCurve; // Curve to define movement over time
        public float amplitude = 0.5f;     // The maximum displacement along the y-axis
        public float duration = 2.0f;      // Duration of one complete cycle of the curve
        public Jump jumpScript;            // Reference to the Jump script

        private float originalY;           // Original y-position of the transform
        private float timer = 0.0f;        // Internal timer to control the curve progression
        private bool goingForward = true;  // Direction flag for ping-pong effect

        void Start()
        {
            if (targetTransform != null)
            {
                originalY = targetTransform.position.y;  // Initialize original Y position
            }
        }

        void Update()
        {
            // Check if there is no input, assuming horizontal and vertical inputs are used for movement
            bool noInput = Mathf.Approximately(Input.GetAxis("Horizontal"), 0f) && Mathf.Approximately(Input.GetAxis("Vertical"), 0f);
            float currentAmplitude = amplitude;

            // Apply damping if the character is jumping
            if (jumpScript != null && jumpScript.IsJumping())
            {
                currentAmplitude *= 0.2f;  // Reduce the amplitude by 80%
            }

            if (noInput)
            {
                // Update the timer based on the direction
                if (goingForward)
                {
                    timer += Time.deltaTime;
                    if (timer > duration)
                    {
                        timer = duration;
                        goingForward = false;
                    }
                }
                else
                {
                    timer -= Time.deltaTime;
                    if (timer < 0)
                    {
                        timer = 0;
                        goingForward = true;
                    }
                }

                // Evaluate the animation curve at the current time
                float curveValue = movementCurve.Evaluate(timer / duration);
                float newY = originalY + curveValue * currentAmplitude;

                // Apply the new position to the transform
                targetTransform.position = new Vector3(targetTransform.position.x, newY, targetTransform.position.z);
            }
            else
            {
                // Optionally reset to original position when there is input or maintain last position
                targetTransform.position = new Vector3(targetTransform.position.x, originalY, targetTransform.position.z);
                timer = 0;  // Reset timer when there is movement
                goingForward = true;  // Reset direction
            }
        }
    }
}