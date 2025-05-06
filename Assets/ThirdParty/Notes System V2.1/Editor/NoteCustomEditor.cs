using UnityEditor;
using UnityEngine;

namespace NoteSystem
{
    [CustomEditor(typeof(Note))]
    public class NoteCustomEditor : Editor
    {
        #region Serialized Properties
        SerializedProperty noteID;
        SerializedProperty noteName;
        SerializedProperty noteIcon;
        SerializedProperty addToInventory;

        SerializedProperty pageScale;
        SerializedProperty pageImages;

        SerializedProperty showTextOnMainPage;
        SerializedProperty showNavigationButtons;
        SerializedProperty pageText;

        SerializedProperty mainTextAreaScale;
        SerializedProperty mainTextSize;
        SerializedProperty mainFontAsset;
        SerializedProperty mainFontColor;

        SerializedProperty enableOverlayText;
        SerializedProperty showOverlayOnOpen;

        SerializedProperty overlayBGColor;
        SerializedProperty overlayTextAreaScale;
        SerializedProperty overlayTextBGScale;

        SerializedProperty overlayTextSize;
        SerializedProperty overlayFontAsset;
        SerializedProperty overlayFontColor;

        SerializedProperty allowAudioPlayback;
        SerializedProperty showPlaybackButtons;
        SerializedProperty playOnOpen;
        SerializedProperty noteReadAudio;
        SerializedProperty notePageAudio;

        SerializedProperty isNoteTrigger;
        #endregion

        bool textCustomisationGroup, textAreaSettingsGroup, flipBGColorGroup, flipTextCustomisationGroup;

        void OnEnable()
        {
            #region SerializedObject Finds
            noteID = serializedObject.FindProperty(nameof(noteID));
            noteName = serializedObject.FindProperty(nameof(noteName));
            noteIcon = serializedObject.FindProperty(nameof(noteIcon));
            addToInventory = serializedObject.FindProperty(nameof(addToInventory));

            pageScale = serializedObject.FindProperty(nameof(pageScale));
            pageImages = serializedObject.FindProperty(nameof(pageImages));

            showTextOnMainPage = serializedObject.FindProperty(nameof(showTextOnMainPage));
            showNavigationButtons = serializedObject.FindProperty(nameof(showNavigationButtons));
            pageText = serializedObject.FindProperty(nameof(pageText));

            mainTextAreaScale = serializedObject.FindProperty(nameof(mainTextAreaScale));
            mainTextSize = serializedObject.FindProperty(nameof(mainTextSize));
            mainFontAsset = serializedObject.FindProperty(nameof(mainFontAsset));
            mainFontColor = serializedObject.FindProperty(nameof(mainFontColor));

            enableOverlayText = serializedObject.FindProperty(nameof(enableOverlayText));
            showOverlayOnOpen = serializedObject.FindProperty(nameof(showOverlayOnOpen));

            overlayBGColor = serializedObject.FindProperty(nameof(overlayBGColor));
            overlayTextAreaScale = serializedObject.FindProperty(nameof(overlayTextAreaScale));
            overlayTextBGScale = serializedObject.FindProperty(nameof(overlayTextBGScale));

            overlayTextSize = serializedObject.FindProperty(nameof(overlayTextSize));
            overlayFontAsset = serializedObject.FindProperty(nameof(overlayFontAsset));
            overlayFontColor = serializedObject.FindProperty(nameof(overlayFontColor));

            allowAudioPlayback = serializedObject.FindProperty(nameof(allowAudioPlayback));
            showPlaybackButtons = serializedObject.FindProperty(nameof(showPlaybackButtons));
            playOnOpen = serializedObject.FindProperty(nameof(playOnOpen));

            noteReadAudio = serializedObject.FindProperty(nameof(noteReadAudio));
            notePageAudio = serializedObject.FindProperty(nameof(notePageAudio));

            isNoteTrigger = serializedObject.FindProperty(nameof(isNoteTrigger));
            #endregion
        }

