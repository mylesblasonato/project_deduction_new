using UnityEngine;

namespace NoteSystem
{
    public class NoteInputManager : MonoBehaviour
    {
        [Header("Note Pickup Input")]
        public KeyCode interactKey;
        public KeyCode closeKey;
        public KeyCode reverseKey;

        [Header("Note Inventory Inputs")]
        public KeyCode openInventoryKey;

        [Header("Note Extra Feature Inputs")]
        public KeyCode playAudioKey;

        [Header("Trigger Inputs")]
        public KeyCode triggerInteractKey;

        [Header("Should persist?")]
        [SerializeField] private bool persistAcrossScenes = true;

        public static NoteInputManager instance;

        /// <summary>
        /// INPUTS FOR THIS SYSTEM ARE LOCATED IN: NoteInteractor and each Note controller script
        /// </summary>

        private void Awake()
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
    }
}
