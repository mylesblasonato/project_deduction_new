using System.Collections;
using System.Collections.Generic;
using MeetAndTalk.GlobalValue;
using MeetAndTalk.Localization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MeetAndTalk
{
    public class DialogueManager : DialogueGetData
    {
        public static DialogueManager Instance;

        public LocalizationManager localizationManager; 
        [Tooltip("Audio Source responsible for playing Dialogue Sounds")] public AudioSource dialogueAudioSource;
        [Tooltip("Audio Source responsible for playing Background Music")] public AudioSource musicAudioSource;
        [Tooltip("Default UI displayed until swapped")] public DialogueUIManager MainUI;

        public UnityEvent StartDialogueEvent;
        public UnityEvent EndDialogueEvent;

        // Private Value
        [HideInInspector] public DialogueUIManager dialogueUIManager;
        private List<Coroutine> activeCoroutines = new List<Coroutine>();

        private BaseNodeData currentDialogueNodeData;
        private BaseNodeData lastDialogueNodeData;
        public List<BaseNodeData> listOfOpenedNodes = new List<BaseNodeData>();
        public List<EventNodeData> listOfOpenedEvents = new List<EventNodeData>();

        private TimerChoiceNodeData tmpSavedTimeChoice;
        private AdvancedTimeChoiceNodeData tmpSavedAdvancedTimeChoice;
        private DialogueNodeData tmpSavedDialogue;
        private DialogueChoiceNodeData tmpSavedChoice;

        float Timer;

        private void Awake()
        {
            Instance = this;

            // Setup UI
            DialogueUIManager[] all = FindObjectsOfType<DialogueUIManager>();
            foreach(DialogueUIManager ui in all) { ui.gameObject.SetActive(false); }

            DialogueUIManager.Instance = MainUI;
            dialogueUIManager = DialogueUIManager.Instance;

            dialogueAudioSource = GetComponent<AudioSource>();

            // Music 
            musicAudioSource.clip = null;
            musicAudioSource.loop = true;
            musicAudioSource.Play();
        }

        private void Update()
        {
            Timer -= Time.deltaTime;
            if (Timer > 0) dialogueUIManager.TimerSlider.value = Timer;
        }

        /// <summary>
        /// Pozwala na zmiane aktualnego UI Dialogu
        /// </summary>
        /// <param name="UI"></param>
        public void ChangeUI(DialogueUIManager UI)
        {
            // Setup UI
            if (UI != null) DialogueUIManager.Instance = UI;
            else Debug.LogError("DialogueUIManager.UI Object jest Pusty!");
        }

        /// <summary>
        /// Pozwala na zmiane aktualnego UI Dialogu
        /// </summary>
        /// <param name="UI"></param>
        public void ChangeUI(string UI)
        {
            List<DialogueUIManager> dialogueElements = new List<DialogueUIManager>(FindObjectsOfType<DialogueUIManager>(true));
            ChangeUI(dialogueElements.Find(d => d.ID == UI));
        }

        /// <summary>
        /// Pozwala na przypisanie aktualnego dialogu
        /// </summary>
        /// <param name="dialogue"></param>
        public void SetupDialogue(DialogueContainerSO dialogue)
        {
            if (dialogue != null) dialogueContainer = dialogue;
            else Debug.LogError("DialogueContainerSO.dialogue Object jest Pusty!");
        }

        public void StartDialogue(DialogueContainerSO dialogue) { StartDialogue(dialogue, ""); }
        public void StartDialogue(string ID) { StartDialogue(null, ID); }
        public void StartDialogue() { StartDialogue(null, ""); }
        public void StartDialogue(DialogueContainerSO DialogueSO, string StartID)
        {

            // Reset Saved Nodes
            listOfOpenedNodes.Clear();
            listOfOpenedEvents.Clear();


            // Update Dialogue UI
            dialogueUIManager = DialogueUIManager.Instance;
            // Setup Dialogue (if not empty)
            if(DialogueSO != null) { SetupDialogue(DialogueSO); }
            // Error: No Setup Dialogue
            if (dialogueContainer == null) { Debug.LogError("Error: Dialogue Container SO is not assigned!"); }

            // Check ID
            if (dialogueContainer.StartNodeDatas.Count == 0) { Debug.LogError("Error: No Start Node in Dialogue Container!"); }

            BaseNodeData _start = null;
            if (StartID != "")
            {
                // IF FInd ID assign Data
                foreach (StartNodeData data in dialogueContainer.StartNodeDatas)
                {
                    if(data.startID == StartID) _start = data;
                }
            }
            if(_start == null)
            {
                _start = dialogueContainer.StartNodeDatas[Random.Range(0, dialogueContainer.StartNodeDatas.Count)];
            }

            // Pro Feature: Load Saved Dialogue
            string GUID = PlayerPrefs.GetString($"{dialogueContainer.name}_Progress");
            BaseNodeData _savedStart = null;
            bool ChangedFromSave = false;

            if(GUID != "" && dialogueContainer.AllowDialogueSave)
            {
                // Dialogue Is Ended
                if(GUID == "ENDED")
                {
                    // Ignore Dialogue
                    if (dialogueContainer.BlockingReopeningDialogue)
                    { 
                        return; 
                    }

                    // Normal Start (Start Node)
                    else { CheckNodeType(GetNextNode(_start)); }
                }
                // Dialogue is in Progress
                else
                {
                    _savedStart = GetNodeByGuid(GUID); 
                    ChangedFromSave = true;
                }
            }
            // Start Dialoguw
            if (ChangedFromSave)
            {
                CheckNodeType(_savedStart);
            }
            else
            {
                // w/o Load From Save
                CheckNodeType(GetNextNode(_start));
            }

            // Enable UI
            dialogueUIManager.dialogueCanvas.SetActive(true);
            StartDialogueEvent.Invoke();

        }

        public void CheckNodeType(BaseNodeData _baseNodeData)
        {
            // PlayerPrefs.SetString($"{dialogueContainer.name}_Progress", _baseNodeData.NodeGuid);
            switch (_baseNodeData)
            {
                case StartNodeData nodeData:
                    RunNode(nodeData);
                    break;
                case DialogueNodeData nodeData:
                    RunNode(nodeData);
                    break;
                case DialogueChoiceNodeData nodeData:
                    RunNode(nodeData);
                    break;
                case TimerChoiceNodeData nodeData:
                    RunNode(nodeData);
                    break;
                case EventNodeData nodeData:
                    RunNode(nodeData);
                    break;
                case EndNodeData nodeData:
                    RunNode(nodeData);
                    break;
                case RandomNodeData nodeData:
                    RunNode(nodeData);
                    break;
                case IfNodeData nodeData:
                    RunNode(nodeData);
                    break;
                case AdvancedIFNodeData nodeData:
                    RunNode(nodeData);
                    break;
                case AdvancedChoiceNodeData nodeData:
                    RunNode(nodeData);
                    break;
                case AdvancedTimeChoiceNodeData nodeData:
                    RunNode(nodeData);
                    break;
                case ResetSavedNodeData nodeData:
                    RunNode(nodeData);
                    break;
                case MusicNodeData nodeData:
                    RunNode(nodeData);
                    break;
                default:
                    Debug.LogError("!!! NIE PRAWID�OWY TYP DANYCH ZNALAZ� SI� W DIALOGUE CONTAINER !!!");
                    break;
            }
        }


        private void RunNode(StartNodeData _nodeData)
        {
            string GUID = _nodeData.NodeGuid;
            PlayerPrefs.SetString($"{dialogueContainer.name}_Progress", GUID);

            // Reset Audio
            dialogueAudioSource.Stop();

            CheckNodeType(GetNextNode(_nodeData));
        }
        private void RunNode(RandomNodeData _nodeData)
        {
            // Dialogue History
            // Doesn't Save Random Node!

            // Go TO Random Node
            CheckNodeType(GetNextNode(_nodeData));
        }
        private void RunNode(IfNodeData _nodeData)
        {
            // Dialogue History
            // Doesn't Save IF Node!

            string ValueName = _nodeData.ValueName;
            GlobalValueIFOperations Operations = _nodeData.Operations;
            string OperationValue = _nodeData.OperationValue;

            GlobalValueManager manager = Resources.Load<GlobalValueManager>("GlobalValue");
            manager.LoadFile();

            //Debug.Log("XXXX" + _nodeData.TrueGUID + "XXXX");
            CheckNodeType(GetNodeByGuid(manager.IfTrue(ValueName, Operations, OperationValue) ? _nodeData.TrueGUID : _nodeData.FalseGUID));
        }
        private void RunNode(AdvancedIFNodeData _nodeData)
        {
            // Dialogue History
            // Doesn't Save Advanced IF Node!

            // GEt List of Conditions
            List<ConditionInputPort> conditions = _nodeData.Conditions;

            // Load GlobalValues
            GlobalValueManager manager = Resources.Load<GlobalValueManager>("GlobalValue");
            manager.LoadFile();

            if (_nodeData.TrueWhenAllCondition)
            {
                // Get Defuatl Result           [All True]
                bool conditionsResult = true;
                foreach (ConditionInputPort condition in conditions)
                {
                    if (!manager.IfTrue(condition.Operation))
                    { conditionsResult = false;  }
                }
                CheckNodeType(GetNodeByGuid(conditionsResult ? _nodeData.TrueGUID : _nodeData.FalseGUID));
            }
            else
            {
                // Get Defuatl Result           [At least one]
                bool conditionsResult = false;
                foreach (ConditionInputPort condition in conditions)
                {
                    if (manager.IfTrue(condition.Operation))
                    { conditionsResult = true; }
                }
                CheckNodeType(GetNodeByGuid(conditionsResult ? _nodeData.TrueGUID : _nodeData.FalseGUID));
            }
        }
        private void RunNode(DialogueNodeData _nodeData)
        {
            // DIalogue Progress
            string GUID = _nodeData.NodeGuid;
            PlayerPrefs.SetString($"{dialogueContainer.name}_Progress", GUID);

            // Dialogue History
            listOfOpenedNodes.Add(_nodeData);
            lastDialogueNodeData = currentDialogueNodeData;
            currentDialogueNodeData = _nodeData;

            // Display Dialogue Text
            dialogueUIManager.DisplayText(_nodeData.Character, _nodeData.TextType.Find(text => text.languageEnum == localizationManager.SelectedLang()).LanguageGenericType);
            dialogueUIManager.SkipButton.SetActive(true);

            // Display Portraits 
            dialogueUIManager.SetupPortraits(_nodeData.Character, _nodeData.PortraitPosition, _nodeData.Emotion,
                _nodeData.SecoundCharacter, _nodeData.SecoundPortraitPosition, _nodeData.SecoundEmotion);

            // Doesn't generate buttons
            MakeButtons(new List<DialogueNodePort>());

            // Play Audio
            if(_nodeData.AudioClips.Find(clip => clip.languageEnum == localizationManager.SelectedLang()).LanguageGenericType != null) dialogueAudioSource.PlayOneShot(_nodeData.AudioClips.Find(clip => clip.languageEnum == localizationManager.SelectedLang()).LanguageGenericType);

            // Select
            tmpSavedDialogue = _nodeData;

            // Stop All Coroutines
            StopAllTrackedCoroutines();

            // Gen to next Node
            IEnumerator tmp() { yield return new WaitForSeconds(_nodeData.Duration); DialogueNode_NextNode(); }
            if(_nodeData.Duration != 0) StartTrackedCoroutine(tmp());;
        }
        private void RunNode(AdvancedChoiceNodeData _nodeData)
        {
            // Dialogue History
            lastDialogueNodeData = currentDialogueNodeData;
            currentDialogueNodeData = _nodeData;

            string GUID = _nodeData.NodeGuid;
            PlayerPrefs.SetString($"{dialogueContainer.name}_Progress", GUID);

            //MakeButtons(new List<DialogueNodePort>());

            StopAllTrackedCoroutines();

            // Generate Buttons
            MakeAdvancedButtons(_nodeData, _nodeData.DialogueNodePorts, false);
            dialogueUIManager.SkipButton.SetActive(false);
        }
        private void RunNode(AdvancedTimeChoiceNodeData _nodeData)
        {
            // Dialogue History
            lastDialogueNodeData = currentDialogueNodeData;
            currentDialogueNodeData = _nodeData;

            string GUID = _nodeData.NodeGuid;
            PlayerPrefs.SetString($"{dialogueContainer.name}_Progress", GUID);

            //MakeButtons(new List<DialogueNodePort>());

            StopAllTrackedCoroutines();

            tmpSavedAdvancedTimeChoice = _nodeData;

            // Generate Buttons
            MakeAdvancedButtons(_nodeData, _nodeData.DialogueNodePorts, _nodeData.time, _nodeData.ShowTimer);
        }
        private void RunNode(DialogueChoiceNodeData _nodeData)
        {

            // Dialogue History
            listOfOpenedNodes.Add(_nodeData);
            lastDialogueNodeData = currentDialogueNodeData;
            currentDialogueNodeData = _nodeData;

            string GUID = _nodeData.NodeGuid;
            PlayerPrefs.SetString($"{dialogueContainer.name}_Progress", GUID);

            GlobalValueManager manager = Resources.Load<GlobalValueManager>("GlobalValue");
            manager.LoadFile();

            // Gloval Value Multiline
            if (dialogueUIManager.showSeparateName && dialogueUIManager.nameTextBox != null && _nodeData.Character != null && _nodeData.Character.UseGlobalValue) { dialogueUIManager.ResetText(""); dialogueUIManager.SetSeparateName($"<color={_nodeData.Character.HexColor()}>{manager.Get<string>(GlobalValueType.String, _nodeData.Character.CustomizedName.ValueName)}</color>"); }
            // Normal Multiline
            else if (dialogueUIManager.showSeparateName && dialogueUIManager.nameTextBox != null) { dialogueUIManager.ResetText(""); dialogueUIManager.SetSeparateName($"<color={_nodeData.Character.HexColor()}>{_nodeData.Character.characterName.Find(text => text.languageEnum == localizationManager.SelectedLang()).LanguageGenericType}</color>"); }
            // Global Value Inline
            else if (_nodeData.Character != null && _nodeData.Character.UseGlobalValue) dialogueUIManager.ResetText($"<color={_nodeData.Character.HexColor()}>{manager.Get<string>(GlobalValueType.String, _nodeData.Character.CustomizedName.ValueName)}: </color>");
            // Normal Inline
            else if (_nodeData.Character != null) dialogueUIManager.ResetText($"<color={_nodeData.Character.HexColor()}>{_nodeData.Character.characterName.Find(text => text.languageEnum == localizationManager.SelectedLang()).LanguageGenericType}: </color>");
            // Last Change
            else dialogueUIManager.ResetText("");

            dialogueUIManager.SetFullText($"{_nodeData.TextType.Find(text => text.languageEnum == localizationManager.SelectedLang()).LanguageGenericType}");

            // New Character Avatar
            //if (_nodeData.AvatarPos == AvatarPosition.Left) dialogueUIManager.UpdateAvatars(_nodeData.Character, null, _nodeData.AvatarType);
            //else if (_nodeData.AvatarPos == AvatarPosition.Right) dialogueUIManager.UpdateAvatars(null, _nodeData.Character, _nodeData.AvatarType);
            //else dialogueUIManager.UpdateAvatars(null, null, _nodeData.AvatarType);

            dialogueUIManager.SkipButton.SetActive(true);
            MakeButtons(new List<DialogueNodePort>());

            tmpSavedChoice = _nodeData;

            StopAllTrackedCoroutines();

            IEnumerator tmp() { yield return new WaitForSeconds(_nodeData.Duration); ChoiceNode_GenerateChoice(); }
            StartTrackedCoroutine(tmp()); ;

            if (_nodeData.AudioClips.Find(clip => clip.languageEnum == localizationManager.SelectedLang()).LanguageGenericType != null) dialogueAudioSource.PlayOneShot(_nodeData.AudioClips.Find(clip => clip.languageEnum == localizationManager.SelectedLang()).LanguageGenericType);
        }
        private void RunNode(EventNodeData _nodeData)
        {
            if (!listOfOpenedEvents.Contains(_nodeData))
            {
                // Invoke All Event in Event Node
                foreach (var item in _nodeData.EventScriptableObjects)
                {
                    if (item.DialogueEventSO != null)
                    {
                        item.DialogueEventSO.RunEvent();
                    }
                }
            }

            // Dialogue History
            listOfOpenedEvents.Add(_nodeData);

            CheckNodeType(GetNextNode(_nodeData));
        }
        private void RunNode(EndNodeData _nodeData)
        {
            PlayerPrefs.SetString($"{dialogueContainer.name}_Progress", "ENDED");

            switch (_nodeData.EndNodeType)
            {
                case EndNodeType.End:
                    dialogueUIManager.dialogueCanvas.SetActive(false);
                    EndDialogueEvent.Invoke();
                    break;
                case EndNodeType.ReturnToStart:
                    CheckNodeType(GetNextNode(dialogueContainer.StartNodeDatas[Random.Range(0,dialogueContainer.StartNodeDatas.Count)]));
                    break;
                case EndNodeType.NextDialogue:
                    StartDialogue(_nodeData.Dialogue, "");
                    break;
                default:
                    break;
            }
        }
        private void RunNode(TimerChoiceNodeData _nodeData)
        {

            // Dialogue History
            listOfOpenedNodes.Add(_nodeData);
            lastDialogueNodeData = currentDialogueNodeData;
            currentDialogueNodeData = _nodeData;

            string GUID = _nodeData.NodeGuid;
            PlayerPrefs.SetString($"{dialogueContainer.name}_Progress", GUID);

            GlobalValueManager manager = Resources.Load<GlobalValueManager>("GlobalValue");
            manager.LoadFile();

            // Gloval Value Multiline
            if (dialogueUIManager.showSeparateName && dialogueUIManager.nameTextBox != null && _nodeData.Character != null && _nodeData.Character.UseGlobalValue) { dialogueUIManager.ResetText(""); dialogueUIManager.SetSeparateName($"<color={_nodeData.Character.HexColor()}>{manager.Get<string>(GlobalValueType.String, _nodeData.Character.CustomizedName.ValueName)}</color>"); }
            // Normal Multiline
            else if (dialogueUIManager.showSeparateName && dialogueUIManager.nameTextBox != null) { dialogueUIManager.ResetText(""); dialogueUIManager.SetSeparateName($"<color={_nodeData.Character.HexColor()}>{_nodeData.Character.characterName.Find(text => text.languageEnum == localizationManager.SelectedLang()).LanguageGenericType}</color>"); }
            // Global Value Inline
            else if (_nodeData.Character != null && _nodeData.Character.UseGlobalValue) dialogueUIManager.ResetText($"<color={_nodeData.Character.HexColor()}>{manager.Get<string>(GlobalValueType.String, _nodeData.Character.CustomizedName.ValueName)}: </color>");
            // Normal Inline
            else if (_nodeData.Character != null) dialogueUIManager.ResetText($"<color={_nodeData.Character.HexColor()}>{_nodeData.Character.characterName.Find(text => text.languageEnum == localizationManager.SelectedLang()).LanguageGenericType}: </color>");
            // Last Change
            else dialogueUIManager.ResetText("");

            dialogueUIManager.SetFullText($"{_nodeData.TextType.Find(text => text.languageEnum == localizationManager.SelectedLang()).LanguageGenericType}");

            // New Character Avatar
            //if (_nodeData.AvatarPos == AvatarPosition.Left) dialogueUIManager.UpdateAvatars(_nodeData.Character, null, _nodeData.AvatarType);
            //else if (_nodeData.AvatarPos == AvatarPosition.Right) dialogueUIManager.UpdateAvatars(null, _nodeData.Character, _nodeData.AvatarType);
            //else dialogueUIManager.UpdateAvatars(null, null, _nodeData.AvatarType);

            dialogueUIManager.SkipButton.SetActive(true);
            MakeButtons(new List<DialogueNodePort>());

            tmpSavedTimeChoice = _nodeData;

StopAllTrackedCoroutines();

            IEnumerator tmp() { yield return new WaitForSecondsRealtime(_nodeData.Duration); TimerNode_GenerateChoice(); }
            StartTrackedCoroutine(tmp());;

            if (_nodeData.AudioClips.Find(clip => clip.languageEnum == localizationManager.SelectedLang()).LanguageGenericType != null) dialogueAudioSource.PlayOneShot(_nodeData.AudioClips.Find(clip => clip.languageEnum == localizationManager.SelectedLang()).LanguageGenericType);

        }
        private void RunNode(ResetSavedNodeData _nodeData)
        {
            // Dialogue History
            // Doesn't Save Random Node!

            StopAllTrackedCoroutines();
            listOfOpenedNodes.Clear();
            CheckNodeType(GetNextNode(_nodeData));
        }
        private void RunNode(MusicNodeData _nodeData)
        {
            // Dialogue History
            // Doesn't Save Random Node!

            StopAllTrackedCoroutines();

            StartCoroutine(Crossfade(_nodeData.AudioClips.Find(clip => clip.languageEnum == localizationManager.SelectedLang()).LanguageGenericType, _nodeData.SwitchTime));

            //listOfOpenedNodes.Clear();
            CheckNodeType(GetNextNode(_nodeData));
        }






        private void MakeButtons(List<DialogueNodePort> _nodePorts)
        {
            List<string> texts = new List<string>();
            List<UnityAction> unityActions = new List<UnityAction>();
            List<AdvancedChoiceType> choiceTypes = new List<AdvancedChoiceType>();

            foreach (DialogueNodePort nodePort in _nodePorts)
            {
                texts.Add(nodePort.TextLanguage.Find(text => text.languageEnum == localizationManager.SelectedLang()).LanguageGenericType);
                UnityAction tempAction = null;
                tempAction += () =>
                {
                    if(dialogueContainer.ResetSavedNodeOnChoice) listOfOpenedNodes.Clear();
                    CheckNodeType(GetNodeByGuid(nodePort.InputGuid));
                };
                choiceTypes.Add(AdvancedChoiceType.Normal);
                unityActions.Add(tempAction);
            }

            dialogueUIManager.SetButtons(texts, unityActions, choiceTypes, false);
        }
        private void MakeTimerButtons(List<DialogueNodePort> _nodePorts, float ShowDuration, float timer)
        {
            List<string> texts = new List<string>();
            List<UnityAction> unityActions = new List<UnityAction>();
            List<AdvancedChoiceType> choiceTypes = new List<AdvancedChoiceType>();

            IEnumerator tmp() { yield return new WaitForSeconds(timer); TimerNode_NextNode(); }
            StartTrackedCoroutine(tmp());;

            foreach (DialogueNodePort nodePort in _nodePorts)
            {
                if (nodePort != _nodePorts[0])
                {
                    texts.Add(nodePort.TextLanguage.Find(text => text.languageEnum == localizationManager.SelectedLang()).LanguageGenericType);
                    UnityAction tempAction = null;
                    tempAction += () =>
                    {
                        if (dialogueContainer.ResetSavedNodeOnChoice) listOfOpenedNodes.Clear();
                        StopAllTrackedCoroutines();
                        CheckNodeType(GetNodeByGuid(nodePort.InputGuid));
                    };
                    choiceTypes.Add(AdvancedChoiceType.Normal);
                    unityActions.Add(tempAction);
                }
            }

            dialogueUIManager.SetButtons(texts, unityActions, choiceTypes, tmpSavedTimeChoice.ShowTimer);
            dialogueUIManager.TimerSlider.maxValue = timer; Timer = timer;
        }

        private void MakeAdvancedButtons(BaseNodeData _data, List<AdvancedDialogueNodePort> nodePorts, bool ShowTimer) { MakeAdvancedButtons(_data, nodePorts, 0, ShowTimer); }
        private void MakeAdvancedButtons(BaseNodeData _data, List<AdvancedDialogueNodePort> nodePorts, float timer, bool ShowTimer) 
        {
            // Listy
            List<string> texts = new List<string>();
            List<UnityAction> unityActions = new List<UnityAction>();
            List<AdvancedChoiceType> choices = new List<AdvancedChoiceType>();

            // Global Value Manager
            GlobalValueManager manager = Resources.Load<GlobalValueManager>("GlobalValue");

            // IF ADVANCED TIMER CHOICE NODE
            if(timer!=0)
            {
                IEnumerator tmp() { yield return new WaitForSeconds(timer); AdvancedTimerNode_NextNode(); }
                StartTrackedCoroutine(tmp()); ;
            }

            foreach (AdvancedDialogueNodePort nodePort in nodePorts)
            {
                if (manager.IfTrue(nodePort.Condition))
                {
                    // Get Text
                    texts.Add(nodePort.TextLanguage.Find(text => text.languageEnum == localizationManager.SelectedLang()).LanguageGenericType);
                    // Get Action
                    UnityAction tempAction = null;
                    tempAction += () =>
                    {
                        if (dialogueContainer.ResetSavedNodeOnChoice) listOfOpenedNodes.Clear();
                        CheckNodeType(GetNodeByGuid(nodePort.InputGuid));
                    };
                    unityActions.Add(tempAction);
                    // Get Type
                    choices.Add(nodePort.ChoiceType);
                }
            }

            dialogueUIManager.SetButtons(texts, unityActions, choices, true);
            // IF ADVANCED TIMER CHOICE NODE
            if (timer != 0) dialogueUIManager.TimerSlider.maxValue = timer; Timer = timer;

            if (texts.Count == 0)
            {
                if (_data is AdvancedChoiceNodeData) { CheckNodeType(GetNextNode((AdvancedChoiceNodeData)_data)); }
                if (_data is AdvancedTimeChoiceNodeData) { CheckNodeType(GetNextNode((AdvancedTimeChoiceNodeData)_data)); }
            }
        }




        void DialogueNode_NextNode() { CheckNodeType(GetNextNode(tmpSavedDialogue)); }
        void ChoiceNode_GenerateChoice() { MakeButtons(tmpSavedChoice.DialogueNodePorts);
            dialogueUIManager.SkipButton.SetActive(false);
        }
        void TimerNode_GenerateChoice() { MakeTimerButtons(tmpSavedTimeChoice.DialogueNodePorts, tmpSavedTimeChoice.Duration, tmpSavedTimeChoice.time);
            dialogueUIManager.SkipButton.SetActive(false);
        }
        void TimerNode_NextNode() { CheckNodeType(GetNextNode(tmpSavedTimeChoice)); }
        void AdvancedTimerNode_NextNode() { CheckNodeType(GetNextNode(tmpSavedAdvancedTimeChoice)); }



        #region Improve Coroutine
        private void StopAllTrackedCoroutines()
        {
            foreach (var coroutine in activeCoroutines)
            {
                if (coroutine != null)
                {
                    StopCoroutine(coroutine);
                }
            }
            activeCoroutines.Clear();
        }

        private Coroutine StartTrackedCoroutine(IEnumerator coroutine)
        {
            Coroutine newCoroutine = StartCoroutine(coroutine);
            activeCoroutines.Add(newCoroutine);
            return newCoroutine;
        }
        #endregion


        private System.Collections.IEnumerator Crossfade(AudioClip nextClip, float crossfadeDuration = 4f)
        {
            if (musicAudioSource.clip != nextClip)
            {
                float time = 0f;

                // Zmniejsz g�o�no�� do zera
                while (time < crossfadeDuration / 2)
                {
                    if (musicAudioSource.clip == null) { time = 999; }
                    time += Time.deltaTime;
                    musicAudioSource.volume = Mathf.Lerp(1f, 0f, time / (crossfadeDuration / 2));
                    yield return null;
                }

                // Podmie� klip i odtw�rz nowy
                musicAudioSource.clip = nextClip;
                musicAudioSource.Play();

                time = 0f;

                // Zwi�ksz g�o�no�� do maksymalnej
                while (time < crossfadeDuration / 2)
                {
                    if (musicAudioSource.clip == null) { time = 999; }
                    time += Time.deltaTime;
                    musicAudioSource.volume = Mathf.Lerp(0f, 1f, time / (crossfadeDuration / 2));
                    yield return null;
                }

                // Upewnij si�, �e g�o�no�� jest ustawiona na maksymaln�
                musicAudioSource.volume = 1f;
            }
        }



        public void SkipDialogue()
        {

            // Reset Audio
            dialogueAudioSource.Stop();

            StopAllTrackedCoroutines();

            switch (currentDialogueNodeData)
            {
                case DialogueNodeData nodeData:
                    DialogueNode_NextNode();
                    break;
                case DialogueChoiceNodeData nodeData:
                    ChoiceNode_GenerateChoice();
                    break;
                case TimerChoiceNodeData nodeData:
                    TimerNode_GenerateChoice();
                    break;
                default:
                    break;
            }
        }
        public void ForceEndDialog()
        {            
            // Reset Audio
            dialogueAudioSource.Stop();

            dialogueUIManager.dialogueCanvas.SetActive(false);
            EndDialogueEvent.Invoke();

            StopAllTrackedCoroutines();

            // Reset Audio
            dialogueAudioSource.Stop();
        }

        public void GoToPreviousNode()
        {
            if (listOfOpenedNodes.Count > 1)
            {
                // 
                StopAllTrackedCoroutines();

                // Reset Audio
                dialogueAudioSource.Stop();

                // 
                listOfOpenedNodes.RemoveAt(listOfOpenedNodes.Count - 1);
                string GUID = listOfOpenedNodes[listOfOpenedNodes.Count - 1].NodeGuid;
                listOfOpenedNodes.RemoveAt(listOfOpenedNodes.Count - 1);
                CheckNodeType(GetNodeByGuid(GUID));
            }
        }



    }
}
