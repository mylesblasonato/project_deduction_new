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

namespace MeetAndTalk.Nodes
{

    public class IFNode : BaseNode
    {
        public List<DialogueNodePort> dialogueNodePorts = new List<DialogueNodePort>();

        public string ValueName;
        public GlobalValueIFOperations Operations;
        public string OperationValue;

        private DropdownField ValueName_Field;
        private EnumField AvatarPositionField;
        private TextField Value_Field;

        public IFNode()
        {

        }

        public IFNode(Vector2 _position, DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {
            editorWindow = _editorWindow;
            graphView = _graphView;

            title = "Simple Branch";
            SetPosition(new Rect(_position, defualtNodeSize));
            nodeGuid = Guid.NewGuid().ToString();

            // Add Better Title
            GenerateBetterTitle("Simple Branch", "Branch with a single Condition", "Icon/NodeIcon/Branch");

            AddInputPort("Input ", Port.Capacity.Multi);
            AddOutputPort("IF True", "True", Port.Capacity.Single);
            AddOutputPort("IF False", "False", Port.Capacity.Single);
            //AddOutputPort("Output", "Output", Port.Capacity.Multi, typeof(bool));
            AddValidationContainer();

            // Value Name Field
            GlobalValueManager manager = Resources.Load<GlobalValueManager>("GlobalValue");
            manager.LoadFile();

            List<string> valueNames = new List<string>();
            valueNames.AddRange(manager.IntValues.Select(intValue => intValue.ValueName));
            valueNames.AddRange(manager.FloatValues.Select(floatValue => floatValue.ValueName));
            valueNames.AddRange(manager.BoolValues.Select(boolValue => boolValue.ValueName));

            ValueName_Field = new DropdownField(label: "Global Value", choices: (valueNames.Distinct().ToList()), defaultIndex: 0);
            ValueName_Field.RegisterValueChangedCallback(value =>
            {
                ValueName = value.newValue;   
                
                // Sprawd�, czy ValueName znajduje si� w manager.IntValues lub manager.FloatValues
                bool isValueNameInIntValues = manager.IntValues.Any(intValue => intValue.ValueName == ValueName);
                bool isValueNameInFloatValues = manager.FloatValues.Any(floatValue => floatValue.ValueName == ValueName);

                // Ustaw widoczno�� AvatarPositionField w zale�no�ci od warunk�w
                AvatarPositionField.style.display = isValueNameInIntValues || isValueNameInFloatValues ? DisplayStyle.Flex : DisplayStyle.None;
                Value_Field.style.display = isValueNameInIntValues || isValueNameInFloatValues ? DisplayStyle.Flex : DisplayStyle.None;
            });
            ValueName_Field.SetValueWithoutNotify(ValueName);
            mainContainer.Add(ValueName_Field);

            // Operation Enum Field
            AvatarPositionField = new EnumField("Operation", Operations);
            AvatarPositionField.RegisterValueChangedCallback(value =>
            {
                Operations = (GlobalValueIFOperations)value.newValue;
            });
            AvatarPositionField.SetValueWithoutNotify(Operations);
            mainContainer.Add(AvatarPositionField);

            // Value Field
            Value_Field = new TextField("Value");
            Value_Field.RegisterValueChangedCallback(value =>
            {
                OperationValue = value.newValue;
            });
            Value_Field.SetValueWithoutNotify(OperationValue);
            Value_Field.multiline = true;
            mainContainer.Add(Value_Field);
        }

        public override void LoadValueInToField()
        {
            GlobalValueManager manager = Resources.Load<GlobalValueManager>("GlobalValue");
            manager.LoadFile();

            bool isValueNameInIntValues = manager.IntValues.Any(intValue => intValue.ValueName == ValueName);
            bool isValueNameInFloatValues = manager.FloatValues.Any(floatValue => floatValue.ValueName == ValueName);

            // Ustaw widoczno�� AvatarPositionField w zale�no�ci od warunk�w
            AvatarPositionField.style.display = isValueNameInIntValues || isValueNameInFloatValues ? DisplayStyle.Flex : DisplayStyle.None;
            Value_Field.style.display = isValueNameInIntValues || isValueNameInFloatValues ? DisplayStyle.Flex : DisplayStyle.None;

            AvatarPositionField.SetValueWithoutNotify(Operations);
            Value_Field.SetValueWithoutNotify(OperationValue);
            ValueName_Field.SetValueWithoutNotify(ValueName);
        }

        /// <summary>
        /// Factory method to create a new IFNode instance and initialize its properties.
        /// </summary>
        /// <param name="_position">The position of the new node in the graph editor.</param>
        /// <param name="_editorWindow">The editor window managing the graph.</param>
        /// <param name="_graphView">The graph view where the node will be added.</param>
        /// <returns>A new IFNode instance.</returns>
        public static IFNode CreateNewGraphNode(Vector2 _position, DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {
            IFNode tmp = new IFNode(_position, _editorWindow, _graphView);
            tmp.name = "IF";
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
        public static IFNode GenerateNode(IfNodeData data, DialogueEditorWindow editor, DialogueGraphView graph)
        {
            IFNode newNode = CreateNewGraphNode(data.Position, editor, graph);
            newNode.nodeGuid = data.NodeGuid;

            newNode.ValueName = data.ValueName;
            newNode.Operations = data.Operations;
            newNode.OperationValue = data.OperationValue;

            newNode.LoadValueInToField();

            return newNode; // Return the generated node.
        }

        /// <summary>
        /// Serializes the node's state into a IFNodeData object for saving.
        /// </summary>
        /// <returns>A IFNodeData instance containing the node's serialized state.</returns>
        public IfNodeData SaveNodeData(List<Edge> edges)
        {
            IfNodeData nodeData = new IfNodeData
            {
                NodeGuid = nodeGuid,
                Position = GetPosition().position,

                ValueName = ValueName,
                Operations = Operations,
                OperationValue = OperationValue
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

            return nodeData; // Return the serialized data.
        }

        public override void SetValidation()
        {
            List<string> error = new List<string>();
            List<string> warning = new List<string>();

            Port input = inputContainer.Query<Port>().First();
            if (!input.connected) warning.Add("Node cannot be called");

            if (!outputContainer.Query<Port>().AtIndex(0).connected) error.Add("True does not lead to any node");
            if (!outputContainer.Query<Port>().AtIndex(1).connected) error.Add("False does not lead to any node");

            ErrorList = error;
            WarningList = warning;
        }
    }
}
