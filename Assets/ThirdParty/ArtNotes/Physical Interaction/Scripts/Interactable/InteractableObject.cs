using ArtNotes.PhysicalInteraction.Interfaces;
using ThirdParty.ArtNotes.Physical_Interaction.Scripts.Executors;
using UnityEngine;

namespace ArtNotes.PhysicalInteraction
{
    public abstract class InteractableObject : MonoBehaviour, IInteractable
    {
        public Vector3 HitPos => transform.TransformPoint(_hitLocal);

        [Range(0f, 1f)]
        public float LookingSpeed = 1;
        public PlayerLooking.HandMode Hand  = PlayerLooking.HandMode.grab; // private set

        [SerializeField] internal Executor _mainExecutor, _startExecutor, _endExecutor;
        [SerializeField] internal bool _constHit;
        [SerializeField] internal Vector3 _constHitPos;

        internal Vector3 _hitLocal;

        public virtual void InteractStart(RaycastHit hit)
        {
            if (_startExecutor) _startExecutor.Execute(1);

            if (_constHit)
            {
                _hitLocal = transform.position +
                    transform.right * _constHitPos.x +
                    transform.up * _constHitPos.y +
                    transform.forward * _constHitPos.z;
                _hitLocal = transform.InverseTransformPoint(_hitLocal);
                return;
            }
            _hitLocal = transform.InverseTransformPoint(hit.point);
        }

        public virtual void InteractEnd()
        {
            if (_endExecutor) _endExecutor.Execute(0);
        }

        public float GetLookSpeed()
        {
            return LookingSpeed;
        }

        public PlayerLooking.HandMode GetHandMode()
        {
            return Hand;
        }

        public Vector3 GetHandLocation()
        {
            return HitPos;
        }

        internal virtual void OnDrawGizmosSelected()
        {
            if (_constHit)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position +
                    transform.right * _constHitPos.x +
                    transform.up * _constHitPos.y +
                    transform.forward * _constHitPos.z, .1f);
            }

            if (_mainExecutor != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, _mainExecutor.transform.position);
            }
        }
        
        internal static bool isMinPosBelowMax(Vector3 minPos, Vector3 maxPos, bool checkVertical)
		{
			var min = Camera.main.WorldToScreenPoint(minPos);
			var max = Camera.main.WorldToScreenPoint(maxPos);
				
			if (checkVertical)
				return min.y < max.y;

			return min.x < max.x;
		}
    }
}
