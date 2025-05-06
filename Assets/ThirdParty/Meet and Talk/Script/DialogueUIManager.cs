using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MeetAndTalk.GlobalValue;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MeetAndTalk
{
    public class DialogueUIManager : MonoBehaviour
    {
        public static DialogueUIManager Instance;
        public string ID;

        [Header("Type Writing")]                    // Premium Feature
        public bool EnableTypeWriting = false;      // Premium Feature
        public float typingSpeed = 50.0f;           // Premium Feature

        [Header("Dialogue UI")]
        public bool showSeparateName = false;
        public bool clearNameColor = false;         // Premium Feature
        public TextMeshProUGUI nameTextBox;
        public TextMeshProUGUI textBox;
        [Space()]
        public GameObject dialogueCanvas;
        public Slider TimerSlider;
        public GameObject SkipButton;
        public GameObject GoBackButton;

        [Header("Dynamic Dialogue UI")]
        public List<ChoiceTypeButton> BtnPrefabs = new List<ChoiceTypeButton>();
        public GameObject ButtonContainer;

        public List<PortraitUIClass> Portraits = new List<PortraitUIClass>();
        public List<GameObject> HideIfAllAvatarEmpty = new List<GameObject>();          // Premium Feature
        public List<GameObject> HideIfNoCharacter = new List<GameObject>();             // Premium Feature
        public List<GameObject> HideIfChoiceEmpty = new List<GameObject>();             // Premium Feature
        public UnityEvent StartDialogueEvent;
        public UnityEvent EndDialogueEvent;

        [HideInInspector] public string prefixText;
        [HideInInspector] public string fullText;
        private string currentText = "";
        private int characterIndex = 0;
        private float lastTypingTime;

        private List<Button> buttons = new List<Button>();
        private List<TextMeshProUGUI> buttonsTexts = new List<TextMeshProUGUI>();



        private void Awake()
        {
            if (Instance == null) Instance = this;

            // Premium Feature: Type-Writing
            if(EnableTypeWriting) lastTypingTime = Time.time;
        }

        private void OnValidate()
        {
            foreach (AdvancedChoiceType type in Enum.GetValues(typeof(AdvancedChoiceType)))
            {
                bool exists = BtnPrefabs.Exists(btn => btn.Type == type);
                if (!exists)
                {
                    ChoiceTypeButton btn = new ChoiceTypeButton();
                    btn.Type = type;
                    BtnPrefabs.Add(btn);
                }
            }

            foreach (PortraitPosition type in Enum.GetValues(typeof(PortraitPosition)))
            {
                bool exists = Portraits.Exists(btn => btn.Position == type);
                if (!exists && type != PortraitPosition.None)
                {
                    PortraitUIClass btn = new PortraitUIClass();
                    btn.Position = type;
                    Portraits.Add(btn);
                }
            }
        }

        private void Update()
        {
            // Premium Feature: Type-Writing
            if (characterIndex < fullText.Length && EnableTypeWriting)
            {
                if (Time.time - lastTypingTime > 1.0f / typingSpeed)
                {
                    if (fullText[characterIndex].ToString() == "<")
                    {
                        while(fullText[characterIndex].ToString() != ">")
                        {
                            currentText += fullText[characterIndex];
                            characterIndex++;
                        }
                        currentText += fullText[characterIndex];
                        characterIndex++;
                        textBox.text = currentText;
                    }
                    else { currentText += fullText[characterIndex]; characterIndex++; textBox.text = currentText; }

                    lastTypingTime = Time.time;
                }
            }
            else
            {
                textBox.text = prefixText+fullText;
            }

            if (DialogueManager.Instance.listOfOpenedNodes.Count > 1) { GoBackButton.SetActive(true); }else { GoBackButton.SetActive(false); }
        }

        /// <summary>
        /// Funkcja 
        /// </summary>
        /// <param name="Character"></param>
        /// <param name="DialogueText"></param>
        public void DisplayText(DialogueCharacterSO Character, string DialogueText)
        {
            // Get Actual Global Values 
            GlobalValueManager manager = Resources.Load<GlobalValueManager>("GlobalValue");
            manager.LoadFile();

            // Multiline
            if (showSeparateName && nameTextBox != null)
            {
                // Reset Dialogue Text
                ResetText("");

                // Use Global Value [Dynamic] Name
                if (Character != null && Character.UseGlobalValue) { SetSeparateName($"<color={Character.HexColor()}>{manager.Get<string>(GlobalValueType.String, Character.CustomizedName.ValueName)}: </color>"); }
                
                // Use Defualt Character Name
                else if (Character != null) { SetSeparateName($"<color={Character.HexColor()}>{Character.characterName.Find(text => text.languageEnum == DialogueManager.Instance.localizationManager.SelectedLang()).LanguageGenericType}: </color>"); }
                
                // Don't Show Character Name
                else { SetSeparateName(""); }
            }

            // Single
            else
            {
                // Use Global Value [Dynamic] Name
                if (Character != null && Character.UseGlobalValue) { ResetText($"<color={Character.HexColor()}>{manager.Get<string>(GlobalValueType.String, Character.CustomizedName.ValueName)}: </color>"); }

                // Use Defualt Character Name
                else if (Character != null) { ResetText($"<color={Character.HexColor()}>{Character.characterName.Find(text => text.languageEnum == DialogueManager.Instance.localizationManager.SelectedLang()).LanguageGenericType}: </color>"); }

                // Don't Show Character Name
                else { ResetText(""); }
            }

            SetFullText(DialogueText);

            //
            if (nameTextBox==null || nameTextBox.text == "") { foreach (GameObject obj in HideIfAllAvatarEmpty) obj.SetActive(true); }
            else { foreach (GameObject obj in HideIfAllAvatarEmpty) obj.SetActive(false); }
        }

        public void SetupPortraits(DialogueCharacterSO Character, PortraitPosition Position, string Emotion,
            DialogueCharacterSO SecoundCharacter, PortraitPosition SecoundPosition, string SecoundEmotion)
        {
            // Reset All
            foreach (PortraitUIClass portrait in Portraits)
            {
                foreach (GameObject obj in HideIfAllAvatarEmpty) { obj.SetActive(false); }
                portrait.PortraitImage.SetActive(false);
            }
            bool any = false;

            // Main Character
            if (Character != null)
            {
                if (Position != PortraitPosition.None)
                {
                    PortraitUIClass ui = Portraits.Find(d => d.Position == Position);
                    if (ui != null)
                    {
                        // Setup Portrait
                        Sprite tmp = Character.Avatars.Find(d => Emotion == d.EmotionName)?.Portrait.SpriteImage;
                        if (tmp != null)
                        {
                            ui.PortraitImage.SetActive(true);
                            ui.PortraitImage.GetComponent<Image>().sprite = tmp;
                            // Enable
                            foreach (GameObject obj in ui.HideIfPortraitEmpty) obj.SetActive(true);
                        }
                        else
                        {
                            ui.PortraitImage.SetActive(false);
                        }
                    }
                    any = true;
                }
            }

            // Alt Character
            if (SecoundCharacter != null)
            {
                if (SecoundPosition != PortraitPosition.None)
                {
                    PortraitUIClass ui = Portraits.Find(d => d.Position == SecoundPosition);
                    if (ui != null)
                    {
                        Sprite tmp = SecoundCharacter.Avatars.Find(d => SecoundEmotion == d.EmotionName)?.Portrait.SpriteImage;
                        if (tmp != null)
                        {
                            // Setup Portrait
                            ui.PortraitImage.SetActive(true);
                            ui.PortraitImage.GetComponent<Image>().sprite = SecoundCharacter.Avatars.Find(d => SecoundEmotion == d.EmotionName).Portrait.SpriteImage;

                            // Enable
                            foreach (GameObject obj in ui.HideIfPortraitEmpty) obj.SetActive(true);
                        }
                        else
                        {
                            ui.PortraitImage.SetActive(false);
                        }
                    }
                    any = true;
                }
            }

            // Show / Hide Elements
            if (any) { foreach (GameObject obj in HideIfAllAvatarEmpty) obj.SetActive(true);  }
            else { foreach (GameObject obj in HideIfAllAvatarEmpty) obj.SetActive(false); }
        }

        public void ResetText(string prefix)
        {
            // Premium Feature: Clean Name
            if (clearNameColor) prefix = RemoveRichTextTags(prefix);

            currentText = prefix;
            prefixText = prefix;
            characterIndex = 0;
        }

        public void SetSeparateName(string name)
        {
            // Premium Feature: Clean Name
            if (clearNameColor) name = RemoveRichTextTags(name);

            nameTextBox.text = name;
        }

        public void SetFullText(string text)
        {
            string newText = text;


            Regex regex = new Regex(@"\{(.*?)\}");
            MatchEvaluator matchEvaluator = new MatchEvaluator(match =>
            {
                string OldText = match.Groups[1].Value;
                return ChangeReplaceableText(OldText); 
            });

            newText = regex.Replace(newText, matchEvaluator);

            fullText = newText;
        }

        public void SetButtons(List<string> _texts, List<UnityAction> _unityActions, List<AdvancedChoiceType> _type, bool showTimer)
        {
            // Hide If Choice Empty
            foreach (GameObject obj in HideIfChoiceEmpty)
            {
                if (obj != null && _texts.Count > 0) { obj.SetActive(true); }
                else if (obj != null) { obj.SetActive(false); }
            }

            foreach (Transform child in ButtonContainer.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            for (int i = 0; i < _texts.Count; i++)
            {
                GameObject btn = null;

                //Debug.Log(_type[i]);

                ChoiceTypeButton tmpButton = BtnPrefabs.FirstOrDefault(d => d.Type == _type[i]);
                GameObject tmp = tmpButton != null ? tmpButton.BtnPrefab : null;

                btn = Instantiate(tmp, ButtonContainer.transform);
                btn.transform.Find("Text").GetComponent<TMP_Text>().text = _texts[i];
                btn.gameObject.SetActive(true);
                btn.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
                btn.GetComponent<Button>().onClick.AddListener(_unityActions[i]);


            }

            TimerSlider.gameObject.SetActive(showTimer);
        }

        string ChangeReplaceableText(string text)
        {
            GlobalValueManager manager = Resources.Load<GlobalValueManager>("GlobalValue");
            manager.LoadFile();

            string TextToReplace = "[Error Value]";
            /* Global Value */
            for (int i = 0; i < manager.IntValues.Count; i++) { if (text == manager.IntValues[i].ValueName) TextToReplace = manager.IntValues[i].Value.ToString(); } 
            for (int i = 0; i < manager.FloatValues.Count; i++) { if (text == manager.FloatValues[i].ValueName) TextToReplace = manager.FloatValues[i].Value.ToString(); } 
            for (int i = 0; i < manager.BoolValues.Count; i++) { if (text == manager.BoolValues[i].ValueName) TextToReplace = manager.BoolValues[i].Value.ToString(); } 
            for (int i = 0; i < manager.StringValues.Count; i++) { if (text == manager.StringValues[i].ValueName) TextToReplace = manager.StringValues[i].Value; }

            //
            if(text.Contains(",")) 
            {
                string[] tmp = text.Split(',');
                for (int i = 0; i < manager.IntValues.Count; i++) { if (tmp[0] == manager.IntValues[i].ValueName) TextToReplace = Mathf.Abs(manager.IntValues[i].Value - (int)System.Convert.ChangeType(tmp[1], typeof(int))).ToString(); }
                for (int i = 0; i < manager.FloatValues.Count; i++) { if (tmp[0] == manager.FloatValues[i].ValueName) TextToReplace = Mathf.Abs(manager.FloatValues[i].Value - (int)System.Convert.ChangeType(tmp[1], typeof(int))).ToString(); }
            }

            return TextToReplace;
        }

        string RemoveRichTextTags(string input)
        {
            return System.Text.RegularExpressions.Regex.Replace(input, "<.*?>", string.Empty);
        }


    }
}
