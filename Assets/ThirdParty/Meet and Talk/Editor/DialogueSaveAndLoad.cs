using System.Collections.Generic;
using System.Linq;
using MeetAndTalk.Nodes;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace MeetAndTalk.Editor
{
#if UNITY_EDITOR

    public class DialogueSaveAndLoad
    {
        private List<Edge> edges => graphView.edges.ToList();
        private List<BaseNode> nodes => graphView.nodes.ToList().Where(node => node is BaseNode).Cast<BaseNode>().ToList();

        public DialogueGraphView graphView;

        public DialogueSaveAndLoad(DialogueGraphView _graphView)
        {
            graphView = _graphView;
        }

        public void Save(DialogueContainerSO _dialogueContainerSO)
        {
            SaveEdges(_dialogueContainerSO);
            SaveNodes(_dialogueContainerSO);

            EditorUtility.SetDirty(_dialogueContainerSO);
            AssetDatabase.SaveAssets();
        }
        public void Load(DialogueContainerSO _dialogueContainerSO)
        {
            ClearGraph();
            GenerateNodes(_dialogueContainerSO);
            ConnectNodes(_dialogueContainerSO);
        }

        #region Save

        public void SaveEdges(DialogueContainerSO _dialogueContainerSO)
        {
            _dialogueContainerSO.NodeLinkDatas.Clear();

            Edge[] connectedEdges = edges.Where(edge => edge.input.node != null).ToArray();
            for (int i = 0; i < connectedEdges.Count(); i++)
            {
                BaseNode outputNode = (BaseNode)connectedEdges[i].output.node;
                BaseNode inputNode = connectedEdges[i].input.node as BaseNode;

                _dialogueContainerSO.NodeLinkDatas.Add(new NodeLinkData
                {
                    BaseNodeGuid = outputNode.nodeGuid,
                    TargetNodeGuid = inputNode.nodeGuid
                });
            }
        }

        private void SaveNodes(DialogueContainerSO _dialogueContainerSO)
        {
            _dialogueContainerSO.DialogueChoiceNodeDatas.Clear();
            _dialogueContainerSO.DialogueNodeDatas.Clear();
            _dialogueContainerSO.TimerChoiceNodeDatas.Clear();
            _dialogueContainerSO.EventNodeDatas.Clear();
            _dialogueContainerSO.EndNodeDatas.Clear();
            _dialogueContainerSO.StartNodeDatas.Clear();
            _dialogueContainerSO.RandomNodeDatas.Clear();
            _dialogueContainerSO.CommandNodeDatas.Clear();
            _dialogueContainerSO.IfNodeDatas.Clear();
            _dialogueContainerSO.AdvancedIFNodeDatas.Clear();
            _dialogueContainerSO.AdvancedChoiceNodeDatas.Clear();
            _dialogueContainerSO.AdvancedTimeChoiceNodeDatas.Clear();
            _dialogueContainerSO.ResetSavedNodeDatas.Clear();
            _dialogueContainerSO.MusicNodeDatas.Clear();

            nodes.ForEach(node =>
            {
                switch (node)
                {
                    case DialogueChoiceNode dialogueChoiceNode:
                        _dialogueContainerSO.DialogueChoiceNodeDatas.Add(dialogueChoiceNode.SaveNodeData(edges));
                        break;
                    case DialogueNode dialogueNode:
                        _dialogueContainerSO.DialogueNodeDatas.Add(dialogueNode.SaveNodeData());
                        break;
                    case TimerChoiceNode timerChoiceNode:
                        _dialogueContainerSO.TimerChoiceNodeDatas.Add(timerChoiceNode.SaveNodeData(edges));
                        break;
                    case StartNode startNode:
                        _dialogueContainerSO.StartNodeDatas.Add(startNode.SaveNodeData());
                        break;
                    case EndNode endNode:
                        _dialogueContainerSO.EndNodeDatas.Add(endNode.SaveNodeData());
                        break;
                    case EventNode eventNode:
                        _dialogueContainerSO.EventNodeDatas.Add(SaveNodeData(eventNode));
                        break;
                    case RandomNote randomNode:
                        _dialogueContainerSO.RandomNodeDatas.Add(SaveNodeData(randomNode));
                        break;
                    case CommandNode commandNode:
                        _dialogueContainerSO.CommandNodeDatas.Add(commandNode.SaveNodeData());
                        break;
                    case IFNode ifNode:
                        _dialogueContainerSO.IfNodeDatas.Add(ifNode.SaveNodeData(edges));
                        break;
                    case AdvancedIFNode advancedIFNode:
                        _dialogueContainerSO.AdvancedIFNodeDatas.Add(advancedIFNode.SaveNodeData(edges));
                        break;
                    case AdvancedTimeChoiceNode advancedChoiceNode:
                        _dialogueContainerSO.AdvancedTimeChoiceNodeDatas.Add(advancedChoiceNode.SaveNodeData(edges));
                        break;
                    case AdvancedChoiceNode advancedTimeChoiceNode:
                        _dialogueContainerSO.AdvancedChoiceNodeDatas.Add(advancedTimeChoiceNode.SaveNodeData(edges));
                        break;
                    case MusicNode musicNode:
                        _dialogueContainerSO.MusicNodeDatas.Add(musicNode.SaveNodeData());
                        break;
                    case ResetSavedNode resetSavedNode:
                        _dialogueContainerSO.ResetSavedNodeDatas.Add(resetSavedNode.SaveNodeData());
                        break;
                    default:
                        break;
                }
            });
        }

        private RandomNodeData SaveNodeData(RandomNote _node)
        {
            RandomNodeData dialogueNodeData = new RandomNodeData
            {
                NodeGuid = _node.nodeGuid,
                Position = _node.GetPosition().position,
            };


            return dialogueNodeData;
        }

        private EventNodeData SaveNodeData(EventNode _node)
        {
            EventNodeData nodeData = new EventNodeData
            {
                NodeGuid = _node.nodeGuid,
                Position = _node.GetPosition().position,
                EventScriptableObjects = _node.EventScriptableObjectDatas
            };
            return nodeData;
        }
        #endregion

        #region Load

        private void ClearGraph()
        {
            edges.ForEach(edge => graphView.RemoveElement(edge));

            foreach (BaseNode node in nodes)
            {
                graphView.RemoveElement(node);
            }
        }

        private void GenerateNodes(DialogueContainerSO _dialogueContainer)
        {
            // Generate all StartNode
            foreach (StartNodeData node in _dialogueContainer.StartNodeDatas)
            {
                graphView.AddElement(StartNode.GenerateNode(node, graphView.editorWindow, graphView));
            }

            // Generate all EndNode
            foreach (EndNodeData node in _dialogueContainer.EndNodeDatas)
            {
                graphView.AddElement(EndNode.GenerateNode(node, graphView.editorWindow, graphView));
            }

            // Generate all DialogueNode
            foreach (DialogueNodeData node in _dialogueContainer.DialogueNodeDatas)
            {
                graphView.AddElement(DialogueNode.GenerateNode(node, graphView.editorWindow, graphView));
            }

            // Generate All CommandNodes
            foreach (CommandNodeData node in _dialogueContainer.CommandNodeDatas)
            {
                graphView.AddElement(CommandNode.GenerateNode(node, graphView.editorWindow, graphView));
            }

            // Generate All MusicNodes
            foreach (MusicNodeData node in _dialogueContainer.MusicNodeDatas)
            {
                graphView.AddElement(MusicNode.GenerateNode(node, graphView.editorWindow, graphView));
            }

            // Generate All CommandNodes
            foreach (ResetSavedNodeData node in _dialogueContainer.ResetSavedNodeDatas)
            {
                graphView.AddElement(ResetSavedNode.GenerateNode(node, graphView.editorWindow, graphView));
            }



            /* Event Node */
            foreach (EventNodeData node in _dialogueContainer.EventNodeDatas)
            {
                EventNode tempNode = graphView.CreateEventNode(node.Position);
                tempNode.nodeGuid = node.NodeGuid;

                foreach (EventScriptableObjectData item in node.EventScriptableObjects)
                {
                    tempNode.AddScriptableEvent(item);
                    //tempNode.GenerateFields(item);
                }

                tempNode.LoadValueInToField();
                graphView.AddElement(tempNode);
            }

            // Generate All IFNodes
            foreach (IfNodeData node in _dialogueContainer.IfNodeDatas)
            {
                graphView.AddElement(IFNode.GenerateNode(node, graphView.editorWindow, graphView));
            }
            // Generate Advance All IFNodes
            foreach (AdvancedIFNodeData node in _dialogueContainer.AdvancedIFNodeDatas)
            {
                graphView.AddElement(AdvancedIFNode.GenerateNode(node, graphView.editorWindow, graphView));
            }

            // Generate Dialogue Choice Node
            foreach (DialogueChoiceNodeData node in _dialogueContainer.DialogueChoiceNodeDatas)
            {
                graphView.AddElement(DialogueChoiceNode.GenerateNode(node, graphView.editorWindow, graphView));
            }

            // Generate Advanced Choice Node
            foreach (AdvancedChoiceNodeData node in _dialogueContainer.AdvancedChoiceNodeDatas)
            {
                graphView.AddElement(AdvancedChoiceNode.GenerateNode(node, graphView.editorWindow, graphView));
            }

            // Generate Advanced Tim e Choice Node
            foreach (AdvancedTimeChoiceNodeData node in _dialogueContainer.AdvancedTimeChoiceNodeDatas)
            {
                graphView.AddElement(AdvancedTimeChoiceNode.GenerateNode(node, graphView.editorWindow, graphView));
            }

            // Generate Advanced Tim e Choice Node
            foreach (RandomNodeData node in _dialogueContainer.RandomNodeDatas)
            {
                graphView.AddElement(RandomNote.GenerateNode(node, graphView.editorWindow, graphView));
            }

            //  Generate Advanced Time Choice Node
            foreach (TimerChoiceNodeData node in _dialogueContainer.TimerChoiceNodeDatas)
            {
                graphView.AddElement(TimerChoiceNode.GenerateNode(node, graphView.editorWindow, graphView));
            }
        }

        private void ConnectNodes(DialogueContainerSO _dialogueContainer)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                List<NodeLinkData> connections = _dialogueContainer.NodeLinkDatas.Where(edge => edge.BaseNodeGuid == nodes[i].nodeGuid).ToList();

                for (int j = 0; j < connections.Count; j++)
                {
                    string targetNodeGuid = connections[j].TargetNodeGuid;
                    BaseNode targetNode = nodes.First(node => node.nodeGuid == targetNodeGuid);

                    if (nodes[i] is RandomNote)
                    {
                        if (nodes[i].outputContainer[0].Q<Port>() == null) Debug.LogError("1");
                        if ((Port)targetNode.inputContainer[0] == null) Debug.LogError("2");
                        LinkNodesTogether(nodes[i].outputContainer[0].Q<Port>(), (Port)targetNode.inputContainer[0]);
                    }
                    // Li
                    else if (!(nodes[i] is DialogueChoiceNode) && !(nodes[i] is TimerChoiceNode) && !(nodes[i] is AdvancedTimeChoiceNode) && !(nodes[i] is AdvancedChoiceNode))
                    {
                        LinkNodesTogether(nodes[i].outputContainer[j].Q<Port>(), (Port)targetNode.inputContainer[0]);
                    }
                }
            }

            // ?
            
            List<DialogueChoiceNode> dialogueNodes = nodes.FindAll(node => node is DialogueChoiceNode).Cast<DialogueChoiceNode>().ToList();
            foreach (DialogueChoiceNode dialogueNode in dialogueNodes)
            {
                foreach (DialogueNodePort nodePort in dialogueNode.dialogueNodePorts)
                {
                    if (nodePort.InputGuid != string.Empty)
                    {
                        BaseNode targetNode = nodes.First(Node => Node.nodeGuid == nodePort.InputGuid);
                        LinkNodesTogether(nodePort.MyPort, (Port)targetNode.inputContainer[0]);
                    }
                }
            }

            List<TimerChoiceNode> timerChoiceNodes = nodes.FindAll(node => node is TimerChoiceNode).Cast<TimerChoiceNode>().ToList();
            foreach (TimerChoiceNode dialogueNode in timerChoiceNodes)
            {
                List<NodeLinkData> AllLinkedData = _dialogueContainer.NodeLinkDatas.Where(edge => edge.BaseNodeGuid == dialogueNode.nodeGuid).ToList();
                //Debug.Log(AllLinkedData.Count);

                foreach (DialogueNodePort nodePort in dialogueNode.dialogueNodePorts)
                {
                    if (nodePort.InputGuid != string.Empty)
                    {
                        BaseNode targetNode = nodes.First(Node => Node.nodeGuid == nodePort.InputGuid);
                        LinkNodesTogether(nodePort.MyPort, (Port)targetNode.inputContainer[0]);

                        AllLinkedData.RemoveAt(0);
                    }
                }

                foreach (NodeLinkData nodeLink in AllLinkedData)
                {
                    LinkNodesTogether((Port)dialogueNode.outputContainer[0], (Port)nodes.Find(node => node.nodeGuid == nodeLink.TargetNodeGuid).inputContainer[0]); //(Port)targetNode.inputContainer[0]);
                }
            }

            List<AdvancedTimeChoiceNode> advancedTimeChoiceNodes = nodes.FindAll(node => node is AdvancedTimeChoiceNode).Cast<AdvancedTimeChoiceNode>().ToList();
            foreach (AdvancedTimeChoiceNode dialogueNode in advancedTimeChoiceNodes)
            {
                List<NodeLinkData> AllLinkedData = _dialogueContainer.NodeLinkDatas.Where(edge => edge.BaseNodeGuid == dialogueNode.nodeGuid).ToList();
                Debug.Log(AllLinkedData.Count);

                foreach (AdvancedDialogueNodePort nodePort in dialogueNode.dialogueNodePorts)
                {
                    if (nodePort.InputGuid != string.Empty)
                    {
                        BaseNode targetNode = nodes.First(Node => Node.nodeGuid == nodePort.InputGuid);
                        LinkNodesTogether(nodePort.MyPort, (Port)targetNode.inputContainer[0]);

                        AllLinkedData.RemoveAt(0);
                    }
                }

                foreach (NodeLinkData nodeLink in AllLinkedData)
                {
                    LinkNodesTogether((Port)dialogueNode.outputContainer[0], (Port)nodes.Find(node => node.nodeGuid == nodeLink.TargetNodeGuid).inputContainer[0]); //(Port)targetNode.inputContainer[0]);
                }
            }

            List<AdvancedChoiceNode> advancedChoiceNodes = nodes.FindAll(node => node is AdvancedChoiceNode).Cast<AdvancedChoiceNode>().ToList();
            foreach (AdvancedChoiceNode dialogueNode in advancedChoiceNodes)
            {
                List<NodeLinkData> AllLinkedData = _dialogueContainer.NodeLinkDatas.Where(edge => edge.BaseNodeGuid == dialogueNode.nodeGuid).ToList();
                Debug.Log(AllLinkedData.Count);

                foreach (AdvancedDialogueNodePort nodePort in dialogueNode.dialogueNodePorts)
                {
                    if (nodePort.InputGuid != string.Empty)
                    {
                        BaseNode targetNode = nodes.First(Node => Node.nodeGuid == nodePort.InputGuid);
                        LinkNodesTogether(nodePort.MyPort, (Port)targetNode.inputContainer[0]);

                        AllLinkedData.RemoveAt(0);
                    }
                }

                foreach (NodeLinkData nodeLink in AllLinkedData)
                {
                    LinkNodesTogether((Port)dialogueNode.outputContainer[0], (Port)nodes.Find(node => node.nodeGuid == nodeLink.TargetNodeGuid).inputContainer[0]); //(Port)targetNode.inputContainer[0]);
                }
            }
        }

        private void LinkNodesTogether(Port _outputPort, Port _inputPort)
        {
            Edge tempEdge = new Edge()
            {
                output = _outputPort,
                input = _inputPort
            };
            tempEdge.input.Connect(tempEdge);
            tempEdge.output.Connect(tempEdge);
            graphView.Add(tempEdge);
        }

        #endregion
    }

#endif
}
