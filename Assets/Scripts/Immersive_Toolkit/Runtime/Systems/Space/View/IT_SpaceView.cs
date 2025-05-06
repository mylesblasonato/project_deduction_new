using UnityEngine;

namespace Immersive_Toolkit.Runtime
{
    public class IT_SpaceView : MonoBehaviour, ISpaceView
    {
        [SerializeField] private GameObject _gameFrame;
        public GameObject GameFame
        {
            get { return _gameFrame; }
        }
    }
}