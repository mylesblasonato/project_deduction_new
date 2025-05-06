namespace Runemark.SCEMA
{
    using Runemark.Common;
    using System.Collections.Generic;
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    [HelpURL("https://runemarkstudio.com/scema/user-guide/")]
    [CreateAssetMenu( fileName = "New Location", menuName ="RunemarkStudio/Location")]
    public class Sequence : ScriptableObject
    {
        public string Name;
        public Sprite LoadingScreen;
        public SceneAsset Scene;
        public List<string> additiveScenes = new List<string>();
        
        public void Enter()
        {
            SCEMA.Instance.LoadLocation(this);
        }

        public override string ToString()
        {
            return "Location: " + Name;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Sequence))]
    public class LocationInspector : CustomInspectorBase
    {
        Sequence myTarget;
        List<string> additives = new List<string>();

        protected override void OnInit()
        {
            myTarget = (Sequence)target;
            foreach (var a in SCEMA.Instance.AdditiveScenes)
            {
                additives.Add(a.Scene.name);
            }

            int group = 0;
            AddProperty("Name", null, group);
            AddProperty("LoadingScreen", null, group);

            var sceneProp = FindProperty("Scene.asset");
            AddCustomField("Scene", () => 
            {
                EditorGUILayout.PropertyField(sceneProp, new GUIContent("Scene"));
                if (myTarget.Scene.Asset == null)
                    myTarget.Scene.name = "";
                else if (myTarget.Scene.name != myTarget.Scene.Asset.name)
                {
                    myTarget.Scene.name = myTarget.Scene.Asset.name;
                }

            }, group);


          
            group = 1;
            AddHeader("Additive Scenes", group);

            AddCustomField("Additives", () => 
            {              
                foreach (var scene in additives)
                {
                    EditorGUI.BeginChangeCheck();
                    bool enabled = myTarget.additiveScenes.Contains(scene);
                    bool newEnabled = EditorGUILayout.Toggle(scene, enabled);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (newEnabled) myTarget.additiveScenes.Add(scene);
                        else myTarget.additiveScenes.Remove(scene);
                    }
                }
            }, group);
        }
    }
#endif
}