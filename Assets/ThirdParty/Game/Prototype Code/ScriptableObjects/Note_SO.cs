using UnityEngine;

namespace Game.Prototype_Code.ScriptableObjects
{
    [CreateAssetMenu(fileName = "New Note", menuName = "Game/Note", order = 1)]
    public class Note_SO : ScriptableObject
    {
        public Sprite _image;
        public string[] _text;
    }
}