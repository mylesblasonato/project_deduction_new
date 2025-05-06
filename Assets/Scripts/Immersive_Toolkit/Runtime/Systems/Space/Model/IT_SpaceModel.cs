using Runemark.SCEMA;
using UnityEngine;
using UnityEngine.Serialization;

namespace Immersive_Toolkit.Runtime
{
    public class IT_SpaceModel : MonoBehaviour, ISpaceModel
    {
        [SerializeField] IGraphicsWrapper _graphicsWrapper;
        public IGraphicsWrapper GraphicsWrapper => UnityGraphicsGraphicsWrapper.Instance;

        [SerializeField] Sequence _sequence;
        public Sequence Sequence => _sequence;
    }
}