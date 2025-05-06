using System;
using System.Collections.Generic;
using System.Linq;
using MeetAndTalk.Editor;
using MeetAndTalk.Event;
using MeetAndTalk.Localization;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MeetAndTalk.Nodes
{

    public class RandomNote : BaseNode
    {
        public RandomNote()
        {

        }

        public RandomNote(Vector2 _position, DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {
            editorWindow = _editorWindow;
            graphView = _graphView;

            title = "Random Output";
            SetPosition(new Rect(_position, defualtNodeSize));
            nodeGuid = Guid.NewGuid().ToString();

            // Add Better Title
            GenerateBetterTitle("Random Output", "Randomizes next Node", "Icon/NodeIcon/Random");

            AddInputPort("Input", Port.Capacity.Multi);
            AddOutputPort("Output", Port.Capacity.Multi);
            AddValidationContainer();

            
        }

        public void ReloadLanguage()
        {
        }

        public override void LoadValueInToField()
        {

        }

        public static RandomNote CreateNewGraphNode(Vector2 _position, DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {
            RandomNote tmp = new RandomNote(_position, _editorWindow, _graphView);
            tmp.name = "Random";
            tmp.ReloadLanguage();

            return tmp;
        }

        public static RandomNote GenerateNode(RandomNodeData data, DialogueEditorWindow editor, DialogueGraphView graph)
        {
            // Generate a new EndNode using its factory method.
            RandomNote newNode = RandomNote.CreateNewGraphNode(data.Position, editor, graph);

            // Restore the node's unique identifier and start ID.
            newNode.nodeGuid = data.NodeGuid;

            // Load the restored values into the corresponding fields.
            newNode.LoadValueInToField();

            return newNode; // Return the generated node.
        }

        public override void SetValidation()
        {
            List<string> error = new List<string>();
            List<string> warning = new List<string>();

            Port input = inputContainer.Query<Port>().First();
            if (!input.connected) warning.Add("Node cannot be called");

            Port output = outputContainer.Query<Port>().First();
            if (!output.connected) error.Add("Output does not lead to any node");

            ErrorList = error;
            WarningList = warning;
        }
    }
}