        public override void OnInspectorGUI()
        {
            #region Visual Script Reference
            GUI.enabled = false;
            MonoScript ms = MonoScript.FromScriptableObject((ScriptableObject)target);
            EditorGUILayout.ObjectField("Script:", ms, typeof(MonoScript), false);
            GUI.enabled = true;
            #endregion

            EditorGUILayout.Space(5);
            Note noteSO = (Note)target;

            #region isReadable Section
            EditorGUILayout.LabelField("Basic Note Settings", EditorStyles.toolbarTextField);

            EditorGUILayout.Space(2);
            EditorGUILayout.PropertyField(noteID);
            EditorGUILayout.Space(2);
            EditorGUILayout.PropertyField(noteName);
            EditorGUILayout.Space(2);
            EditorGUILayout.PropertyField(noteIcon);
            EditorGUILayout.Space(2);
            EditorGUILayout.PropertyField(addToInventory);
            #endregion

            #region Basic Page Settings
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Main Note Visuals", EditorStyles.toolbarTextField);
            EditorGUILayout.Space(2);
            EditorGUILayout.PropertyField(pageScale);
            EditorGUILayout.Space(2);
            EditorGUILayout.PropertyField(pageImages);
            EditorGUILayout.Space(2);
            EditorGUILayout.PropertyField(pageText);
            #endregion

            #region Multiple Page Settings
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Multiple Page Settings (If you have multiple pages)", EditorStyles.toolbarTextField);
 
            EditorGUILayout.Space(2);
            EditorGUILayout.PropertyField(showNavigationButtons);
            #endregion

            #region Main Note Text Customisation
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Main Note Text Customisation", EditorStyles.toolbarTextField);
            EditorGUILayout.Space(2);
            EditorGUILayout.PropertyField(showTextOnMainPage);
            EditorGUILayout.Space(2);

            if (showTextOnMainPage.boolValue) // Checking the boolean value of the SerializedProperty
            {
                EditorGUILayout.PropertyField(mainTextAreaScale);
                EditorGUILayout.Space(2);

                textCustomisationGroup = EditorGUILayout.BeginFoldoutHeaderGroup(textCustomisationGroup, "Main Note Font Settings");
                if (textCustomisationGroup)
                {
                    EditorGUILayout.PropertyField(mainTextSize);
                    EditorGUILayout.PropertyField(mainFontAsset);
                    //EditorGUILayout.PropertyField(mainFontStyle);
                    EditorGUILayout.PropertyField(mainFontColor);
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
            EditorGUILayout.Space(5);
            #endregion

            #region Overlay Text Customisation Settings
            EditorGUILayout.LabelField("Overlay Text Customisation Settings", EditorStyles.toolbarTextField);
            EditorGUILayout.Space(2);

            EditorGUILayout.PropertyField(enableOverlayText);
            EditorGUILayout.Space(5);

            if (noteSO.EnableOverlayText)
            {              
                EditorGUILayout.PropertyField(showOverlayOnOpen);
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(overlayTextAreaScale);
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(overlayTextBGScale);
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(overlayBGColor);

                EditorGUILayout.Space(2);
                flipTextCustomisationGroup = EditorGUILayout.BeginFoldoutHeaderGroup(flipTextCustomisationGroup, "Overlay Font Settings");
                {
                    if (flipTextCustomisationGroup)
                    {
                        EditorGUILayout.PropertyField(overlayTextSize);
                        EditorGUILayout.PropertyField(overlayFontAsset);
                        //EditorGUILayout.PropertyField(overlayFontStyle);
                        EditorGUILayout.PropertyField(overlayFontColor);
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
                EditorGUILayout.Space(5);
            }        
            #endregion

            #region Basic Audio Settings
            EditorGUILayout.LabelField("Basic Audio Settings", EditorStyles.toolbarTextField);
            EditorGUILayout.Space(2);

            EditorGUILayout.PropertyField(allowAudioPlayback);
            if (noteSO.AllowAudioPlayback)
            {
                EditorGUILayout.Space(2);
                EditorGUILayout.LabelField("---------------", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(showPlaybackButtons);
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(playOnOpen);
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(noteReadAudio);
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Note Default Audio", EditorStyles.toolbarTextField);

            EditorGUILayout.Space(2);
            EditorGUILayout.PropertyField(notePageAudio);
            #endregion

            #region Trigger Settings
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Basic Trigger Settings", EditorStyles.toolbarTextField);

            EditorGUILayout.Space(2);
            EditorGUILayout.PropertyField(isNoteTrigger);
            #endregion

            OpenEditorScript();

            serializedObject.ApplyModifiedProperties();
        }

        void OpenEditorScript()
        {
            EditorGUILayout.Space(5);
            if (GUILayout.Button("Open Editor Script"))
            {
                string scriptFilePath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
                AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<MonoScript>(scriptFilePath));
            }
        }
    }
}
