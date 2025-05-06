using UnityEngine;
using UnityEngine.Events;

namespace ArtNotes.PhysicalInteraction
{
    public class SimpleExecutor : Executor
    {
        [SerializeField] bool _destroyObjects,
                              _enableOrDisableObjs;
        [SerializeField] UnityEvent OnExecuted;

        public override void Execute(float signal)
        {
            if (_destroyObjects) Destroy(gameObject);
            else if (_enableOrDisableObjs) gameObject.SetActive(signal >= 1);

            OnExecuted?.Invoke();
        }
    }
}
