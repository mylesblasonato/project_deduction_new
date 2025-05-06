using System.Collections.Generic;
using MeetAndTalk.Nodes;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace MeetAndTalk.Editor
{
    public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private DialogueEditorWindow editorWindow;
        private DialogueGraphView graphView;

        public void Configure(DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {
            editorWindow = _editorWindow;
            graphView = _graphView;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> tree = new List<SearchTreeEntry>
        {
            new SearchTreeGroupEntry(new GUIContent("Dialogue Node"),0),

            new SearchTreeGroupEntry(new GUIContent("Base Dialogue Node",EditorGUIUtility.FindTexture("d_FilterByType")),1),

            AddNodeSearchToGroup("Dialogue Start",new StartNode(),"Icon/NodeIcon/Start"),
            AddNodeSearchToGroup("Dialogue",new DialogueNode(),"Icon/NodeIcon/Dialogue"),
            AddNodeSearchToGroup("Dialogue Choice",new DialogueChoiceNode(),"Icon/NodeIcon/Choice"),
            AddNodeSearchToGroup("Advanced Choice",new AdvancedChoiceNode(),"Icon/NodeIcon/AdvancedChoice"),
            AddNodeSearchToGroup("Dialogue Time Choice",new TimerChoiceNode(),"Icon/NodeIcon/TimeChoice"),
            AddNodeSearchToGroup("Advanced Time Choice",new AdvancedTimeChoiceNode(),"Icon/NodeIcon/AdvancedTimeChoice"),
            AddNodeSearchToGroup("Dialogue End",new EndNode(),"Icon/NodeIcon/End"),

            new SearchTreeGroupEntry(new GUIContent("Functional Dialogue Node",EditorGUIUtility.FindTexture("d_Favorite Icon")),1),

            AddNodeSearchToGroup("Event Invoker",new EventNode(),"Icon/NodeIcon/Event"),
            AddNodeSearchToGroup("Random Output",new RandomNote(),"Icon/NodeIcon/Random"),
            AddNodeSearchToGroup("Simple Branch",new IFNode(),"Icon/NodeIcon/Branch"),
            AddNodeSearchToGroup("Advanced Branch",new AdvancedIFNode(),"Icon/NodeIcon/AdvancedBranch"),
            AddNodeSearchToGroup("Reset Saved Nodes",new ResetSavedNode(),"Icon/NodeIcon/ResetData"),
            AddNodeSearchToGroup("Change Music",new MusicNode(),"Icon/NodeIcon/Music"),

            new SearchTreeGroupEntry(new GUIContent("Decoration Dialogue Node",EditorGUIUtility.FindTexture("d_Favorite Icon")),1),

            AddNodeSearchToGroup("Editor Notes",new CommandNode(),"Icon/NodeIcon/Note"),
        };

            return tree;
        }

        private SearchTreeEntry AddNodeSearchToGroup(string _name, BaseNode _baseNode, string IconName)
        {
            Texture2D _icon = Resources.Load<Texture2D>(IconName);
            SearchTreeEntry tmp = new SearchTreeEntry(new GUIContent(_name, _icon))
            {
                level = 2,
                userData = _baseNode
            };

            return tmp;
        }

        private SearchTreeEntry AddNodeSearch(string _name, BaseNode _baseNode, string IconName)
        {
            Texture2D _icon = EditorGUIUtility.FindTexture(IconName) as Texture2D;
            SearchTreeEntry tmp = new SearchTreeEntry(new GUIContent(_name, _icon))
            {
                level = 1,
                userData = _baseNode
            };

            return tmp;
        }

        public bool OnSelectEntry(SearchTreeEntry _searchTreeEntry, SearchWindowContext _context)
        {
            Vector2 mousePosition = editorWindow.rootVisualElement.ChangeCoordinatesTo
                (
                editorWindow.rootVisualElement.parent, _context.screenMousePosition - editorWindow.position.position
                );
            Vector2 graphMousePosition = graphView.contentViewContainer.WorldToLocal(mousePosition);

            return CheckForNodeType(_searchTreeEntry, graphMousePosition);
        }

        private bool CheckForNodeType(SearchTreeEntry _searchTreeEntry, Vector2 _pos)
        {
            switch (_searchTreeEntry.userData)
            {
                case StartNode node:
                    graphView.AddElement(StartNode.CreateNewGraphNode(_pos, editorWindow, graphView));
                    return true;
                case DialogueNode node:
                    graphView.AddElement(DialogueNode.CreateNewGraphNode(_pos, editorWindow, graphView));
                    return true;
                case DialogueChoiceNode node:
                    graphView.AddElement(DialogueChoiceNode.CreateNewGraphNode(_pos, editorWindow, graphView));
                    return true;
                case TimerChoiceNode node:
                    graphView.AddElement(graphView.CreateTimerChoiceNode(_pos));
                    return true;
                case AdvancedChoiceNode node:
                    graphView.AddElement(AdvancedChoiceNode.CreateNewGraphNode(_pos, editorWindow, graphView));
                    return true;
                case AdvancedTimeChoiceNode node:
                    graphView.AddElement(AdvancedTimeChoiceNode.CreateNewGraphNode(_pos, editorWindow, graphView));
                    return true;
                case EventNode node:
                    graphView.AddElement(graphView.CreateEventNode(_pos));
                    return true;
                case EndNode node:
                    graphView.AddElement(EndNode.CreateNewGraphNode(_pos, editorWindow, graphView));
                    return true;
                case RandomNote node:
                    graphView.AddElement(RandomNote.CreateNewGraphNode(_pos, editorWindow, graphView));
                    return true;
                case CommandNode node:
                    graphView.AddElement(CommandNode.CreateNewGraphNode(_pos, editorWindow, graphView));
                    return true;
                case IFNode node:
                    graphView.AddElement(IFNode.CreateNewGraphNode(_pos, editorWindow, graphView));
                    return true;
                case AdvancedIFNode node:
                    graphView.AddElement(AdvancedIFNode.CreateNewGraphNode(_pos, editorWindow, graphView));
                    return true;
                case ResetSavedNode node:
                    graphView.AddElement(ResetSavedNode.CreateNewGraphNode(_pos, editorWindow, graphView));
                    return true;
                case MusicNode node:
                    graphView.AddElement(MusicNode.CreateNewGraphNode(_pos, editorWindow, graphView));
                    return true;
            }
            return false;
        }
    }
}