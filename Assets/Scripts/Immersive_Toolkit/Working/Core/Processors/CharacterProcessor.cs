using UnityEngine;

public static class CharacterProcessor
{
    public static void UpdateMovePosition(ICharacterController characterController, IIT_Locomotion locomotion)
    {
        Transform t = ObjectProcessor.GetCharacterObject(characterController);
        Rigidbody rb = t.GetComponent<Rigidbody>();

        // Convert the input into a movement direction vector
        Vector3 converted = t.TransformDirection(
            new Vector3(locomotion.MovementAxis[0], 0, locomotion.MovementAxis[1]));
        rb.MovePosition(rb.position + converted * locomotion.Speed * locomotion.SpeedMultiplier * Time.fixedDeltaTime);
    }
}
