public interface IIT_Locomotion
{
    // Properties
    float Speed { get; set; }
    float[] MovementAxis { get; set; }
    float SpeedMultiplier { get; set; }
    bool IsAllowed { get; set; }
    bool InputHasBeenReceived { get; }

    // Methods
    public void ExecuteWithTheFollowing(float speed);
    public void SetSpeedMultiplierOf(float multiplier);
}