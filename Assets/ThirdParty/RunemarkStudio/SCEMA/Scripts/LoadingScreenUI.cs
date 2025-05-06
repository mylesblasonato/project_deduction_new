namespace Runemark.SCEMA
{
    using UnityEngine;
    using UnityEngine.UI;
    using Runemark.Common;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    public class LoadingScreenUI : WindowBaseUI
    {
        public bool showTotalProgress;

        public enum CloseCondition { Automatically, OnKeyPressed }
        public CloseCondition closeCondition;
        public KeyCode closeKey;

        public Hints hintDatabase;
        public string hintPrefix = "Hint: ";
        
        [System.Serializable]
        public class UIElements
        {
            public Text locationName;
            public Image splashscreen;
            public Text hintLabel;

            [Header("Loading Progress")]
            public Slider loadingBar;
            public Text loadingProgress;
            public Text loadingDescription;
            public Text loadingIndicator;

            public void UpdateProgress(string description, float percentage)
            {
                Set(loadingBar, percentage);
                Set(loadingProgress, percentage.ToString("##%"));
                Set(loadingDescription, description);
            }

            public void Set(Text uiElement, string value)
            {
                if(uiElement != null)
                    uiElement.text = value;
            }
            public void Set(Image uiElement, Sprite value)
            {
                if (uiElement != null)
                    uiElement.sprite = value;
            }
            public void Set(Slider uiElement, float value)
            {
                if (uiElement != null)
                    uiElement.value = value;
            }
        }
        public UIElements uiElements = new UIElements();

        SCEMA scema;

        bool loading;
        bool waitingForClose = false;
       
        void Update()
        {
            if (isOpen)
            {              
                if (waitingForClose && Input.GetKey(closeKey))
                {
                    waitingForClose = false;
                    Close();
                }
                else
                {
                    float progress = (showTotalProgress) ? scema.totalProgress : scema.currentProcessProgress;
                    uiElements.UpdateProgress(scema.currentProcessDescription, progress);
                }
            }
        }

        public void Open(string locationName, Sprite splashscreen)
        {
            scema = SCEMA.Instance;

            uiElements.Set(uiElements.loadingIndicator, "LOADING");
                    
            Open();

            uiElements.Set(uiElements.locationName, locationName);
            uiElements.Set(uiElements.splashscreen, splashscreen);
            uiElements.Set(uiElements.hintLabel, hintPrefix + hintDatabase.RandomHint());

            uiElements.UpdateProgress("", 0);
        }

        public void OnLoadingFinished() 
        {
            if (closeCondition == CloseCondition.Automatically) Close();
            else
            {
                waitingForClose = true;
                uiElements.Set(uiElements.loadingIndicator, "PRESS " + closeKey.ToString().ToUpper() +"!"); 
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(LoadingScreenUI))]
    public class LocationLoaderUIInspector : WindowBaseUIInspector
    {
        LoadingScreenUI mytarget;

        protected override void OnInit()
        {
            base.OnInit();
            mytarget = (LoadingScreenUI)target;            
            AddFilter("KeyBinding", () => { return false; });

            int group = 0;

            AddHeader("Settings", group);
            AddProperty("showTotalProgress", null, group);
            AddProperty("closeCondition", null, group);
            AddProperty("closeKey", null, group);
            AddFilter("closeKey", () => { return mytarget.closeCondition == LoadingScreenUI.CloseCondition.OnKeyPressed; });
            AddSpace(10, group);

            group = 1;

            AddHeader("Hints", group);
            AddProperty("hintDatabase", null, group);
            AddProperty("hintPrefix", null, group);
            AddSpace(10, group);


            group = 2;

            
            AddProperty("uiElements", null, group);            
        }
    }
#endif
}