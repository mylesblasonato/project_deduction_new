using System.Collections;
using UnityEngine;

namespace Slate.ActionClips
{

    [Category("Transform")]
    public class ScaleTo : ActorActionClip
    {

        [SerializeField]
        [HideInInspector]
        private float _length = 1;

        public Vector3 targetScale;
        public EaseType interpolation = EaseType.QuadraticInOut;

        private Vector3 originalScale;

        public override string info {
            get { return string.Format("Scale To\n{0}", targetScale); }
        }

        public override float length {
            get { return _length; }
            set { _length = value; }
        }

        public override float blendIn {
            get { return length; }
        }

        protected override void OnEnter() {
            originalScale = actor.transform.localScale;
        }

        protected override void OnUpdate(float deltaTime) {
            if ( length == 0 ) {
                actor.transform.localScale = targetScale;
                return;
            }
            actor.transform.localScale = Easing.Ease(interpolation, originalScale, targetScale, deltaTime / length);
        }

        protected override void OnReverse() {
            actor.transform.localScale = originalScale;
        }
    }
}