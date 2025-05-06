#if UNITY_EDITOR

using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Slate
{

    [CustomEditor(typeof(CutsceneGroup), true)]
    public class CutsceneGroupInspector : Editor
    {

        private CutsceneGroup group {
            get { return (CutsceneGroup)target; }
        }

        public override void OnInspectorGUI() {
            GUI.enabled = group.root.currentTime == 0;
            base.OnInspectorGUI();
        }
    }
}

#endif