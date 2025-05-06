using System.Collections.Generic;
using UnityEngine;

namespace Game.Prototype_Code.ScriptableObjects
{
    [CreateAssetMenu(fileName = "RequiredWords", menuName = "Game/Required Words", order = 1)]
    public class RequiredWords_SO : ScriptableObject
    {
        public List<string> _words;
    }
}