using UltEvents;
using UnityEngine;

public class EventManager : MonoBehaviour, IEventManager
{
    public UltEvent OnDeath;

    public static EventManager Instance { get; private set; } // Static instance of the singleton
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy duplicate instance
            return;
        }

        Instance = this;  // Assign the instance
    }
}
