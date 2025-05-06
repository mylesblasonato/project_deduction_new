using Runemark.SCEMA;
using UnityEngine;

namespace Immersive_Toolkit.Runtime
{
    public interface ISpaceModel
    {
        public Sequence Sequence { get; }
        public IGraphicsWrapper GraphicsWrapper { get; }
    }
}