using Game.Prototype_Code.Managers;
using MPUIKIT;
using TMPro;
using UltEvents;
using UnityEngine;

namespace Game.Prototype_Code.Controllers
{
    public class ReadController : MonoBehaviour
    {
        public GameObject _readInterface;
        public MPImageBasic _image;
        public TextMeshProUGUI _text;
        public TextMeshProUGUI _pageCount;
        public UltEvent _onClose;
        public Note currentNote;
        public Transform _heldObject;

        private void Awake()
        {
            _readInterface.GetComponent<Canvas>().enabled = false;
        }

        public void Read(Note note)
        {
            if (note == null) return;
            currentNote = note;
            _heldObject = currentNote.gameObject.GetComponent<InspectableObject>()._heldItem;
            _image.sprite = note.GetImage();       
            _text.text = note.GetCurrentPage();
            _pageCount.text = (currentNote.currentPage + 1).ToString() + "/" + note._noteData._text.Length;
            _readInterface.GetComponent<Canvas>().enabled = true;
        }

        public void NextPage()
        {
            if (currentNote == null) return;
            _text.text = currentNote.NextPage();
            _image.sprite = currentNote.GetImage();
            _pageCount.text = (currentNote.currentPage + 1).ToString() + "/" + currentNote._noteData._text.Length;
        }

        public void PreviousPage()
        {
            if (currentNote == null) return;
            _text.text = currentNote.PreviousPage();
            _image.sprite = currentNote.GetImage();
            _pageCount.text = (currentNote.currentPage + 1).ToString() + "/" + currentNote._noteData._text.Length;
        }

        public void Close()
        {   
            UnityInputManager.Instance.isUIOpen = false;
            UIManager.Instance._cursor.LockCursor(true);

            if (!_heldObject)
                GameManager.Instance.PauseControls(false);

            currentNote = null;
            _readInterface.GetComponent<Canvas>().enabled = false;
            _onClose?.Invoke();
        }
    }
}
