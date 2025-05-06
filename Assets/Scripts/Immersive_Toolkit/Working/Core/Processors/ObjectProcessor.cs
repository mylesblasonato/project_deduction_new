using UnityEngine;

public static class ObjectProcessor
{
    public static bool HasFoundObject<T>(T component)
    {
        return component != null;
    }

    public static T FindObjectInTheScene<T>() where T : class
    {
        MonoBehaviour[] allObjects = GameObject.FindObjectsOfType<MonoBehaviour>(true); // true includes inactive
        foreach (var obj in allObjects)
        {
            if (obj is T match)
                return match;
        }

        return null;
    }

    public static Transform GetCharacterObject(object obj)
    {
        if (obj is Component component)
            return component.transform;

        Debug.LogError("Object does not inherit from Component.");
        return null;
    }

}
