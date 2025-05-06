using System;
using System.Collections.Generic;
using MeetAndTalk.Editor;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace MeetAndTalk.Nodes
{
    [System.Serializable]
    public class ResetSavedNode : BaseNode
    {
        public ResetSavedNode() { }

        public ResetSavedNode(Vector2 _position, DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {
            // Assign references to the editor window and graph view for later use.
            editorWindow = _editorWindow;
            graphView = _graphView;

            // Configure the base properties of the node.
            title = "Reset Saved Node";
            SetPosition(new Rect(_position, defualtNodeSize)); // Position the node in the graph view.
            nodeGuid = Guid.NewGuid().ToString(); // Generate a unique identifier for the node.

            // Add an output port to the node with a single connection capacity.
            AddInputPort("Input", Port.Capacity.Single);
            AddOutputPort("Output", Port.Capacity.Single);

            GenerateBetterTitle("Reset Saved Node", "Disable \"Go Back\" Function", "Icon/NodeIcon/ResetData");


            // Refresh the node's state and UI.
            RefreshExpandedState(); // Update the expanded/collapsed state of the node.
            RefreshPorts(); // Ensure all ports are correctly configured and displayed.
            AddValidationContainer(); // Add containers for error and warning messages to the node.
        }

        /// <summary>
        /// Factory method to create a new ResetSavedNode instance and initialize its properties.
        /// </summary>
        /// <param name="_position">The position of the new node in the graph editor.</param>
        /// <param name="_editorWindow">The editor window managing the graph.</param>
        /// <param name="_graphView">The graph view where the node will be added.</param>
        /// <returns>A new ResetSavedNode instance.</returns>
        public static ResetSavedNode CreateNewGraphNode(Vector2 _position, DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {
            ResetSavedNode tmp = new ResetSavedNode(_position, _editorWindow, _graphView); // Create the ResetSavedNode instance.
            tmp.name = "ResetSaved"; // Set the node's name.
            tmp.ReloadLanguage(); // Reload any language-related fields.

            return tmp; // Return the newly created node.
        }

        /// <summary>
        /// Generates a StartNode based on saved data, restoring its state and properties.
        /// </summary>
        /// <param name="data">The serialized data to use for generating the node.</param>
        /// <param name="editor">The editor window managing the graph.</param>
        /// <param name="graph">The graph view where the node will be added.</param>
        /// <returns>A restored StartNode instance.</returns>
        public static ResetSavedNode GenerateNode(ResetSavedNodeData data, DialogueEditorWindow editor, DialogueGraphView graph)
        {
            // Generate a new StartNode using its factory method.
            ResetSavedNode newNode = ResetSavedNode.CreateNewGraphNode(data.Position, editor, graph);

            // Restore the node's unique identifier and start ID.
            newNode.nodeGuid = data.NodeGuid;

            // Load the restored values into the corresponding fields.
            newNode.LoadValueInToField();

            return newNode; // Return the generated node.
        }

        /// <summary>
        /// Serializes the node's state into a StartNodeData object for saving.
        /// </summary>
        /// <returns>A StartNodeData instance containing the node's serialized state.</returns>
        public ResetSavedNodeData SaveNodeData()
        {
            ResetSavedNodeData nodeData = new ResetSavedNodeData
            {
                NodeGuid = nodeGuid, // Save the node's unique identifier.
                Position = GetPosition().position, // Save the node's position in the graph.
            };
            return nodeData; // Return the serialized data.
        }

        /// <summary>
        /// Validates the node's connections and updates error and warning lists.
        /// </summary>
        public override void SetValidation()
        {
            // Reset validation lists for errors and warnings.
            List<string> error = new List<string>();
            List<string> warning = new List<string>();

            // Error: Check if the output port is connected to another node.
            Port port = outputContainer.Query<Port>().First(); // Retrieve the output port.
            if (!port.connected) error.Add("Output does not lead to any node"); // Add error if no connection exists.

            // Assign validation results to the node's error and warning lists.
            ErrorList = error;
            WarningList = warning;
        }
    }
}
