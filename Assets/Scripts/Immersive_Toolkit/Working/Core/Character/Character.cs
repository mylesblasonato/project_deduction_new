using UnityEngine;

[CreateAssetMenu(fileName = "New Character", menuName = "Immersive Toolkit/Character", order = 3)]
public class Character : ScriptableObject
{
    public float _locomotionSpeed = 3;
    public float LocomotionSpeed
    {
        get { return _locomotionSpeed; }
        set { _locomotionSpeed = value; }
    }
}
