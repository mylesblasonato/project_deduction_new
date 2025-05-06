namespace Runemark.Common
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    public class WindowBaseUI : MonoBehaviour
    {
        public KeyCode KeyBinding;

        protected bool isOpen { get { return window.activeSelf; } }
        GameObject window;

        protected virtual void OnEnable()
        {
            foreach (var t in transform.GetComponentsInChildren<Transform>())
            {
                if (t.name.ToLower() == "window")
                {
                    window = t.gameObject;
                    break;
                }
            }
            Close();
        }
        void Update()
        {
            if (Input.GetKeyUp(KeyBinding))
                Toggle();

            if (window.activeSelf)
                OnUpdate();
        }

        public void Open()
        {
            window.SetActive(true);
            OnOpen();
        }
        public void Close()
        {
            window.SetActive(false);
            OnClose();
        }
        public void Toggle()
        {
            if (window.activeSelf) Close();
            else Open();
        }

        protected virtual void OnOpen() { }
        protected virtual void OnClose() { }
        protected virtual void OnUpdate() { }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(WindowBaseUI))]
    public class WindowBaseUIInspector : CustomInspectorBase
    {
        protected override void OnInit()
        {
            AddProperty("KeyBinding");
        }
    }
#endif
}