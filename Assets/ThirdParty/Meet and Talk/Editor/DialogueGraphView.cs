using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using MeetAndTalk.Nodes;
using MeetAndTalk.Settings;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MeetAndTalk.Editor
{
    [ExecuteInEditMode]
    public class DialogueGraphView : GraphView
    {
        // Reference to the Dialogue Editor Window
        public DialogueEditorWindow editorWindow;

        // Reference to the Node Search Window
        private NodeSearchWindow searchWindow;

        // Settings Box
        public Box settingsBox;
        public Toggle AutoSave, AllowSave, BlockReopening, ResetSavedOnChoice, LimitChoice, ShowMinimap, ShowWarning, ShowError;
        public IntegerField MaxChoice;

        public MiniMap minimap;

        //
        public int Errors, Warnings;

        public DialogueGraphView(DialogueEditorWindow _editorWindow)
        {
            editorWindow = _editorWindow;

            StyleSheet tmpStyleSheet = Resources.Load<StyleSheet>("Themes/DarkTheme");
            styleSheets.Add(tmpStyleSheet);

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            //Debug.Log($"DEBUGGING: {ContentZoomer.DefaultMinScale} - {ContentZoomer.DefaultMaxScale}");

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new FreehandSelector());

            /*serializeGraphElements += CutCopyOperation;
            unserializeAndPaste += PasteOperation;
            canPasteSerializedData += CanPaste;*/
            ///unserializeAndPaste += OnPause

            //Debug.Log(canPaste);
            //Debug.Log(canCopySelection);

            GridBackground grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            AddSearchWindow();
            AddMiniMap();
            AddSettings();

            //this.AddManipulator(CreateGroupContextualMenu());
        }

        public void ValidateDialogue()
        {
            Errors = 0;
            Warnings = 0;

            List<BaseNode> bases = nodes.ToList().Where(node => node is BaseNode).Cast<BaseNode>().ToList();
            foreach (BaseNode node in bases) 
            { 
                node.Validate();

                // Add to Indicators
                Warnings += node.WarningList.Count;
                Errors += node.ErrorList.Count;

                node.UpdateNodeUI();
            }


            //Debug.Log(canCopySelection);
        }

        public void UpdateTheme(string name)
        {
            styleSheets.Remove(styleSheets[styleSheets.count - 1]);
            styleSheets.Add(Resources.Load<StyleSheet>($"Themes/{name}Theme"));

            List<BaseNode> bases = nodes.ToList().Where(node => node is BaseNode).Cast<BaseNode>().ToList();
            foreach (BaseNode node in bases) { node.UpdateTheme(name); }

        }

        private void AddMiniMap()
        {
            minimap = new MiniMap()
            {
                anchored = true,
                elementTypeColor = Color.green,
                name = "minimap",
                maxHeight = 100,
                maxWidth = 150
            };
            minimap.SetPosition(new Rect(0, 14, 200, 100));
            Add(minimap);
        }

        private void AddSettings()
        {
            settingsBox = new Box();
            settingsBox.AddToClassList("settingBox");
            settingsBox.style.width = 220;
            settingsBox.style.top = 34;
            settingsBox.style.right = 4;
            settingsBox.style.position = Position.Absolute;


            Label _title2 = new Label("Editor Settings");
            _title2.style.unityTextAlign = TextAnchor.MiddleCenter;
            _title2.style.fontSize = 10;
            _title2.style.unityFontStyleAndWeight = FontStyle.Bold;
            _title2.AddToClassList("title");
            settingsBox.Add(_title2);

            AutoSave = new Toggle($"Auto Save Dialogue [{Resources.Load<MeetAndTalkSettings>("MeetAndTalkSettings").AutoSaveInterval} Sec]");
            AutoSave.RegisterValueChangedCallback(evt =>
            {
                editorWindow.AutoSave = evt.newValue;
                Resources.Load<MeetAndTalkSettings>("MeetAndTalkSettings").AutoSave = evt.newValue;
                editorWindow.Save();
            });
            AutoSave.value = Resources.Load<MeetAndTalkSettings>("MeetAndTalkSettings").AutoSave;
            settingsBox.Add(AutoSave);

            ShowMinimap = new Toggle($"Show Minimap in Dialogue Editor");
            ShowMinimap.RegisterValueChangedCallback(evt =>
            {
                Resources.Load<MeetAndTalkSettings>("MeetAndTalkSettings").ShowMinimap = evt.newValue;
            });
            ShowMinimap.value = Resources.Load<MeetAndTalkSettings>("MeetAndTalkSettings").ShowMinimap;
            settingsBox.Add(ShowMinimap);

            ShowWarning = new Toggle($"Show Warning's in Editor");
            ShowWarning.RegisterValueChangedCallback(evt =>
            {
                Resources.Load<MeetAndTalkSettings>("MeetAndTalkSettings").ShowWarnings = evt.newValue;
            });
            ShowWarning.value = Resources.Load<MeetAndTalkSettings>("MeetAndTalkSettings").ShowWarnings;
            settingsBox.Add(ShowWarning);

            ShowError = new Toggle($"Show Error's in Editor");
            ShowError.RegisterValueChangedCallback(evt =>
            {
                Resources.Load<MeetAndTalkSettings>("MeetAndTalkSettings").ShowErrors = evt.newValue;
            });
            ShowError.value = Resources.Load<MeetAndTalkSettings>("MeetAndTalkSettings").ShowErrors;
            settingsBox.Add(ShowError);

            Button settingsBtn = new Button();
            settingsBtn.text = "Open Advanced Meet and Talk Settings";
            settingsBtn.style.fontSize = 10;
            settingsBtn.clicked += () =>
            {
                SettingsService.OpenProjectSettings("Project/Meet and Talk");
            };
            settingsBox.Add(settingsBtn);


            Label _title = new Label("Dialogue Settings");
            _title.style.unityTextAlign = TextAnchor.MiddleCenter;
            _title.style.fontSize = 10;
            _title.style.unityFontStyleAndWeight = FontStyle.Bold;
            _title.AddToClassList("title");
            settingsBox.Add(_title);

            AllowSave = new Toggle("Allow Save Progression");
            //AllowSave.value = editorWindow.currentDialogueContainer.AllowDialogueSave;
            settingsBox.Add(AllowSave);

            BlockReopening = new Toggle("Block Reopening when Ended");
            //BlockReopening.value = editorWindow.currentDialogueContainer.BlockingReopeningDialogue;
            settingsBox.Add(BlockReopening);

            ResetSavedOnChoice = new Toggle("Disable \"Go Back\" on after Choice");
            //ResetSavedOnChoice.value = editorWindow.currentDialogueContainer.ResetSavedNodeOnChoice;
            settingsBox.Add(ResetSavedOnChoice);

            LimitChoice = new Toggle("Limit Max Choice Count per Node");
            //LimitChoice.value = editorWindow.currentDialogueContainer.LimitChoiceOptionPerNode;
            settingsBox.Add(LimitChoice);

            MaxChoice = new IntegerField("Limit of Choice per Node");
            //MaxChoice.value = editorWindow.currentDialogueContainer.MaxChoiceOptionPerNode;
            settingsBox.Add(MaxChoice);

            Add(settingsBox);
        }

        private void AddSearchWindow()
        {
            searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
            searchWindow.Configure(editorWindow, this);
            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
        }

        IManipulator CreateGroupContextualMenu()
        {
            ContextualMenuManipulator contextual = new ContextualMenuManipulator
            (
                menuEvent => menuEvent.menu.AppendAction("Add Group", actionEvent => AddElement(CreateGroup("New Group", actionEvent.eventInfo.mousePosition)))
            );

            return contextual;
        }

        GraphElement CreateGroup(string Title, Vector2 localMousePosition)
        {
            Group group = new Group()
            {
                title = Title,
            };

            group.SetPosition(new Rect(localMousePosition, Vector2.zero));
            return group;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compactiblePorts = new List<Port>();
            Port startPortView = startPort;

            ports.ForEach((port) =>
            {
                Port portView = port;

                if (startPortView != portView && startPortView.node != portView.node && startPortView.direction != port.direction &&
            startPortView.portType == portView.portType)
                {
                    compactiblePorts.Add(port);
                }
            });

            return compactiblePorts;
        }

        public void LanguageReload()
        {
            foreach (BaseNode node in nodes)
            {
                node.ReloadLanguage();
            }
        }


        public EventNode CreateEventNode(Vector2 _pos)
        {
            EventNode tmp = new EventNode(_pos, editorWindow, this);
            tmp.name = "Event";

            return tmp;
        }
        public TimerChoiceNode CreateTimerChoiceNode(Vector2 _pos)
        {
            TimerChoiceNode tmp = new TimerChoiceNode(_pos, editorWindow, this);
            tmp.name = "TimerChoice";
            tmp.ReloadLanguage();

            return tmp;
        }
        /*

        // Copu Pasty
        private string CutCopyOperation(IEnumerable<GraphElement> elements)
        {
            int savedContent = 0;
            string clipboard = "";

            List<NodeJsonData> nodeJsonDataList = new List<NodeJsonData>();

            foreach (var node in elements)
            {
                // Tworzymy nowy NodeJsonData dla ka�dego w�z�a
                NodeJsonData nodeJsonData = new NodeJsonData
                {
                    Type = node.GetType().Name, // Ustawiamy nazw� typu w�z�a
                    Data = JsonUtility.ToJson(node) // Serializujemy dane w�z�a do JSON
                };

                nodeJsonDataList.Add(nodeJsonData);
                savedContent++;

                Debug.Log($"Przetworzono w�ze�: {nodeJsonData.Type}, Dane: {nodeJsonData.Data}");
            }

            // Konwersja listy NodeJsonData do jednego stringa JSON
            clipboard = JsonUtility.ToJson(new { Nodes = nodeJsonDataList.ToArray() }, true);

            Debug.Log($"Clipboard JS{clipboard[0]}ON: {clipboard} {clipboard.Length}");



            return clipboard;
        }

        private void PasteOperation(string operationName, string data)
        {
            string[] stringDatas = JsonHelper.FromJson<string>(data);
            int order = 0;
            List<object> jsonObjects = new List<object>();

            foreach (string stringData in stringDatas)
            {
                JsonHelper.JsonData jsonData = JsonUtility.FromJson<JsonHelper.JsonData>(stringData);
                var jsonObject = JsonUtility.FromJson(jsonData.Data, System.Type.GetType(jsonData.Type));
                jsonObjects.Add(jsonObject);
                //Debug.Log($"{order}. Data, {jsonData.Type}: {JsonUtility.ToJson(jsonObject)}");

                order++;
            }

            Debug.Log("Paste");
        }

        private bool CanPaste(string data)
        {
            return true;
        }*/
    }

}
