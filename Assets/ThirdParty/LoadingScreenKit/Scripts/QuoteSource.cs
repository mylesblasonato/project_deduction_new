using UnityEngine;
using System;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LoadingScreenKit.Scripts
{
    [Serializable]
    public class QuoteSource : ScriptableObject
    {
        public string[] quotes;
    }

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(QuoteSource), true)]
    public class MyScriptableObjectDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Draw the default property field
            Rect objectFieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(objectFieldRect, property, label);

            // Calculate the position for the button
            Rect buttonRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight);

            // Draw the button
            if (GUI.Button(buttonRect, "Generate QuoteSource"))
            {
                CreateScriptableObject(property);
            }

            EditorGUI.EndProperty();
        }

        private void CreateScriptableObject(SerializedProperty property)
        {
            // Determine the path of the currently selected folder in the Project View
            var folderPath = "Assets";
            if (Selection.activeObject != null)
            {
                folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                if (!string.IsNullOrEmpty(folderPath) && !AssetDatabase.IsValidFolder(folderPath))
                {
                    folderPath = System.IO.Path.GetDirectoryName(folderPath);
                }
            }

            // Create an instance of the ScriptableObject
            var newAsset = ScriptableObject.CreateInstance<QuoteSource>();

            // Generate a unique path for the asset
            var assetPath = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/New{typeof(QuoteSource).Name}.asset");

            // Create the asset in the specified folder and allow renaming
            ProjectWindowUtil.CreateAsset(newAsset, assetPath);

            // Assign the created asset to the property
            property.objectReferenceValue = newAsset;
            property.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Increase height to accommodate the button
            return EditorGUIUtility.singleLineHeight * 2 + 4;
        }
    }


#endif
}