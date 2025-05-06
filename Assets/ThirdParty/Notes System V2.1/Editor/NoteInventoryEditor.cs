using UnityEngine;
using UnityEditor;
using System.IO;

namespace NoteSystem
{
    [CustomEditor(typeof(NoteInventory))]
    public class NoteInventoryEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            NoteInventory noteInventory = (NoteInventory)target;

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Saving Settings", EditorStyles.toolbarTextField);

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Open Inventory File Location"))
            {
                OpenInventoryFileLocation(noteInventory.GetInventoryFilePath());
            }

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Check Inventory File"))
            {
                noteInventory.CheckInventoryFile();
            }

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Delete Inventory File"))
            {
                noteInventory.DeleteInventoryFile();
            }

            OpenEditorScript();
        }

        private void OpenInventoryFileLocation(string filePath)
        {
            string folderPath = Path.GetDirectoryName(filePath);

            #if UNITY_EDITOR_WIN
                System.Diagnostics.Process.Start("explorer.exe", folderPath);
            #elif UNITY_EDITOR_OSX
                EditorUtility.RevealInFinder(folderPath);
            #elif UNITY_EDITOR_LINUX
                System.Diagnostics.Process.Start("xdg-open", folderPath);
            #endif
        }

        void OpenEditorScript()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Editor Script", EditorStyles.toolbarTextField);
            EditorGUILayout.Space(5);
            if (GUILayout.Button("Open Editor Script"))
            {
                string scriptFilePath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
                AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<MonoScript>(scriptFilePath));
            }
        }
    }
}

