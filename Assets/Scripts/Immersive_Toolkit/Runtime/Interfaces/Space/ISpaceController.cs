using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Immersive_Toolkit.Runtime
{
    public interface ISpaceController
    {
        public void BindObjects();
        public void BindObjects(Action callback);
        public void Initialise();
        public void Initialise(Action callback);
        public void CreateObjects();
        public void CreateObjects(Action callback);
        public void Prepare();
        public void Prepare(Action callback);
        public void Begin();
        public void Begin(Action callback);
    }
}