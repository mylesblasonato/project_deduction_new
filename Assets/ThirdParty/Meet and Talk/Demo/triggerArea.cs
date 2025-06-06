using UnityEngine;
using UnityEngine.Events;

namespace MeetAndTalk.Demo
{
    public class triggerArea : MonoBehaviour
    {
        public UnityEvent OnEnter;
        public UnityEvent OnExit;

        private void OnTriggerEnter(Collider other)
        {
            OnEnter.Invoke();
        }

        private void OnTriggerExit(Collider other)
        {
            OnExit.Invoke();
        }
    }
}
