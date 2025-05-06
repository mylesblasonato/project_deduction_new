using System;
using System.Collections.Generic;
using System.Linq;
using MeetAndTalk.Editor;
using MeetAndTalk.Event;
using MeetAndTalk.GlobalValue;
using MeetAndTalk.Localization;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static MeetAndTalk.DialogueGetData;

namespace MeetAndTalk.Nodes
{
    public class AdvancedIFNode : BaseNode
    {
        public List<ConditionInputPort> conditions = new List<ConditionInputPort>();
        public bool TrueWhenAllCOndition;

        public PopupField<string> IFType;
        List<string> Options = new List<string>() { "All Conditions Returns True", "At least One Conditions Return True" };

        //public string ValueName;
        //public GlobalValueIFOperations Operations;
        //public string OperationValue;

        public AdvancedIFNode()
        {

        }

        public AdvancedIFNode(Vector2 _position, DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {
            editorWindow = _editorWindow;
            graphView = _graphView;

            title = "Advanced Branch";
            SetPosition(new Rect(_position, defualtNodeSize));
            nodeGuid = Guid.NewGuid().ToString();

            // Add Better Title
            GenerateBetterTitle("Advanced Branch", "Branch with a multiple Condition", "Icon/NodeIcon/AdvancedBranch");

            AddInputPort("Input ", Port.Capacity.Multi);
            AddOutputPort("IF True", "True", Port.Capacity.Single);
            AddOutputPort("IF False", "False", Port.Capacity.Single);
            //AddOutputPort("Output", "Output", Port.Capacity.Multi, typeof(bool));

            int tmpIndex = 0;
            if (!TrueWhenAllCOndition) tmpIndex = 1;
            IFType = new PopupField<string>("Return True If", Options, tmpIndex);
            IFType.RegisterValueChangedCallback((value) =>
            {
                TrueWhenAllCOndition = IFType.index == 0 ? true : false;
            });
            mainContainer.Add(IFType);


            Label ConditionLabel = new Label("Condition List");
            ConditionLabel.AddToClassList("label_name");         // Add class for styling
            ConditionLabel.AddToClassList("Label");              // Add additional class for general styling
            mainContainer.Add(ConditionLabel);                   // Add the label to the node's main container


            //
            Button button = new Button()
            {
                text = "+ Add Condition"
            };
            button.clicked += () =>
            {
                AddChoicePort(this);
            };

            NewTitleBox.Add(button);
            AddValidationContainer();
        }

        public Port AddChoicePort(BaseNode _basenote, ConditionInputPort _dialogueNodePort = null)
        {
            Port port = GetPortInstance(Direction.Input, Port.Capacity.Single, typeof(bool));
            port.AddToClassList("conditionPort");

            // Create a new ConditionInputPort instead of DialogueNodePort
            ConditionInputPort conditionInputPort = new ConditionInputPort();
            conditionInputPort.PortGuid = Guid.NewGuid().ToString(); //NOWE
            conditionInputPort.Operation = new ConditionClass(); //NOWE

            if (_dialogueNodePort != null)
            {
                conditionInputPort.InputGuid = _dialogueNodePort.InputGuid;
                conditionInputPort.OutputGuid = _dialogueNodePort.OutputGuid;

                if (_dialogueNodePort.PortGuid == "") { _dialogueNodePort.PortGuid = Guid.NewGuid().ToString(); } //NOWE
                conditionInputPort.PortGuid = _dialogueNodePort.PortGuid; //NOWE
                conditionInputPort.Operation = _dialogueNodePort.Operation;
            }


            //port.contentContainer.Add(_dialogueNodePort.Operation.CreateVisualElement());

            port.contentContainer.Add(conditionInputPort.Operation.CreateVisualElement());

            // Create a delete button
            Button deleteButton = new Button(() => DeleteButton(_basenote, port))
            {
                text = "X"
            };
            port.contentContainer.Add(deleteButton);

#if UNITY_EDITOR
            // Set the port reference in the ConditionInputPort
            conditionInputPort.MyPort = port;
#endif
            //port.portName = "";

            // Add the created ConditionInputPort to a list (ensure you have a list for it)
            conditions.Add(conditionInputPort);  // Assuming you have a list `conditionInputPorts`
            //Debug.Log(conditions.Count);

            _basenote.mainContainer.Add(port);

            _basenote.RefreshPorts();
            _basenote.RefreshExpandedState();

            return port;
        }

        private void DeleteButton(BaseNode _node, Port _port)
        {
#if UNITY_EDITOR
            ConditionInputPort tmp = conditions.Find(port => port.MyPort == _port);
            conditions.Remove(tmp);
#endif

            IEnumerable<Edge> portEdge = graphView.edges.ToList().Where(edge => edge.output == _port);

            if (portEdge.Any())
            {
                Edge edge = portEdge.First();
                edge.input.Disconnect(edge);
                edge.output.Disconnect(edge);
                graphView.RemoveElement(edge);
            }

            _node.mainContainer.Remove(_port);

            _node.RefreshPorts();
            _node.RefreshExpandedState();
        }


        public override void LoadValueInToField()
        {
            GlobalValueManager manager = Resources.Load<GlobalValueManager>("GlobalValue");
            manager.LoadFile();

            IFType.SetValueWithoutNotify(Options[TrueWhenAllCOndition ? 0 : 1]);
            IFType.index = TrueWhenAllCOndition ? 0 : 1;
            //bool isValueNameInIntValues = manager.IntValues.Any(intValue => intValue.ValueName == ValueName);
            //bool isValueNameInFloatValues = manager.FloatValues.Any(floatValue => floatValue.ValueName == ValueName);

        }

        /// <summary>
        /// Factory method to create a new IFNode instance and initialize its properties.
        /// </summary>
        /// <param name="_position">The position of the new node in the graph editor.</param>
        /// <param name="_editorWindow">The editor window managing the graph.</param>
        /// <param name="_graphView">The graph view where the node will be added.</param>
        /// <returns>A new IFNode instance.</returns>
        public static AdvancedIFNode CreateNewGraphNode(Vector2 _position, DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {
            AdvancedIFNode tmp = new AdvancedIFNode(_position, _editorWindow, _graphView);
            tmp.name = "AdvancedIF";
            tmp.ReloadLanguage();

            return tmp; // Return the newly created node.
        }

        /// <summary>
        /// Generates a IFNode based on saved data, restoring its state and properties.
        /// </summary>
        /// <param name="data">The serialized data to use for generating the node.</param>
        /// <param name="editor">The editor window managing the graph.</param>
        /// <param name="graph">The graph view where the node will be added.</param>
        /// <returns>A restored IFNode instance.</returns>
        public static AdvancedIFNode GenerateNode(AdvancedIFNodeData data, DialogueEditorWindow editor, DialogueGraphView graph)
        {
            AdvancedIFNode newNode = CreateNewGraphNode(data.Position, editor, graph);
            newNode.nodeGuid = data.NodeGuid;
            newNode.TrueWhenAllCOndition = data.TrueWhenAllCondition;

            foreach (ConditionInputPort nodePort in data.Conditions)
            {
                newNode.AddChoicePort(newNode, nodePort);
            }

            newNode.LoadValueInToField();

            return newNode; // Return the generated node.
        }

        /// <summary>
        /// Serializes the node's state into a IFNodeData object for saving.
        /// </summary>
        /// <returns>A IFNodeData instance containing the node's serialized state.</returns>
        public AdvancedIFNodeData SaveNodeData(List<Edge> edges)
        {
            AdvancedIFNodeData nodeData = new AdvancedIFNodeData
            {
                NodeGuid = nodeGuid,
                Position = GetPosition().position,
                Conditions = conditions,
                TrueWhenAllCondition = TrueWhenAllCOndition
            };



            List<string> tmp = new List<string>();
            foreach (Edge edge in edges)
            {
                if ((edge.output.node as BaseNode).nodeGuid == nodeGuid)
                {
                    tmp.Add((edge.input.node as BaseNode).nodeGuid);
                }
            }
            if (tmp.Count > 0) { nodeData.TrueGUID = tmp[0]; }
            if (tmp.Count > 1) { nodeData.FalseGUID = tmp[1]; }

            //Debug.Log(conditions.Count);
            foreach (ConditionInputPort nodePort in nodeData.Conditions)
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




            return nodeData; // Return the serialized data.
        }

        public override void SetValidation()
        {
            List<string> error = new List<string>();
            List<string> warning = new List<string>();

            Port input = inputContainer.Query<Port>().First();
            if (!input.connected) warning.Add("Node cannot be called");

            if (!outputContainer.Query<Port>().AtIndex(0).connected) error.Add("\"IF True\" does not lead to any node");
            if (!outputContainer.Query<Port>().AtIndex(1).connected) error.Add("\"IF False\" does not lead to any node");

            for(int i = 0; i < conditions.Count; i++)
            {
                if (!conditions[i].Operation.isValidated()) error.Add($"Condition {i+1} have validation error");
            }

            ErrorList = error;
            WarningList = warning;
        }
    }
}
