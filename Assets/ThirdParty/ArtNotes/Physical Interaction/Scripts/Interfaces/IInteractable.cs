using UnityEngine;

namespace ArtNotes.PhysicalInteraction.Interfaces
{
    public interface IInteractable
    {
        public virtual void InteractStart(RaycastHit hit)
        {
            
        }

        public virtual void InteractEnd()
        {
            
        }

        public virtual float GetLookSpeed()
        {
            return 1f;
        }

        public virtual PlayerLooking.HandMode GetHandMode()
        {
            // As a virtual method, we assume the implementing class defines `Hand`.
            // Returning `PlayerLooking.HandMode.canUse` as a default functionality.
            return PlayerLooking.HandMode.canUse;
        }

        public virtual Vector3 GetHandLocation()
        {
            return Vector3.zero;
        }

    }
}