using System;
using System.Collections.Generic;
using System.Diagnostics;
using MeetAndTalk.Editor;
using MeetAndTalk.Localization;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MeetAndTalk.Nodes
{
    [System.Serializable]
    public class DialogueNode : BaseNode
    {
        // Node Value
        private List<LanguageGeneric<AudioClip>> audioClip = new List<LanguageGeneric<AudioClip>>();
        private List<LanguageGeneric<string>> texts = new List<LanguageGeneric<string>>();
        private float durationShow = 10;
        //public List<DialogueNodePort> dialogueNodePorts = new List<DialogueNodePort>();

        public List<LanguageGeneric<AudioClip>> AudioClip { get => audioClip; set => audioClip = value; }
        public List<LanguageGeneric<string>> Texts { get => texts; set => texts = value; }
        public float DurationShow { get => durationShow; set => durationShow = value; }

        // Node Field
        private ObjectField audioClips_Field;
        private TextField texts_Field;
        private FloatField duration_Field;

        // New Emotion System
        public DialogueCharacterSO character = ScriptableObject.CreateInstance<DialogueCharacterSO>();
        private ObjectField character_Field;
        public PortraitPosition PortraitPosition;
        public EnumField PortrainPositionField;
        public string Emotion;
        public PopupField<string> EmotionField;
        public List<string> EmotionList = new List<string>();

        public DialogueCharacterSO secoundCharacter = ScriptableObject.CreateInstance<DialogueCharacterSO>();
        private ObjectField secoundCharacter_Field;
        public PortraitPosition secoundPortraitPosition;
        public EnumField secoundPortrainPositionField;
        public string secoundEmotion;
        public PopupField<string> secoundEmotionField;
        public List<string> secoundEmotionList = new List<string>();

        public DialogueNode() { }

        public DialogueNode(Vector2 _position, DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {
            // Assign references to the editor window and graph view for later use.
            editorWindow = _editorWindow;
            graphView = _graphView;

            title = "Dialogue";
            SetPosition(new Rect(_position, defualtNodeSize));
            nodeGuid = Guid.NewGuid().ToString();

            // Add Better Title
            GenerateBetterTitle("Dialogue", "Basic Node to display Dialogue", "Icon/NodeIcon/Dialogue");

            AddInputPort("Input", Port.Capacity.Multi);
            AddOutputPort("Output", Port.Capacity.Single);

            // Load Language Values
            foreach (LocalizationEnum language in (LocalizationEnum[])Enum.GetValues(typeof(LocalizationEnum)))
            {
                texts.Add(new LanguageGeneric<string>
                {
                    languageEnum = language,
                    LanguageGenericType = ""
                });
                AudioClip.Add(new LanguageGeneric<AudioClip>
                {
                    languageEnum = language,
                    LanguageGenericType = null
                });
            }

            // CATEGORY LABEL: CHARACTER SETTINGS
            Label characterLabel = new Label("Character Settings");
            characterLabel.AddToClassList("label_texts");
            characterLabel.AddToClassList("Label");
            mainContainer.Add(characterLabel);

            // CHARACTER FIELD
            character_Field = new ObjectField("Character")
            {
                objectType = typeof(DialogueCharacterSO),
                allowSceneObjects = false,
            };
            character_Field.RegisterValueChangedCallback(value =>
            {
                character = value.newValue as DialogueCharacterSO;
                UpdatePortraits();
            });
            character_Field.SetValueWithoutNotify(character);
            mainContainer.Add(character_Field);

            // PORTRAIT ENUM FIELD
            PortrainPositionField = new EnumField("Portrait Pos.") { value = PortraitPosition };
            PortrainPositionField.Init(PortraitPosition);                           
            PortrainPositionField.RegisterValueChangedCallback(value =>
            {
                PortraitPosition = (PortraitPosition)value.newValue;
                if(PortraitPosition== PortraitPosition.None) { EmotionField.SetEnabled(false); }
                else { EmotionField.SetEnabled(true); }
            });
            PortrainPositionField.SetValueWithoutNotify(PortraitPosition);
            PortrainPositionField.AddToClassList("Emotions");
            mainContainer.Add(PortrainPositionField);

            // EMOTION FIELD
            EmotionField = new PopupField<string>("Emotion", EmotionList, 0);
            EmotionField.RegisterValueChangedCallback(value =>
            {
                Emotion = value.newValue;
            });
            EmotionField.AddToClassList("Emotions");
            mainContainer.Add(EmotionField);



            // CHARACTER FIELD
            secoundCharacter_Field = new ObjectField("Alt. Character")
            {
                objectType = typeof(DialogueCharacterSO),
                allowSceneObjects = false,
            };
            secoundCharacter_Field.RegisterValueChangedCallback(value =>
            {
                secoundCharacter = value.newValue as DialogueCharacterSO;
                UpdatePortraits();
            });
            secoundCharacter_Field.SetValueWithoutNotify(secoundCharacter);
            mainContainer.Add(secoundCharacter_Field);

            // PORTRAIT ENUM FIELD
            secoundPortrainPositionField = new EnumField("Portrait Pos.") { value = secoundPortraitPosition };
            secoundPortrainPositionField.Init(PortraitPosition);
            secoundPortrainPositionField.RegisterValueChangedCallback(value =>
            {
                secoundPortraitPosition = (PortraitPosition)value.newValue;
                if (secoundPortraitPosition == PortraitPosition.None) { secoundEmotionField.SetEnabled(false); }
                else { secoundEmotionField.SetEnabled(true); }
            });
            secoundPortrainPositionField.SetValueWithoutNotify(secoundPortraitPosition);
            secoundPortrainPositionField.AddToClassList("Emotions");
            mainContainer.Add(secoundPortrainPositionField);

            // EMOTION FIELD
            secoundEmotionField = new PopupField<string>("Emotion", secoundEmotionList, 0);
            secoundEmotionField.RegisterValueChangedCallback(value =>
            {
                secoundEmotion = value.newValue;
            });
            secoundEmotionField.AddToClassList("Emotions");
            mainContainer.Add(secoundEmotionField);



            /* TEXT BOX */
            VisualElement labelContainer = new VisualElement();
            labelContainer.AddToClassList("label-container");
            Image icon = new Image();
            icon.image = Resources.Load<Texture2D>("Icon/Editor/Translate"); // �cie�ka do pliku w Resources
            icon.AddToClassList("icon");
            Label label_texts = new Label("Dialogue Content");
            label_texts.AddToClassList("label_texts");
            labelContainer.Add(icon);
            labelContainer.Add(label_texts);
            mainContainer.Add(labelContainer);

            texts_Field = new TextField("");
            texts_Field.RegisterValueChangedCallback(value =>
            {
                texts.Find(text => text.languageEnum == editorWindow.LanguageEnum).LanguageGenericType = value.newValue;
            });
            texts_Field.SetValueWithoutNotify(texts.Find(text => text.languageEnum == editorWindow.LanguageEnum).LanguageGenericType);
            texts_Field.multiline = true;

            texts_Field.AddToClassList("TextBox");
            mainContainer.Add(texts_Field);
            /* AUDIO CLIPS */
            audioClips_Field = new ObjectField()
            {
                objectType = typeof(AudioClip),
                allowSceneObjects = false,
                value = audioClip.Find(audioClips => audioClips.languageEnum == editorWindow.LanguageEnum).LanguageGenericType,
            };
            audioClips_Field.RegisterValueChangedCallback(value =>
            {
                audioClip.Find(audioClips => audioClips.languageEnum == editorWindow.LanguageEnum).LanguageGenericType = value.newValue as AudioClip;
            });
            audioClips_Field.SetValueWithoutNotify(audioClip.Find(audioClips => audioClips.languageEnum == editorWindow.LanguageEnum).LanguageGenericType);
            mainContainer.Add(audioClips_Field);


            /* Character Label */
            Label settingsLabel = new Label("Display Settings");
            settingsLabel.AddToClassList("label_texts");
            settingsLabel.AddToClassList("Label");
            mainContainer.Add(settingsLabel);
            /* Duration NAME */
            duration_Field = new FloatField("Display Time");
            duration_Field.RegisterValueChangedCallback(value =>
            {
                durationShow = value.newValue;
            });
            duration_Field.SetValueWithoutNotify(durationShow);

            duration_Field.AddToClassList("TextDuration");
            mainContainer.Add(duration_Field);

            if (PortraitPosition == PortraitPosition.None) { EmotionField.SetEnabled(false); }
            else { EmotionField.SetEnabled(true); }
            if (secoundPortraitPosition == PortraitPosition.None) { secoundEmotionField.SetEnabled(false); }
            else { secoundEmotionField.SetEnabled(true); }

            // Refresh the node's state and UI.
            RefreshExpandedState();         // Update the expanded/collapsed state of the node.
            RefreshPorts();                 // Ensure all ports are correctly configured and displayed.
            AddValidationContainer();       // Add containers for error and warning messages to the node.
        }

        public override void ReloadLanguage()
        {
            texts_Field.RegisterValueChangedCallback(value =>
            {
                texts.Find(text => text.languageEnum == editorWindow.LanguageEnum).LanguageGenericType = value.newValue;
            });
            texts_Field.SetValueWithoutNotify(texts.Find(text => text.languageEnum == editorWindow.LanguageEnum).LanguageGenericType);

            audioClips_Field.RegisterValueChangedCallback(value =>
            {
                audioClip.Find(audioClips => audioClips.languageEnum == editorWindow.LanguageEnum).LanguageGenericType = value.newValue as AudioClip;
            });
            audioClips_Field.SetValueWithoutNotify(audioClip.Find(audioClips => audioClips.languageEnum == editorWindow.LanguageEnum).LanguageGenericType);
            UpdatePortraits();
        }

        public override void LoadValueInToField()
        {
            texts_Field.SetValueWithoutNotify(texts.Find(language => language.languageEnum == editorWindow.LanguageEnum).LanguageGenericType);
            audioClips_Field.SetValueWithoutNotify(audioClip.Find(language => language.languageEnum == editorWindow.LanguageEnum).LanguageGenericType);

            UpdatePortraits();
            character_Field.SetValueWithoutNotify(character);
            PortrainPositionField.SetValueWithoutNotify(PortraitPosition);
            EmotionField.SetValueWithoutNotify(Emotion);

            secoundCharacter_Field.SetValueWithoutNotify(secoundCharacter);
            secoundPortrainPositionField.SetValueWithoutNotify(secoundPortraitPosition);
            secoundEmotionField.SetValueWithoutNotify(secoundEmotion);

            duration_Field.SetValueWithoutNotify(durationShow);
        }

        public static DialogueNode CreateNewGraphNode(Vector2 _position, DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {
            DialogueNode tmp = new DialogueNode(_position, _editorWindow, _graphView);
            tmp.name = "Dialog";
            tmp.ReloadLanguage();

            return tmp; // Return the newly created node.
        }

        public static DialogueNode GenerateNode(DialogueNodeData data, DialogueEditorWindow editor, DialogueGraphView graph)
        {
            // Generate a new DialogueNode using its factory method.
            DialogueNode newNode = DialogueNode.CreateNewGraphNode(data.Position, editor, graph);

            // Restore the node's unique identifier and values.
            newNode.nodeGuid = data.NodeGuid;

            // Character
            newNode.character = data.Character;
            newNode.PortraitPosition = data.PortraitPosition;
            newNode.Emotion = data.Emotion;

            // Secound Character
            newNode.secoundCharacter = data.SecoundCharacter;
            newNode.secoundPortraitPosition = data.SecoundPortraitPosition;
            newNode.secoundEmotion = data.SecoundEmotion;

            newNode.DurationShow = data.Duration;
            foreach (LanguageGeneric<string> languageGeneric in data.TextType)
            {
                newNode.Texts.Find(language => language.languageEnum == languageGeneric.languageEnum).LanguageGenericType = languageGeneric.LanguageGenericType;
            }
            foreach (LanguageGeneric<AudioClip> languageGeneric in data.AudioClips)
            {
                newNode.AudioClip.Find(language => language.languageEnum == languageGeneric.languageEnum).LanguageGenericType = languageGeneric.LanguageGenericType;
            }

            // Load the restored values into the corresponding fields.
            newNode.LoadValueInToField();

            return newNode; // Return the generated node.
        }

        public DialogueNodeData SaveNodeData()
        {
            DialogueNodeData nodeData = new DialogueNodeData
            {
                // Base Settings
                NodeGuid = nodeGuid,
                Position = GetPosition().position,

                // Character
                Character = character,
                PortraitPosition = PortraitPosition,
                Emotion = Emotion,

                // Secound Character
                SecoundCharacter = secoundCharacter,
                SecoundPortraitPosition = secoundPortraitPosition,
                SecoundEmotion = secoundEmotion,

                // Dialogue Content
                TextType = Texts,
                AudioClips = AudioClip,

                // Display Settings
                Duration = DurationShow
            };

            return nodeData; // Return the serialized data.
        }

        public override void SetValidation()
        {
            List<string> error = new List<string>();
            List<string> warning = new List<string>();

            Port input = inputContainer.Query<Port>().First();
            if (!input.connected) warning.Add("Node cannot be called");

            Port output = outputContainer.Query<Port>().First();
            if (!output.connected) error.Add("Output does not lead to any node");

            if (durationShow < 1 && durationShow != 0) warning.Add("Short time for Make Decision");
            for (int i = 0; i < Texts.Count; i++) { if (Texts[i].LanguageGenericType == "") warning.Add($"No Text for {Texts[i].languageEnum} Language"); }

            ErrorList = error;
            WarningList = warning;
        }


        public void UpdatePortraits()
        {
            EmotionList.Clear();
            if (character != null)
            {
                if (character.Avatars.Count == 0)
                {
                    EmotionField.SetEnabled(false);
                    PortrainPositionField.SetEnabled(false);
                    PortrainPositionField.SetValueWithoutNotify(PortraitPosition.None);
                }
                else
                {
                    EmotionField.SetEnabled(true);
                    PortrainPositionField.SetEnabled(true);
                    foreach (EmotionClass Emotion in character.Avatars)
                    {
                        EmotionList.Add(Emotion.EmotionName);
                    }
                    if (Emotion == null)
                    {
                        EmotionField.SetValueWithoutNotify(character.Avatars[0].EmotionName);
                        Emotion = character.Avatars[0].EmotionName;
                    }
                }
            }
            else
            {
                EmotionField.SetEnabled(false);
                PortrainPositionField.SetEnabled(false);
                PortrainPositionField.SetValueWithoutNotify(PortraitPosition.None);
            }

            if (PortraitPosition == PortraitPosition.None) { EmotionField.SetEnabled(false); }
            else { EmotionField.SetEnabled(true); }


            // Secound CHaracter
            secoundEmotionList.Clear();
            if (secoundCharacter != null)
            {
                if (secoundCharacter.Avatars.Count == 0)
                {
                    secoundEmotionField.SetEnabled(false);
                    secoundPortrainPositionField.SetEnabled(false);
                    secoundPortrainPositionField.SetValueWithoutNotify(PortraitPosition.None);
                }
                else
                {
                    secoundEmotionField.SetEnabled(true);
                    secoundPortrainPositionField.SetEnabled(true);
                    foreach (EmotionClass Emotion in secoundCharacter.Avatars)
                    {
                        secoundEmotionList.Add(Emotion.EmotionName);
                    }
                    if (secoundEmotion == null)
                    {
                        secoundEmotionField.SetValueWithoutNotify(secoundCharacter.Avatars[0].EmotionName);
                        secoundEmotion = secoundCharacter.Avatars[0].EmotionName;
                    }
                }
            }
            else
            {
                secoundEmotionField.SetEnabled(false);
                secoundPortrainPositionField.SetEnabled(false);
                secoundPortrainPositionField.SetValueWithoutNotify(PortraitPosition.None);
            }

            if (secoundPortraitPosition == PortraitPosition.None) { secoundEmotionField.SetEnabled(false); }
            else { secoundEmotionField.SetEnabled(true); }
        }
    }
}