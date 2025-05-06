using Game.Prototype_Code.ScriptableObjects;
using UnityEngine;

namespace Game.Prototype_Code
{
    public class Note : UseableObject
    {
        public Note_SO _noteData;
        public int currentPage = 0;

        public string GetCurrentPage() => _noteData._text[currentPage];

        public Sprite GetImage() => _noteData._image;

        public void ResetPage() => currentPage = 0;

        public string NextPage()
        {
            if (currentPage < _noteData._text.Length - 1)
            {
                currentPage++;
            }
            return _noteData._text[currentPage];
        }

        public string PreviousPage()
        {
            if (currentPage > 0)
            {
                currentPage--;
            }
            return _noteData._text[currentPage];
        }
    }
}