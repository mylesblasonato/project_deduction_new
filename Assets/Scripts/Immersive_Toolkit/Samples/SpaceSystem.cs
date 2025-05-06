using Cysharp.Threading.Tasks;
using Immersive_Toolkit.Runtime;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Immersive_Toolkit.Samples
{
    public class SpaceSystem: MonoBehaviour
    {
        [SerializeField] IT_SpaceModel _model;
        [SerializeField] IT_SpaceView _view;
        
        public void Awake()
        { 
            Context context = new Context();

            ISpaceModel spaceModel = _model;
            
            SpaceMini exampleSpace = new SpaceMini(_model, _view, context);
        }
    }
}