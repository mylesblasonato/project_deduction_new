using UnityEngine;

namespace NoteSystem
{
    [RequireComponent(typeof(Camera))]
    public class NotesInteractor : MonoBehaviour
    {
        [Header("Raycast Features")]
        [SerializeField] private float rayLength = 5;
        private NoteInteractable viewableNote;
        private Camera _camera;

        void Start()
        {
            _camera = GetComponent<Camera>();
        }

        void Update()
        {
            if (Physics.Raycast(_camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f)), transform.forward, out RaycastHit hit, rayLength))
            {
                var noteItem = hit.collider.GetComponent<NoteInteractable>();
                if (noteItem != null)
                {
                    viewableNote = noteItem;
                    HighlightCrosshair(true);
                }
                else
                {
                    ClearExaminable();
                }
            }
            else
            {
                ClearExaminable();
            }

            if (viewableNote != null)
            {
                if (Input.GetKeyDown(NoteInputManager.instance.interactKey))
                {
                    viewableNote.ShowNote();
                }
            }
        }

        private void ClearExaminable()
        {
            if (viewableNote != null)
            {
                HighlightCrosshair(false);
                viewableNote = null;
            }
        }

        void HighlightCrosshair(bool on)
        {
            NoteUIManager.instance.HighlightCrosshair(on);
        }
    }
}
