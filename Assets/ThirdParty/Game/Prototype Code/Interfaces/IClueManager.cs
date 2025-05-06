using Game.Prototype_Code.Inheritance;
using UnityEngine;

namespace Game.Prototype_Code.Interfaces
{
    public interface IClueManager
    {
        void AddSticky(Vector2 spawnPosition);
        void RemoveSticky(Sticky sticky);
    }
}
