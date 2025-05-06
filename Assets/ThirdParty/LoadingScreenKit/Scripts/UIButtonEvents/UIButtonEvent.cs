using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace LoadingScreenKit.Scripts.Utilities
{
    [RequireComponent(typeof(UIDocument))]
    public class UIButtonEvent : MonoBehaviour
    {
        [SerializeField] private UIDocument document;

        public List<ButtonRef> Buttons = new();

        private void OnEnable()
        {
            document = GetComponent<UIDocument>();

            var buttons = document.rootVisualElement.Query<Button>().ToList();

            if (buttons == null) return;

            foreach (var button in buttons)
            {
                UnityEvent unityEvent = Buttons.First(x => x.buttonName == button.name).onClick;

                button.RegisterCallback<ClickEvent>(x => unityEvent.Invoke());
            }
        }

        [Serializable]
        public class ButtonRef
        {
            public string buttonName;
            public UnityEvent onClick;

            public ButtonRef(string button)
            {
                this.buttonName = button;
                this.onClick = new UnityEvent();
            }
        }
    }
}