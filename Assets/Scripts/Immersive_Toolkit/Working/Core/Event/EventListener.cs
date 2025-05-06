using UnityEngine;
using UnityEngine.Events;

namespace Immersive_Toolkit.Editor.Runtime
{
    public class EventListener : MonoBehaviour
    {
        [SerializeField] Event _gameEvent;
        [SerializeField] UnityEvent _unityEvent;

        void Awake() => _gameEvent.Register(this);
        void OnDestroy() => _gameEvent.Deregister(this);
        public void RaiseEvent() => _unityEvent?.Invoke();

    }
}