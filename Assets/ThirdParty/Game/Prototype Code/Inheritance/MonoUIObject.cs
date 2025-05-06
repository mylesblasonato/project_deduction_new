using Game.Prototype_Code.Interfaces;
using UnityEngine;

namespace Game.Prototype_Code.Inheritance
{
    public class MonoUIObject : MonoBehaviour, IUIObject
    {
        public IInputManager _inputManager;

        public IInputManager InputManagerInstance()
        {
            if (_inputManager == null)
                _inputManager = UnityInputManager.Instance; // Choose your input system
            return _inputManager;
        }

        public virtual void HideCursor(bool hide)
        {
            throw new System.NotImplementedException();
        }

        public virtual void LockCursor(bool lockCursor)
        {
            throw new System.NotImplementedException();
        }
    }
}
