using Obvious.Soap;
using UnityEngine;

namespace Obvious.Soap
{
    [CreateAssetMenu(fileName = "scriptable_variable_" + nameof(Quaternion), menuName = "Soap/ScriptableVariables/"+ nameof(Quaternion))]
    public class QuaternionVariable : ScriptableVariable<Quaternion>
    {
            
    }
}
