using UnityEngine;

namespace Immersive_Toolkit.Runtime
{
    public class SpaceMini : ISpace
    {
        IT_SpaceModel _model;
        IT_SpaceView _view;
        IT_SpaceController _spaceController;
        Context _context;

        public SpaceMini(ISpaceModel model, ISpaceView view, IContext context)
        {
            _model = (IT_SpaceModel)model;
            _view = (IT_SpaceView)view;
            _context = (Context)context;
            _spaceController = new IT_SpaceController(_model, _view, _context);

            _model.Sequence.Enter();
            Initialise();
        }
        
        public void Initialise()
        {
            _spaceController.CreateObjects(() => CreateObjects());
        }

        private void CreateObjects()
        {
            Debug.Log("Create Objects");
            //_model.Sequence.Enter();
        }
    }
}