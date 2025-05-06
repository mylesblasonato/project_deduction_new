using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Prototype_Code.Managers
{
    public class QuestionManager : MonoBehaviour
    {
        private List<string> _requiredWords; // The words needed to win
        private HashSet<string> _playerWords = new HashSet<string>(); // Words the player has written

        public static QuestionManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            NewRequiredWords();
        }

        public void NewRequiredWords()
        {
            _requiredWords = GameManager.Instance.GetRequiredWords(GameManager.Instance.GetQuestion());
        }

        public void ProcessStickyNoteText(string text)
        {
            // ✅ Extract words from the sticky note text
            List<string> words = ExtractWords(text);

            // ✅ Add new words to the player's word set
            foreach (string word in words)
            {
                _playerWords.Add(word.ToLower()); // Convert to lowercase for case-insensitivity
            }
        }

        private List<string> ExtractWords(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>(); // Handle empty text safely

            // ✅ Remove all invisible characters (Zero-Width Spaces, Unicode non-breaking spaces)
            text = text.Replace("\u200B", "").Replace("\u200C", "").Replace("\u200D", "").Trim();

            // ✅ Split text into words, removing punctuation and empty entries
            char[] delimiters = new char[] { ' ', ',', '.', '!', '?', ':', ';', '\n', '\r' };
            return text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries)
                .Select(word => word.ToLower().Trim()) // Convert to lowercase
                .Distinct() // Remove duplicates
                .ToList();
        }

        public bool HasAllRequiredWords()
        {
            // ✅ Print all required words
            Debug.Log("📝 Required Words: " + string.Join(", ", _requiredWords.Select(w => w.ToLower())));

            // ✅ Print all player-entered words
            Debug.Log("✍ Player Words: " + string.Join(", ", _playerWords));

            // ✅ Convert both lists to lowercase to ensure case-insensitive matching
            bool allMatch = _requiredWords
                .Select(word => word.ToLower().Trim()) // Convert required words to lowercase & remove spaces
                .All(word => _playerWords.Contains(word.ToLower().Trim())); // Compare with stored words

            Debug.Log(allMatch ? "✅ ALL REQUIRED WORDS MATCH!" : "❌ MISSING WORDS!");

            return allMatch;
        }
    }
}
