using UnityEngine;

namespace ArtNotes.PhysicalInteraction
{
    [RequireComponent(typeof(Light))]
    public class LightExecutor : Executor
    {
        Light _light;

        private void Awake() => _light = GetComponent<Light>();

        public override void Execute(float signal)
        {
            _light.intensity = signal;
        }
    }
}
