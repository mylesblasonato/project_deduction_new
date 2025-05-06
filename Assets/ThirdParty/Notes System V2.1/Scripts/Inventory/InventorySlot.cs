using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NoteSystem
{
    public class InventorySlot : MonoBehaviour
    {
        [SerializeField] private Image noteIcon = null;
        [SerializeField] private Button slotButton = null;
        [SerializeField] private TMP_Text buttonText = null;
        
        private Note assignedNote;

        public void AssignNote(Note note)
        {
            assignedNote = note;
            buttonText.text = note.NoteName;
            noteIcon.sprite = note.NoteIcon;

            // Add a listener to the button to show the note when clicked.
            slotButton.onClick.AddListener(ShowNoteDetails);
        }

        void ShowNoteDetails()
        {
            NoteController.instance.CurrentNoteSource = NoteController.NoteSource.Inventory;
            NoteController.instance.ShowNote(assignedNote);
        }
    }
}