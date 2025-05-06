using System;
using System.Collections.Generic;
using Codice.CM.Common;
using MeetAndTalk.Editor;
using MeetAndTalk.Localization;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

namespace MeetAndTalk.Nodes
{
    [System.Serializable]
    public class MusicNode : BaseNode
    {
        public List<LanguageGeneric<AudioClip>> audioClip = new List<LanguageGeneric<AudioClip>>();
        private ObjectField audioClips_Field;

        public float SwitchTime = 2f;
        public FloatField switchTimeField;

        public MusicNode() { }

        public MusicNode(Vector2 _position, DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {
            // Assign references to the editor window and graph view for later use.
            editorWindow = _editorWindow;
            graphView = _graphView;

            // Configure the base properties of the node.
            title = "Change Music"; // Set the node title to "Start".
            SetPosition(new Rect(_position, defualtNodeSize)); // Position the node in the graph view.
            nodeGuid = Guid.NewGuid().ToString(); // Generate a unique identifier for the node.

            // Add Better Title
            GenerateBetterTitle("Change Music", "Set Background Music", "Icon/NodeIcon/Music");

            // Add an output port to the node with a single connection capacity.
            AddInputPort("Input", Port.Capacity.Single);
            AddOutputPort("Output", Port.Capacity.Single);

            // Load Language Values
            foreach (LocalizationEnum language in (LocalizationEnum[])Enum.GetValues(typeof(LocalizationEnum)))
            {
                audioClip.Add(new LanguageGeneric<AudioClip>
                {
                    languageEnum = language,
                    LanguageGenericType = null
                });
            }

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

            switchTimeField = new FloatField("Change Time")
            {
                value = SwitchTime
            };
            switchTimeField.RegisterValueChangedCallback(value =>
            {
                SwitchTime = value.newValue;
            });
            switchTimeField.SetValueWithoutNotify(SwitchTime);
            mainContainer.Add(switchTimeField);

            // Refresh the node's state and UI.
            RefreshExpandedState(); // Update the expanded/collapsed state of the node.
            RefreshPorts(); // Ensure all ports are correctly configured and displayed.
            AddValidationContainer(); // Add containers for error and warning messages to the node.
        }

        public override void ReloadLanguage()
        {
            audioClips_Field.RegisterValueChangedCallback(value =>
            {
                audioClip.Find(audioClips => audioClips.languageEnum == editorWindow.LanguageEnum).LanguageGenericType = value.newValue as AudioClip;
            });
            audioClips_Field.SetValueWithoutNotify(audioClip.Find(audioClips => audioClips.languageEnum == editorWindow.LanguageEnum).LanguageGenericType);
        }

        public override void LoadValueInToField()
        {
            audioClips_Field.SetValueWithoutNotify(audioClip.Find(language => language.languageEnum == editorWindow.LanguageEnum).LanguageGenericType);
            switchTimeField.SetValueWithoutNotify(SwitchTime);
        }

        /// <summary>
        /// Factory method to create a new ResetSavedNode instance and initialize its properties.
        /// </summary>
        /// <param name="_position">The position of the new node in the graph editor.</param>
        /// <param name="_editorWindow">The editor window managing the graph.</param>
        /// <param name="_graphView">The graph view where the node will be added.</param>
        /// <returns>A new ResetSavedNode instance.</returns>
        public static MusicNode CreateNewGraphNode(Vector2 _position, DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {
            MusicNode tmp = new MusicNode(_position, _editorWindow, _graphView); // Create the ResetSavedNode instance.
            tmp.name = "Music"; // Set the node's name.
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
        public static MusicNode GenerateNode(MusicNodeData data, DialogueEditorWindow editor, DialogueGraphView graph)
        {
            // Generate a new StartNode using its factory method.
            MusicNode newNode = MusicNode.CreateNewGraphNode(data.Position, editor, graph);

            // Restore the node's unique identifier and start ID.
            newNode.nodeGuid = data.NodeGuid;
            newNode.SwitchTime = data.SwitchTime;

            foreach (LanguageGeneric<AudioClip> languageGeneric in data.AudioClips)
            {
                newNode.audioClip.Find(language => language.languageEnum == languageGeneric.languageEnum).LanguageGenericType = languageGeneric.LanguageGenericType;
            }

            // Load the restored values into the corresponding fields.
            newNode.LoadValueInToField();

            return newNode; // Return the generated node.
        }

        /// <summary>
        /// Serializes the node's state into a StartNodeData object for saving.
        /// </summary>
        /// <returns>A StartNodeData instance containing the node's serialized state.</returns>
        public MusicNodeData SaveNodeData()
        {
            MusicNodeData nodeData = new MusicNodeData
            {
                NodeGuid = nodeGuid, // Save the node's unique identifier.
                Position = GetPosition().position, // Save the node's position in the graph.
                AudioClips = audioClip,
                SwitchTime = SwitchTime,
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

            // Error
            for (int i = 0; i < audioClip.Count; i++) { if (audioClip[i].LanguageGenericType == null) warning.Add($"No Music for {audioClip[i].languageEnum} Language"); }


            // Assign validation results to the node's error and warning lists.
            ErrorList = error;
            WarningList = warning;
        }
    }
}