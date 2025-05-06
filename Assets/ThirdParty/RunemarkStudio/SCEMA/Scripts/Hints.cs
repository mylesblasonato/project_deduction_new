namespace Runemark.SCEMA
{
    using System.Collections.Generic;
    using UnityEngine;
    using Runemark.Common;

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditorInternal;
#endif

    [HelpURL("https://runemarkstudio.com/scema/user-guide/")]
    [CreateAssetMenu(fileName = "New Hint List", menuName = "RunemarkStudio/Hints")]
    public class Hints : ScriptableObject
    {
        public List<string> hints = new List<string>();
        public string RandomHint()
        {
            int index = Random.Range(0, hints.Count-1);
            return hints[index];
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Hints))]
    public class HintsInspector : CustomInspectorBase
    {
        ReorderableList hints;
        protected override void OnInit()
        {
            var hintsProperty = FindProperty("hints");
            hints = new ReorderableList(serializedObject, hintsProperty, true, true, true, true);
            hints.drawHeaderCallback = (Rect rect) =>
            {
                GUI.Label(rect, "Hints");
            };

            hints.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => 
            {
                var element = hintsProperty.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(rect, element, GUIContent.none);
            };

            AddCustomField("hints", () => 
            {
                serializedObject.Update();
                hints.DoLayoutList();
                serializedObject.ApplyModifiedProperties();
             });
        }
    }
#endif

}
