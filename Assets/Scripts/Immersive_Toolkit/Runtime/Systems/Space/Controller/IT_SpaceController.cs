using System;
using Cysharp.Threading.Tasks;
using Runemark.SCEMA;
using UnityEngine;

namespace Immersive_Toolkit.Runtime
{
    public class IT_SpaceController : ISpaceController
    {
        private IT_SpaceModel _model;
        private IT_SpaceView _view;
        private Context _context;
        public IGraphicsWrapper GraphicsWrapper;

        public IT_SpaceController(IT_SpaceModel model, IT_SpaceView view, Context context)
        {
            _model = model;
            _view = view;
            _context = context;
        }

        public void BindObjects()
        {
            throw new NotImplementedException();
        }

        public void BindObjects(Action callback)
        {
            callback?.Invoke();
        }

        public void Initialise()
        {
            throw new NotImplementedException();
        }

        public void Initialise(Action callback)
        {
            callback?.Invoke();
        }

        public void CreateObjects()
        {
            throw new NotImplementedException();
        }

        public void CreateObjects(Action callback)
        {
            callback?.Invoke();
        }

        public void Prepare()
        {
            throw new NotImplementedException();
        }

        public void Prepare(Action callback)
        {
            callback?.Invoke();
        }

        public void Begin()
        {
            throw new NotImplementedException();
        }

        public void Begin(Action callback)
        {
            callback?.Invoke();
        }
    }
}