using System.Collections;
using AdvancedSceneManager.Loading;
using AdvancedSceneManager.Utility;
using UnityEngine;
using UnityEngine.UIElements;

namespace LoadingScreenKit.Sample
{
    public class LoadingScreenSample : LoadingScreen
    {
        [SerializeField] UIDocument UIDocument;

        VisualElement background, components;

        // Cache WaitForSeconds objects
        private readonly WaitForSeconds waitShort = new (0.2f);
        private readonly WaitForSeconds waitLong = new (0.5f);

        protected override void Start()
        {
            base.Start();

            background = UIDocument.rootVisualElement.Q<VisualElement>("Background");
            background.enabledSelf = false;

            components = UIDocument.rootVisualElement.Q<VisualElement>("Components");
            components.enabledSelf = false;
        }

        public override IEnumerator OnOpen()
        {
            background.enabledSelf = true;
            yield return waitLong;

            components.enabledSelf = true;
            yield return waitShort;
        }

        public override IEnumerator OnClose()
        {
            // I like a slight delay after button press before fade.
            yield return waitLong;

            components.enabledSelf = false;
            yield return waitShort;

            background.enabledSelf = false;
            yield return waitLong;
        }
    }
}
