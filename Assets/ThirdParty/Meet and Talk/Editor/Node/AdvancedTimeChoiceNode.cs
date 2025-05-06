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
using static MeetAndTalk.DialogueGetData;

namespace MeetAndTalk.Nodes
{
    public class AdvancedTimeChoiceNode : BaseNode
    {
        private float time = 10;
        private bool showTimer = true;

        public List<AdvancedDialogueNodePort> dialogueNodePorts = new List<AdvancedDialogueNodePort>();
        public float ChoiceTime { get => time; set => time = value; }

        private FloatField time_Field;
        private Toggle ShowTimerToggle;

        public Button button;

        public AdvancedTimeChoiceNode() { }

        public AdvancedTimeChoiceNode(Vector2 _position, DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {
            editorWindow = _editorWindow;
            graphView = _graphView;

            title = "Advanced Time Choice";
            SetPosition(new Rect(_position, defualtNodeSize));
            nodeGuid = Guid.NewGuid().ToString();

            // Add Better Title
            GenerateBetterTitle("Advanced Time Choice", "Advanced Limited Time Responses", "Icon/NodeIcon/AdvancedTimeChoice");

            AddValidationContainer();

            AddInputPort("Input ", Port.Capacity.Multi);
            AddOutputPort("Time's Up or No Choice Available", "Defualt", Port.Capacity.Single);

            time_Field = new FloatField("Decision Time");
            time_Field.RegisterValueChangedCallback(value =>
            {
                time = value.newValue;
            });
            time_Field.SetValueWithoutNotify(time);
            time_Field.AddToClassList("TextTime");

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

            //if (outputContainer.Query("connector").ToList().Count() == 0) AddChoicePort(this);
        }

        public override void ReloadLanguage()
        {
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
            time_Field.SetValueWithoutNotify(time);
            ShowTimerToggle.SetValueWithoutNotify(showTimer);
        }
        /// <summary>
        /// Adds a new choice port to the specified base node and optionally initializes it with data from an existing dialogue node port.
        /// </summary>
        /// <param name="_basenote">The base node to which the port will be added.</param>
        /// <param name="_dialogueNodePort">An optional dialogue node port to copy data from.</param>
        /// <returns>The newly created port.</returns>
        public Port AddChoicePort(BaseNode _basenote, AdvancedDialogueNodePort _dialogueNodePort = null)
        {
            if (editorWindow.currentDialogueContainer.LimitChoiceOptionPerNode && editorWindow.currentDialogueContainer.MaxChoiceOptionPerNode > dialogueNodePorts.Count)
            {
                // Create a new output port instance with single connection capacity.
                Port port = GetPortInstance(Direction.Output, Port.Capacity.Single);

                // Generate a unique name for the output port based on the current number of ports.
                string outputPortName = "";
                int outputPortCount = _basenote.outputContainer.Query("connector").ToList().Count();
                if (outputPortCount < 9)
                {
                    outputPortName = $"Choice 0{outputPortCount + 1}";
                }
                else
                {
                    outputPortName = $"Choice {outputPortCount + 1}";
                }

                // Create a new instance of AdvancedDialogueNodePort with a unique GUID.
                AdvancedDialogueNodePort dialogueNodePort = new AdvancedDialogueNodePort();
                dialogueNodePort.PortGuid = Guid.NewGuid().ToString();
                dialogueNodePort.Condition = new ConditionClass(); //NOWE

                // Initialize text fields for each language supported by LocalizationEnum.
                foreach (LocalizationEnum language in (LocalizationEnum[])Enum.GetValues(typeof(LocalizationEnum)))
                {
                    dialogueNodePort.TextLanguage.Add(new LanguageGeneric<string>()
                    {
                        languageEnum = language,
                        LanguageGenericType = outputPortName
                    });
                }

                // If a source dialogue node port is provided, copy its data to the new port.
                if (_dialogueNodePort != null)
                {
                    dialogueNodePort.InputGuid = _dialogueNodePort.InputGuid;
                    dialogueNodePort.OutputGuid = _dialogueNodePort.OutputGuid;

                    // Ensure the source port has a GUID; if not, generate one.
                    if (_dialogueNodePort.PortGuid == "")
                    {
                        _dialogueNodePort.PortGuid = Guid.NewGuid().ToString();
                    }
                    dialogueNodePort.PortGuid = _dialogueNodePort.PortGuid;

                    // Copy language-specific text data from the source port.
                    foreach (LanguageGeneric<string> languageGeneric in _dialogueNodePort.TextLanguage)
                    {
                        dialogueNodePort.TextLanguage
                            .Find(language => language.languageEnum == languageGeneric.languageEnum)
                            .LanguageGenericType = languageGeneric.LanguageGenericType;
                    }

                    dialogueNodePort.ChoiceType = _dialogueNodePort.ChoiceType;
                    dialogueNodePort.Condition = _dialogueNodePort.Condition;
                }

                VisualElement box = new VisualElement();
                port.contentContainer.Add(box);

                // Create and configure a text field for the port's label.
                dialogueNodePort.TextField = new TextField();
                dialogueNodePort.TextField.RegisterValueChangedCallback(value =>
                {
                    // Update the language-specific text when the text field value changes.
                    dialogueNodePort.TextLanguage
                        .Find(language => language.languageEnum == editorWindow.LanguageEnum)
                        .LanguageGenericType = value.newValue;
                });
                dialogueNodePort.TextField.SetValueWithoutNotify(
                    dialogueNodePort.TextLanguage
                        .Find(language => language.languageEnum == editorWindow.LanguageEnum)
                        .LanguageGenericType
                );

                // Apply a CSS class to the text field for styling.
                dialogueNodePort.TextField.AddToClassList("ChoiceLabel");

                Box choice = new Box();
                choice.style.alignItems = Align.Center;
                choice.style.backgroundColor = new Color(0, 0, 0, 0);

                EnumField enumField = new EnumField() { value = dialogueNodePort.ChoiceType };
                enumField.Init(dialogueNodePort.ChoiceType);                            // Initialize the field with the endNodeType value
                enumField.RegisterValueChangedCallback((value) =>
                {
                    dialogueNodePort.ChoiceType = (AdvancedChoiceType)value.newValue;          // Update the endNodeType when the selection changes
                });
                enumField.SetValueWithoutNotify(dialogueNodePort.ChoiceType);           // Set the initial value without triggering a callback
                enumField.AddToClassList("TypeClass");
                enumField.style.marginRight = 0;


                choice.Add(enumField); 
                Image icon = new Image();
                icon.image = Resources.Load<Texture2D>("Icon/Editor/Translate"); // �cie�ka do pliku w Resources
                icon.AddToClassList("icon");
                icon.style.marginLeft = 0;
                icon.style.marginRight = 0;
                icon.style.width = 14;
                icon.style.height = 14;
                icon.style.minHeight = 14;
                icon.style.minWidth = 14;
                icon.style.marginRight = 4;
                choice.contentContainer.Add(icon);
                choice.Add(dialogueNodePort.TextField);



                choice.style.flexDirection = FlexDirection.Row;
                /////
                box.Add(choice);
                box.Add(dialogueNodePort.Condition.CreateVisualElement());


                // Add a delete button to the port, allowing it to be removed.
                Button deleteButton = new Button(() => DeleteButton(_basenote, port))
                {
                    text = "X"
                };
                port.contentContainer.Add(deleteButton);

#if UNITY_EDITOR
                // Associate the Unity Editor port instance with the dialogue node port for debugging.
                dialogueNodePort.MyPort = port;
#endif

                // Set the port name to an empty string for UI clarity.
                port.portName = "";
                port.AddToClassList("advanceChoicePort");

                // Add the newly created dialogue node port to the list of dialogue node ports.
                dialogueNodePorts.Add(dialogueNodePort);

                // Add the port to the base node's output container.
                _basenote.outputContainer.Add(port);

                // Refresh the base node's ports and expanded state to reflect the changes.
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
            AdvancedDialogueNodePort tmp = dialogueNodePorts.Find(port => port.MyPort == _port);
            dialogueNodePorts.Remove(tmp);
#endif

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


        public static AdvancedTimeChoiceNode CreateNewGraphNode(Vector2 _position, DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {
            AdvancedTimeChoiceNode tmp = new AdvancedTimeChoiceNode(_position, _editorWindow, _graphView); // Create the StartNode instance.
            tmp.name = "AdvancedTimeChoice";
            tmp.ReloadLanguage();

            return tmp;
        }
        public static AdvancedTimeChoiceNode GenerateNode(AdvancedTimeChoiceNodeData data, DialogueEditorWindow editor, DialogueGraphView graph)
        {
            AdvancedTimeChoiceNode tempNode = AdvancedTimeChoiceNode.CreateNewGraphNode(data.Position, editor, graph);
            tempNode.nodeGuid = data.NodeGuid;

            foreach (AdvancedDialogueNodePort nodePort in data.DialogueNodePorts)
            {
                tempNode.AddChoicePort(tempNode, nodePort);
            }
            tempNode.ChoiceTime = data.time;
            tempNode.showTimer = data.ShowTimer;

            tempNode.LoadValueInToField();

            return tempNode; // Return the generated node.

        }
        public AdvancedTimeChoiceNodeData SaveNodeData(List<Edge> edges)
        {
            AdvancedTimeChoiceNodeData dialogueNodeData = new AdvancedTimeChoiceNodeData
            {
                NodeGuid = nodeGuid,
                Position = GetPosition().position,
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
            if (!outputContainer.Query<Port>().First().connected) error.Add("\"Time's Up or No Choice Available\" does not lead to any node");
            if (dialogueNodePorts.Count < 1) error.Add("You need to add more Choice");
            else
            {
                for (int i = 0; i < dialogueNodePorts.Count; i++)
                {
                    // Choice Number
                    string number = "";
                    if (editorWindow.currentDialogueContainer.LimitChoiceOptionPerNode) number = $"[{i + 1}/{editorWindow.currentDialogueContainer.MaxChoiceOptionPerNode}]";
                    else number = (i + 1).ToString();

                    // validation
                    if (!dialogueNodePorts[i].MyPort.connected) error.Add($"Choice {number} does not lead to any node");
                    if (!dialogueNodePorts[i].Condition.isValidated()) error.Add($"Choice {number} Condition have validation error");
                }
            }
            if (ChoiceTime < 3) warning.Add("Short time for Make Decision");

            ErrorList = error;
            WarningList = warning;
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


