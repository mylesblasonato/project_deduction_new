using System.Collections.Generic;
using MeetAndTalk.GlobalValue;
using MeetAndTalk.Localization;
using UnityEditor;
using UnityEngine;

namespace MeetAndTalk
{
    [CreateAssetMenu(menuName = "Dialogue/New Dialogue Character")]
    public class DialogueCharacterSO : ScriptableObject
    {
        //[Header("Name Settings")]
        public List<LanguageGeneric<string>> characterName;
        public GlobalValueClass CustomizedName;
        public bool UseGlobalValue = false;
        public Color textColor = new Color(.8f, .8f, .8f, 1);

        //[Header("Portrait Settings")]
        public List<EmotionClass> Avatars = new List<EmotionClass>();
        //public PortraitPosition DefualtPosition;

        public string HexColor()
        {
            return $"#{ColorUtility.ToHtmlStringRGB(textColor)}";
        }


        public string GetName()
        {
            LocalizationManager _manager = (LocalizationManager)Resources.Load("Languages");
            if (_manager != null)
            {
                return characterName.Find(text => text.languageEnum == _manager.SelectedLang()).LanguageGenericType;
            }
            else
            {
                return "Can't find Localization Manager in scene";
            }
        }
    }
}

[System.Serializable]
public class EmotionClass
{
    public string EmotionName;
    public OrientationPortrait Portrait;
}

[System.Serializable]
public class OrientationPortrait
{
    public Sprite SpriteImage;
}


public enum PortraitPosition
{
    None = 0, 
    Primary = 1,        PrimaryDist= 2,
    Secoundary = 3,     SecoundaryDist = 4,
}
