#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using static LoadingScreenKit.Scripts.Utilities.UIButtonEvent;

namespace LoadingScreenKit.Scripts.Utilities
{
    [CustomEditor(typeof(UIButtonEvent))]
    public class UIButtonEventEditor : Editor
    {
        private UIButtonEvent uiButtonEvent;
        private UIDocument uiDocument;

        private SerializedProperty buttonsList;

        private void OnEnable()
        {
            uiButtonEvent = target as UIButtonEvent;

            // Ensure Buttons dictionary is not null
            if (uiButtonEvent.Buttons == null)
            {
                uiButtonEvent.Buttons = new List<ButtonRef>();
            }

            // Get the UIDocument component on the same GameObject
            uiDocument = uiButtonEvent.GetComponent<UIDocument>();

            // Find the serialized property for buttonsDict
            buttonsList = serializedObject.FindProperty("Buttons");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("Button Events", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            if (uiDocument == null || uiDocument.rootVisualElement == null)
            {
                EditorGUILayout.EndVertical();
                return;
            };

            var buttons = uiDocument.rootVisualElement.Query<Button>().ToList();

            // Ensure uiButtonEvent.Buttons is initialized if null
            if (uiButtonEvent.Buttons == null)
            {
                uiButtonEvent.Buttons = new List<ButtonRef>();
            }

            uiButtonEvent.Buttons.RemoveAll(buttonRef => buttonRef.buttonName == string.Empty);

            // Iterate over each button found in UIDocument
            foreach (var button in buttons)
            {
                if (button.name == string.Empty)
                {
                    EditorGUILayout.LabelField($"Button \"{button.text}\" has not been named.", EditorStyles.boldLabel); // Button name as title
                    continue;
                }

                // Check if a ButtonRef already exists for this button
                ButtonRef buttonRef = uiButtonEvent.Buttons.Find(b => b.buttonName == button.name);

                if (buttonRef == null)
                {
                    // Create a new ButtonRef for new buttons
                    buttonRef = new ButtonRef(button.name);
                    uiButtonEvent.Buttons.Add(buttonRef);
                }

                EditorGUILayout.LabelField(button.name, EditorStyles.boldLabel); // Button name as title

                // Find the index of buttonRef in the Buttons list
                int index = uiButtonEvent.Buttons.IndexOf(buttonRef);

                // Ensure the index is within bounds
                if (index >= 0 && index < buttonsList.arraySize)
                {
                    // Find the serialized property for the UnityEvent onClick
                    SerializedProperty onClickProp = buttonsList.GetArrayElementAtIndex(index).FindPropertyRelative("onClick");
                    EditorGUILayout.PropertyField(onClickProp, true);
                }
                else
                {
                    EditorGUILayout.HelpBox("Button reference index out of bounds.", MessageType.Error);
                }
            }

            EditorGUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties(); // Apply changes to serializedObject
        }

    }
}
#endif