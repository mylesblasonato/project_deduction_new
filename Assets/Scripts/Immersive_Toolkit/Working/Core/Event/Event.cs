using System.Collections.Generic;
using UnityEngine;

namespace Immersive_Toolkit.Editor.Runtime
{
    [CreateAssetMenu(fileName = "New Event", menuName = "Immersive Toolkit/Event", order = 1)]
    public class Event : ScriptableObject
    {
        HashSet<EventListener> _listeners = new HashSet<EventListener>();

        public void Invoke()
        {
            foreach (var gameEventListener in _listeners)
                gameEventListener.RaiseEvent();
        }

        public void Register(EventListener gameEventListener) =>
            _listeners.Add(gameEventListener);
        public void Deregister(EventListener gameEventListener) =>
            _listeners.Remove(gameEventListener);
    }
}