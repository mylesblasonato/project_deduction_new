using System;
using System.Collections.Generic;
using System.Linq;
using MeetAndTalk.Editor;
using MeetAndTalk.Event;
using MeetAndTalk.Localization;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MeetAndTalk.Nodes
{
    public class TimerChoiceNode : BaseNode
    {
        private List<LanguageGeneric<string>> texts = new List<LanguageGeneric<string>>();
        private List<LanguageGeneric<AudioClip>> audioClip = new List<LanguageGeneric<AudioClip>>();
        private float durationShow = 5;
        private bool showTimer = true;
        private float time = 10;

        public List<DialogueNodePort> dialogueNodePorts = new List<DialogueNodePort>();

        public List<LanguageGeneric<string>> Texts { get => texts; set => texts = value; }
        public List<LanguageGeneric<AudioClip>> AudioClip { get => audioClip; set => audioClip = value; }
        public float DurationShow { get => durationShow; set => durationShow = value; }
        public float ChoiceTime { get => time; set => time = value; }

        private TextField texts_Field;
        private ObjectField audioClips_Field;
        private FloatField duration_Field;
        private FloatField time_Field;
        private Toggle ShowTimerToggle;

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

        public Button button;

        public TimerChoiceNode() { }

        public TimerChoiceNode(Vector2 _position, DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {
            editorWindow = _editorWindow;
            graphView = _graphView;

            title = "Dialogue Time Choice";
            SetPosition(new Rect(_position, defualtNodeSize));
            nodeGuid = Guid.NewGuid().ToString();

            // Add Better Title
            GenerateBetterTitle("Dialogue Time Choice", "Dialogue with Time Limited Responses", "Icon/NodeIcon/TimeChoice");

            AddValidationContainer();

            AddInputPort("Input ", Port.Capacity.Multi);
            AddOutputPort("Time Out", "Defualt", Port.Capacity.Single);

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
                if (PortraitPosition == PortraitPosition.None) { EmotionField.SetEnabled(false); }
                else { EmotionField.SetEnabled(true); }
            });
            PortrainPositionField.SetValueWithoutNotify(PortraitPosition);
            PortrainPositionField.AddToClassList("Emotions");
            mainContainer.Add(PortrainPositionField);

            // EMOTION FIELD
            EmotionField = new PopupField<string>("Emotion", EmotionList,EmotionList.IndexOf(Emotion));
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


            time_Field = new FloatField("Decision Time");
            time_Field.RegisterValueChangedCallback(value =>
            {
                time = value.newValue;
            });
            time_Field.SetValueWithoutNotify(time);
            time_Field.AddToClassList("TextTime");
            mainContainer.Add(time_Field);

            ShowTimerToggle = new Toggle("Show TImer");
            ShowTimerToggle.RegisterValueChangedCallback(value =>
            {
                showTimer = value.newValue;
            });
            ShowTimerToggle.SetValueWithoutNotify(showTimer);
            ShowTimerToggle.AddToClassList("TimerToggle");

            Box Container = new Box();
            Container.Add(time_Field);
            Container.Add(ShowTimerToggle);
            Container.style.flexDirection = FlexDirection.Row;
            mainContainer.Add(Container);

            button = new Button()
            {
                text = "+ Add Choice Option"
            };
            button.clicked += () =>
            {
                AddChoicePort(this);
            };

            NewTitleBox.Add(button);


            if (PortraitPosition == PortraitPosition.None) { EmotionField.SetEnabled(false); }
            else { EmotionField.SetEnabled(true); }
            if (secoundPortraitPosition == PortraitPosition.None) { secoundEmotionField.SetEnabled(false); }
            else { secoundEmotionField.SetEnabled(true); }
            //if (outputContainer.Query("connector").ToList().Count() == 0) AddChoicePort(this);
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

            foreach (DialogueNodePort nodePort in dialogueNodePorts)
            {
                nodePort.TextField.RegisterValueChangedCallback(value =>
                              {
                                  nodePort.TextLanguage.Find(language => language.languageEnum == editorWindow.LanguageEnum).LanguageGenericType = value.newValue;
                              });
                nodePort.TextField.SetValueWithoutNotify(nodePort.TextLanguage.Find(language => language.languageEnum == editorWindow.LanguageEnum).LanguageGenericType);
            }
        }

        public override void LoadValueInToField()
        {
            texts_Field.SetValueWithoutNotify(texts.Find(language => language.languageEnum == editorWindow.LanguageEnum).LanguageGenericType);
            audioClips_Field.SetValueWithoutNotify(audioClip.Find(language => language.languageEnum == editorWindow.LanguageEnum).LanguageGenericType);
            duration_Field.SetValueWithoutNotify(durationShow);
            time_Field.SetValueWithoutNotify(time);
            ShowTimerToggle.SetValueWithoutNotify(showTimer);

            character_Field.SetValueWithoutNotify(character);
            PortrainPositionField.SetValueWithoutNotify(PortraitPosition);
            EmotionField.SetValueWithoutNotify(Emotion);
            EmotionField.MarkDirtyRepaint();

            secoundCharacter_Field.SetValueWithoutNotify(secoundCharacter);
            secoundPortrainPositionField.SetValueWithoutNotify(secoundPortraitPosition);
            secoundEmotionField.SetValueWithoutNotify(secoundEmotion);

            UpdatePortraits();
        }

        public Port AddChoicePort(BaseNode _basenote, DialogueNodePort _dialogueNodePort = null)
        {
            if (editorWindow.currentDialogueContainer.LimitChoiceOptionPerNode && editorWindow.currentDialogueContainer.MaxChoiceOptionPerNode > dialogueNodePorts.Count)
            {
                Port port = GetPortInstance(Direction.Output, Port.Capacity.Single);

            string outputPortName = "";
            int outputPortCount = _basenote.outputContainer.Query("connector").ToList().Count();
            if (outputPortCount < 10) { outputPortName = $"Choice 0{outputPortCount}"; }
            else { outputPortName = $"Choice {outputPortCount}"; }

            DialogueNodePort dialogueNodePort = new DialogueNodePort();
            dialogueNodePort.PortGuid = Guid.NewGuid().ToString(); //NOWE

            foreach (LocalizationEnum language in (LocalizationEnum[])Enum.GetValues(typeof(LocalizationEnum)))
            {
                dialogueNodePort.TextLanguage.Add(new LanguageGeneric<string>()
                {
                    languageEnum = language,
                    LanguageGenericType = $"[{language.ToString()}] "+outputPortName
                });
            }

            if (_dialogueNodePort != null)
            {
                dialogueNodePort.InputGuid = _dialogueNodePort.InputGuid;
                dialogueNodePort.OutputGuid = _dialogueNodePort.OutputGuid;

                if (_dialogueNodePort.PortGuid == "") { _dialogueNodePort.PortGuid = Guid.NewGuid().ToString(); } //NOWE
                dialogueNodePort.PortGuid = _dialogueNodePort.PortGuid; //NOWE

                foreach (LanguageGeneric<string> languageGeneric in _dialogueNodePort.TextLanguage)
                {
                    dialogueNodePort.TextLanguage.Find(language => language.languageEnum == languageGeneric.languageEnum).LanguageGenericType = languageGeneric.LanguageGenericType;
                }
            }

                Image icon = new Image();
                icon.image = Resources.Load<Texture2D>("Icon/Editor/Translate"); // �cie�ka do pliku w Resources
                icon.AddToClassList("icon");
                icon.style.marginLeft = 0;
                icon.style.marginRight = 0;


                dialogueNodePort.TextField = new TextField();
            dialogueNodePort.TextField.RegisterValueChangedCallback(value =>
            {
                dialogueNodePort.TextLanguage.Find(language => language.languageEnum == editorWindow.LanguageEnum).LanguageGenericType = value.newValue;
            });
            dialogueNodePort.TextField.SetValueWithoutNotify(dialogueNodePort.TextLanguage.Find(language => language.languageEnum == editorWindow.LanguageEnum).LanguageGenericType);

            if (outputPortCount != 0)
            {
                dialogueNodePort.TextField.AddToClassList("ChoiceLabel");
                port.contentContainer.Add(dialogueNodePort.TextField);
                Button deleteButton = new Button(() => DeleteButton(_basenote, port))
                {
                    text = "X"
                };
                    port.contentContainer.Add(icon);

                port.contentContainer.Add(deleteButton);

                port.portName = "";
            }
            else
            {
                dialogueNodePort.TextField.SetValueWithoutNotify("");
                dialogueNodePort.TextField.SetEnabled(false);
                dialogueNodePort.TextField.visible = false;
                port.contentContainer.Add(dialogueNodePort.TextField);
                port.portName = "Time Out";
                port.AddToClassList("FirstPort");
            }
#if UNITY_EDITOR
            dialogueNodePort.MyPort = port;
#endif

            dialogueNodePorts.Add(dialogueNodePort);

            _basenote.outputContainer.Add(port);

            _basenote.RefreshPorts();
            _basenote.RefreshExpandedState();

                // Sprawd� czy Node ma maksymaln� ilo�� Choice
                if (editorWindow.currentDialogueContainer.LimitChoiceOptionPerNode && editorWindow.currentDialogueContainer.MaxChoiceOptionPerNode == dialogueNodePorts.Count) button.SetEnabled(false);
                else button.SetEnabled(true);
                button.text = UpdateButtonText();

                return port;
            }
            // Disable Add Choice
            return null;
        }

        private void DeleteButton(BaseNode _node, Port _port)
        {
#if UNITY_EDITOR
            DialogueNodePort tmp = dialogueNodePorts.Find(port => port.MyPort == _port);
#endif
            dialogueNodePorts.Remove(tmp);

            IEnumerable<Edge> portEdge = graphView.edges.ToList().Where(edge => edge.output == _port);

            if (portEdge.Any())
            {
                Edge edge = portEdge.First();
                edge.input.Disconnect(edge);
                edge.output.Disconnect(edge);
                graphView.RemoveElement(edge);
            }

            _node.outputContainer.Remove(_port);

            _node.RefreshPorts();
            _node.RefreshExpandedState();

            // Sprawd� czy Node ma maksymaln� ilo�� Choice
            if (editorWindow.currentDialogueContainer.LimitChoiceOptionPerNode && editorWindow.currentDialogueContainer.MaxChoiceOptionPerNode == dialogueNodePorts.Count) button.SetEnabled(false);
            else button.SetEnabled(true);
            button.text = UpdateButtonText();
        }


        public static TimerChoiceNode CreateNewGraphNode(Vector2 _position, DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {
            TimerChoiceNode tmp = new TimerChoiceNode(_position, _editorWindow, _graphView); // Create the StartNode instance.
            tmp.name = "TimerChoice";
            tmp.ReloadLanguage();

            return tmp;
        }
        public static TimerChoiceNode GenerateNode(TimerChoiceNodeData data, DialogueEditorWindow editor, DialogueGraphView graph)
        {
            TimerChoiceNode tempNode = TimerChoiceNode.CreateNewGraphNode(data.Position, editor, graph);
            tempNode.nodeGuid = data.NodeGuid;
            tempNode.showTimer = data.ShowTimer;


            foreach (LanguageGeneric<string> languageGeneric in data.TextType)
            {
                tempNode.Texts.Find(language => language.languageEnum == languageGeneric.languageEnum).LanguageGenericType = languageGeneric.LanguageGenericType;
            }
            foreach (LanguageGeneric<AudioClip> languageGeneric in data.AudioClips)
            {
                tempNode.AudioClip.Find(language => language.languageEnum == languageGeneric.languageEnum).LanguageGenericType = languageGeneric.LanguageGenericType;
            }

            foreach (DialogueNodePort nodePort in data.DialogueNodePorts)
            {
                tempNode.AddChoicePort(tempNode, nodePort);
            }

            // Character
            tempNode.character = data.Character;
            tempNode.PortraitPosition = data.PortraitPosition;
            tempNode.Emotion = data.Emotion;

            Debug.Log("1. " + tempNode.Emotion + " " + data.Emotion);

            // Secound Character
            tempNode.secoundCharacter = data.SecoundCharacter;
            tempNode.secoundPortraitPosition = data.SecoundPortraitPosition;
            tempNode.secoundEmotion = data.SecoundEmotion;

            tempNode.DurationShow = data.Duration;
            tempNode.ChoiceTime = data.time;

            tempNode.LoadValueInToField();

            return tempNode; // Return the generated node.

        }
        public TimerChoiceNodeData SaveNodeData(List<Edge> edges)
        {
            TimerChoiceNodeData dialogueNodeData = new TimerChoiceNodeData
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
                Duration = DurationShow,
                DialogueNodePorts = dialogueNodePorts,
                time = ChoiceTime,
                ShowTimer = showTimer
            };
            foreach (DialogueNodePort nodePort in dialogueNodeData.DialogueNodePorts)
            {
                nodePort.OutputGuid = string.Empty;
                nodePort.InputGuid = string.Empty;
                foreach (Edge edge in edges)
                {
                    if (edge.output == nodePort.MyPort)
                    {
                        nodePort.OutputGuid = (edge.output.node as BaseNode).nodeGuid;
                        nodePort.InputGuid = (edge.input.node as BaseNode).nodeGuid;
                    }
                }
            }


            return dialogueNodeData;
        }

        public override void SetValidation()
        {
            List<string> error = new List<string>();
            List<string> warning = new List<string>();

            Port input = inputContainer.Query<Port>().First();
            if (!input.connected) warning.Add("Node cannot be called");


            if (dialogueNodePorts.Count < 2) error.Add("You need to add more Choice");
            else
            {
                if (!dialogueNodePorts[0].MyPort.connected) error.Add("Time Out does not lead to any node");
                for(int i = 1; i < dialogueNodePorts.Count; i++)
                {
                    if (!dialogueNodePorts[i].MyPort.connected) error.Add($"Choice ID:{i} does not lead to any node");
                }
            }
            if (ChoiceTime < 3) warning.Add("Short time for Make Decision");
            for(int i = 0; i < Texts.Count; i++) { if (Texts[i].LanguageGenericType == "") warning.Add($"No Text for {Texts[i].languageEnum} Language"); }

            ErrorList = error;
            WarningList = warning;

            // Update List
            if (character != null)
            {
                //AvatarPositionField.style.display = DisplayStyle.Flex;
                //AvatarTypeField.style.display = DisplayStyle.Flex;
            }
            else
            {
                //AvatarPositionField.style.display = DisplayStyle.None;
                //AvatarTypeField.style.display = DisplayStyle.None;
            }
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

        string UpdateButtonText()
        {
            if (!editorWindow.currentDialogueContainer.LimitChoiceOptionPerNode)
                return "+ Add New Choice";
            else if (editorWindow.currentDialogueContainer.MaxChoiceOptionPerNode > dialogueNodePorts.Count)
                return $"+ Add New Choice [{dialogueNodePorts.Count}/{editorWindow.currentDialogueContainer.MaxChoiceOptionPerNode}]";
            else
                return $"Choice Limit [{dialogueNodePorts.Count}/{editorWindow.currentDialogueContainer.MaxChoiceOptionPerNode}]";
        }

        public override void UpdateNodeUI()
        {
            button.text = UpdateButtonText();
        }
    }
}
