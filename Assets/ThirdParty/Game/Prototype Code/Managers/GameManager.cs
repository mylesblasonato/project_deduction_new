using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Game.Prototype_Code.Inheritance;
using Game.Prototype_Code.ScriptableObjects;
using Immersive_Toolkit.Working;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;

namespace Game.Prototype_Code.Managers
{
    public class GameManager : MonoBehaviour
    {
        public int _currentQuestion = 0;
        [SerializedDictionary("Action", "Mapping")]
        public SerializedDictionary<string, RequiredWords_SO> _requiredWords; // Scriptable Object reference
        public TextMeshProUGUI _objective;
        public TextMeshProUGUI _checkInputText;
        public List<Sticky> _allStickyNotes;     // List of all active sticky notes
        public CinemachineVirtualCamera _virtualCamera;
    
        private LookAround _lookAround;

        public static GameManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            _lookAround = GameObject.FindGameObjectWithTag("Player").GetComponent<LookAround>();

            PauseControls(false);
            UIManager.Instance.LockCursor(true);
        }

        void Update()
        {
       
        }

        public void CheckInput()
        {
            CheckForWinCondition();
        }

        public void PauseControls(bool pause)
        {
            UnityInputManager.Instance.isControlsOn = !pause;
            _lookAround._rotationSpeed = pause ? 0 : _lookAround._originalRotatioinSpeed;
        }

        void CheckForWinCondition()
        {
            Debug.Log("🔍 Checking all sticky notes for required words...");

            QuestionManager.Instance.ProcessStickyNoteText(_checkInputText.text);

            // ✅ Check if all required words are found
            if (QuestionManager.Instance.HasAllRequiredWords())
            {
                WinGame();
            }
            else
            {
                Debug.Log("❌ Not all required words found yet.");
            }
        }

        void CheckForWinCondition2()
        {
            Debug.Log("🔍 Checking all sticky notes for required words...");

            // ✅ Process each sticky note's text
            foreach (Sticky sticky in ClueManager.Instance.GetAllStickyNotes())
            {
                if (sticky != null)
                {
                    QuestionManager.Instance.ProcessStickyNoteText(sticky._text.text);
                }
            }

            // ✅ Check if all required words are found
            if (QuestionManager.Instance.HasAllRequiredWords())
            {
                WinGame();
            }
            else
            {
                Debug.Log("❌ Not all required words found yet.");
            }
        }

        public string GetQuestion()
        {
            List<string> keys = new List<string>(_requiredWords.Keys);
            DisplayQuestion(keys[_currentQuestion]);
            return keys[_currentQuestion];
        }

        public List<string> GetRequiredWords(string question)
        {
            return _requiredWords[question]._words;
        }

        public void DisplayQuestion(string question)
        {
            _objective.text = question;
        }

        public void WinGame()
        {
            if (_currentQuestion == _requiredWords.Count - 1)
            {
                _currentQuestion = _requiredWords.Count - 1;

                Debug.Log("🎉 Player has found all required words! GAME WON! 🎉");
                // 🚀 Trigger game-winning logic here (e.g., UI pop-up, scene change)     
            }
            else
            {
                _currentQuestion++;
                QuestionManager.Instance.NewRequiredWords();
            }
        }
    }
}
