using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace NoteSystem
{
    public class NoteDisableManager : MonoBehaviour
    {
        [SerializeField] private FirstPersonController player = null;
        [SerializeField] private NotesInteractor noteInteractorScript =  null;

        [Header("Should persist?")]
        [SerializeField] private bool persistAcrossScenes = true;

        public static NoteDisableManager instance;

        void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;
                if (persistAcrossScenes)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
        }

        private void Start()
        {
            DebugReferenceCheck();
        }

        public void ToggleRaycast(bool disable)
        {
            noteInteractorScript.enabled = disable;
        }

        public void DisablePlayer(bool isActive)
        {
            player.enabled = !isActive;
            NoteUIManager.instance.HideCursor(!isActive);
            noteInteractorScript.enabled = !isActive;
        }

        void DebugReferenceCheck()
        {
            if (noteInteractorScript == null)
            {
                print("Disable Manager: Add the camera raycast script to the inspector");
            }

            if (player == null)
            {
                print("Disable Manager: Add the player reference to the inspector");
            }
        }
    }
}
