using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System;

namespace NoteSystem
{
    public class NoteInventory : MonoBehaviour
    {
        [SerializeField] private List<Note> notes = new List<Note>();

        public List<Note> Notes
        {
            get { return notes; }
        }

        [Header("Should persist?")]
        [SerializeField] private bool persistAcrossScenes = true;
        [SerializeField] private bool enableSaving = true; // Add this line

        private string SavePath => $"{Application.persistentDataPath}/inventory.json";

        public static NoteInventory instance;

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
                if (enableSaving) // Check this flag before loading
                {
                    LoadInventory();
                }
            }
        }

        public void AddNote(Note note)
        {
            // Check if the note isn't already in the inventory
            if (!notes.Contains(note))
            {
                notes.Add(note);
                NoteUIManager.instance.DisplayInventory();
                if (enableSaving) // Check this flag before saving
                {
                    SaveInventory();
                }
            }
        }

        public void RemoveNote(Note note)
        {
            if (notes.Contains(note))
            {
                notes.Remove(note);
                NoteUIManager.instance.DisplayInventory();
                if (enableSaving) // Check this flag before saving
                {
                    SaveInventory();
                }
            }
        }

        public bool HasNote(Note note)
        {
            return notes.Contains(note);
        }

        private void SaveInventory()
        {
            List<string> noteIDs = notes.Select(note => note.NoteID).ToList();
            string json = JsonUtility.ToJson(new Serialization<string>(noteIDs));
            File.WriteAllText(SavePath, json);
        }

        private void LoadInventory()
        {
            if (File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);
                Serialization<string> data = JsonUtility.FromJson<Serialization<string>>(json);

                // Load all notes from the Resources/Notes folder
                Note[] allNotes = Resources.LoadAll<Note>("Notes");

                // Debug log to check loaded notes
                Debug.Log($"Loaded {allNotes.Length} notes from Resources.");

                foreach (string id in data.items)
                {
                    Note note = allNotes.FirstOrDefault(n => n.NoteID == id);
                    if (note != null)
                    {
                        AddNote(note);  // Use the AddNote method to add it to the inventory
                    }
                    else
                    {
                        Debug.LogWarning($"Failed to load note with ID: {id}");
                    }
                }
            }
            else
            {
                Debug.LogWarning("No inventory file found to load.");
            }
        }

        public void DeleteInventoryFile()
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
                Debug.Log("Inventory file deleted successfully.");
            }
            else
            {
                Debug.Log("No inventory file to delete.");
            }
        }

        public string GetInventoryFilePath()
        {
            return SavePath;
        }

        public void CheckInventoryFile()
        {
            if (File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);
                Debug.Log($"Inventory file exists at {SavePath}:\n{json}");
            }
            else
            {
                Debug.Log("Inventory file does not exist.");
            }
        }

        [Serializable]
        private class Serialization<T>
        {
            public List<T> items;
            public Serialization(List<T> items)
            {
                this.items = items;
            }
        }
    }
}
