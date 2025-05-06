using System;
using System.Collections.Generic;
using MeetAndTalk.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MeetAndTalk.Nodes
{
    [System.Serializable]
    public class CommandNode : BaseNode
    {
        public string command;
        private TextField textField;

        public CommandNode() { }

        /// <summary>
        /// Constructor for the CommandNode class, initializes the node with default properties, 
        /// UI elements, and validation capabilities.
        /// </summary>
        /// <param name="_position">The initial position of the node in the editor graph.</param>
        /// <param name="_editorWindow">The instance of the editor window managing this node.</param>
        /// <param name="_graphView">The graph view where this node is displayed.</param>
        public CommandNode(Vector2 _position, DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {            
            // Assign references to the editor window and graph view for later use.
            editorWindow = _editorWindow;
            graphView = _graphView;

            // Configure the base properties of the node.
            title = "Editor Notes";
            SetPosition(new Rect(_position, defualtNodeSize));
            nodeGuid = Guid.NewGuid().ToString();

            // Add Better Title
            GenerateBetterTitle("Editor Notes", "�Sticky Note� for saving notes in the editor", "Icon/NodeIcon/Note");

            textField = new TextField("");
            textField.RegisterValueChangedCallback(value =>
            {
                command = value.newValue;
            });

            textField.SetValueWithoutNotify(command);

            textField.multiline = true;
            textField.AddToClassList("TextBox");

            mainContainer.Add(textField);
        }

        public override void LoadValueInToField()
        {
            textField.SetValueWithoutNotify(command);
        }

        /// <summary>
        /// Factory method to create a new CommandNode instance and initialize its properties.
        /// </summary>
        /// <param name="_position">The position of the new node in the graph editor.</param>
        /// <param name="_editorWindow">The editor window managing the graph.</param>
        /// <param name="_graphView">The graph view where the node will be added.</param>
        /// <returns>A new CommandNode instance.</returns>
        public static CommandNode CreateNewGraphNode(Vector2 _position, DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {
            CommandNode tmp = new CommandNode(_position, _editorWindow, _graphView);
            tmp.name = "Command";

            return tmp; // Return the newly created node.
        }

        /// <summary>
        /// Generates a CommandNode based on saved data, restoring its state and properties.
        /// </summary>
        /// <param name="data">The serialized data to use for generating the node.</param>
        /// <param name="editor">The editor window managing the graph.</param>
        /// <param name="graph">The graph view where the node will be added.</param>
        /// <returns>A restored CommandNode instance.</returns>
        public static CommandNode GenerateNode(CommandNodeData data, DialogueEditorWindow editor, DialogueGraphView graph)
        {
            // Generate a new EndNode using its factory method.
            CommandNode newNode = CommandNode.CreateNewGraphNode(data.Position, editor, graph);

            // Restore the node's unique identifier and start ID.
            newNode.nodeGuid = data.NodeGuid;
            newNode.command = data.commmand;

            // Load the restored values into the corresponding fields.
            newNode.LoadValueInToField();

            return newNode; // Return the generated node.
        }

        /// <summary>
        /// Serializes the node's state into a CommandNodeData object for saving.
        /// </summary>
        /// <returns>A CommandNodeData instance containing the node's serialized state.</returns>
        public CommandNodeData SaveNodeData() 
        {
            CommandNodeData nodeData = new CommandNodeData
            {
                NodeGuid = nodeGuid,                    // Save the node's unique identifier.
                Position = GetPosition().position,      // Save the node's position in the graph.
                commmand = command
            };
            return nodeData; // Return the serialized data.
        }
    }
}
