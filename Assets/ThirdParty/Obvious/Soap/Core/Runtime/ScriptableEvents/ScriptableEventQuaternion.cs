using Obvious.Soap;
using UnityEngine;

namespace Obvious.Soap
{
    [CreateAssetMenu(fileName = "scriptable_event_" + nameof(Quaternion), menuName = "Soap/ScriptableEvents/"+ nameof(Quaternion))]
    public class ScriptableEventQuaternion : ScriptableEvent<Quaternion>
    {
        
    }
}
