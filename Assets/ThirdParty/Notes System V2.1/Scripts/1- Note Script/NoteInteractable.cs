using UnityEngine;

namespace NoteSystem
{
    public class NoteInteractable : MonoBehaviour
    {
        [SerializeField] private Note noteData = null;

        public void ShowNote()
        {
            NoteController.instance.CurrentNoteSource = NoteController.NoteSource.World;
            NoteController.instance.ShowNote(noteData);
        }
    }
}
