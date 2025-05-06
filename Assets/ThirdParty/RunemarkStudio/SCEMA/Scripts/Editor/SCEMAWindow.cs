namespace Runemark.SCEMA
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.SceneManagement;

    public class SCEMAWindow : EditorWindow
    {
        [MenuItem("Window/RunemarkStudio/SCEMA")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(SCEMAWindow), false, "SCEMA");
        }

        SCEMA settings;
        BuildScenesManager buildScenes = new BuildScenesManager();
        SceneAsset bootstrapScene;

        string searchField = "";
        Vector2 scrollPosition = Vector2.zero;

        enum Tab { LocationList, Settings, Tester }
        Tab tab = Tab.LocationList;

        GUISkin skin;

        SerializedObject settingsObject;
        SerializedProperty loadingScreenProp;
        SerializedProperty minLoadingTimeProp;
        SerializedProperty additiveScenesProp;

        void OnEnable()
        {
            LoadSettings();
            Refresh();

            string[] skinsGUID = AssetDatabase.FindAssets("SCEMAWindowSkin");
            skin = AssetUtilities.LoadAsset<GUISkin>(skinsGUID[0]);

            settingsObject = new SerializedObject(settings);
            loadingScreenProp = settingsObject.FindProperty("LoadingScreenUI");
            minLoadingTimeProp = settingsObject.FindProperty("MinLoadingTime");
            additiveScenesProp = settingsObject.FindProperty("AdditiveScenes");
        }
        void OnGUI()
        {
            string title = "SCEMA";
            switch (tab)
            {
                case Tab.Settings: title += " - Settings"; break;
                case Tab.Tester: title += " - Setup Wizard"; break;
            }

            // Header
            GUI.Label(new Rect(0, 0, position.width, 40), title, skin.FindStyle("Header"));

            if (GUI.Button(new Rect(position.width - 30, 10, 20, 20), new GUIContent("","Refresh"), skin.FindStyle("Refresh")))
            {
                Refresh();
                return;
            }
            if (GUI.Button(new Rect(position.width - 55, 10, 20, 20), new GUIContent("", "Settings"), skin.FindStyle(tab == Tab.Settings ? "Settings-ON" : "Settings")))
            {
                tab = tab == Tab.Settings ? Tab.LocationList : Tab.Settings;
            }
            if (GUI.Button(new Rect(position.width - 80, 10, 20, 20), new GUIContent("", "Setup Wizard"), skin.FindStyle(tab == Tab.Tester ? "Setup-ON" : currentError != ErrorMessage.NoProblem ? "Setup-Alert" : "Setup")))
            {
                tab = tab == Tab.Tester ? Tab.LocationList : Tab.Tester;
            }


            Rect rect = new Rect(0, 40, position.width, position.height - 50);
            switch (tab)
            {
                case Tab.LocationList:
                    DrawSearchField(new Rect(rect.x, rect.y, rect.width - 20, 20));
                    DrawList(new Rect(rect.x, rect.y + 20, rect.width, rect.height - 20), settings.locations);
                    break;

                case Tab.Settings:
                    Rect r = rect;

                    r.x += 10;
                    r.height = 15;
                    r.width -= 20;


                    settingsObject.Update();
                    EditorGUI.PropertyField(r, loadingScreenProp);
                    r.y += r.height + 5;

                    EditorGUI.PropertyField(r, minLoadingTimeProp);
                    r.y += r.height + 5;
                   
                    GUI.Label(r, "Additive Scenes", EditorStyles.boldLabel);
                    r.y += r.height + 5;

                    if (GUI.Button(new Rect(rect.x, rect.y + rect.height - 10, rect.width, 20), "ADD"))
                    {
                        settings.AdditiveScenes.Add(new SCEMA.AdditiveScene());
                        Repaint();
                        return;
                    }

                    r.height = 35;
                    EditorGUI.HelpBox(r, "These scenes will load additive to locations.Persistent means GameObjects in this scene won't destroy on load.", MessageType.Info);
                    r.y += r.height + 5; r.height = 20;
                    DrawList(new Rect(rect.x, r.y, rect.width, rect.height - r.y + 30), additiveScenesProp);


                    settingsObject.ApplyModifiedProperties();
                    break;

                case Tab.Tester:                
                    SearchForProblems();
                   
                    switch (currentError)
                    {
                        case ErrorMessage.BootstrapMissing:
                            if (DrawTestResult(ref rect, "Error: Bootstrap scene is missing!"))
                            {
                                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                                if (EditorSceneManager.SaveScene(scene, "Assets/RunemarkStudio/SCEMA/Scene/Bootstrap.unity"))
                                {
                                    Debug.Log("Bootstrap created");
                                }
                            }
                            break;

                        case ErrorMessage.BootstrapNotInBuildSettings:
                            if (DrawTestResult(ref rect, "Error: Bootstrap should be in the build settings!"))
                            {
                                buildScenes.Add(bootstrapScene);
                                Debug.Log("Bootstrap is added to build settings.");
                            }
                            break;

                        case ErrorMessage.SCEMAMissing:
                            if (DrawTestResult(ref rect, "Error: SCEMA asset is missing!"))
                            {
                                settings = AssetUtilities.CreateAsset<SCEMA>("RunemarkStudio/Resources", "SCEMA");
                            }                            
                            break;

                        case ErrorMessage.NoProblem:
                            rect.height = 30f;
                            EditorGUI.HelpBox(rect, "Everything is working correctly!", MessageType.Info);
                            break;
                    }
                    break;
            }
        }

        bool DrawTestResult(ref Rect rect, string msg)
        {
            rect.height = 30f;
            EditorGUI.HelpBox(rect, msg, MessageType.Error);
            rect.y += rect.height;
            rect.height = 20;

            if (GUI.Button(rect, "Fix this problem"))
            {
                rect.y += rect.height;
                return true;
            }
            else
                return false;
        }

        void DrawSearchField(Rect rect)
        {            
            searchField = EditorGUI.TextField(rect, "", searchField, (GUIStyle)"SearchTextField");
            rect.x += rect.width;
            rect.width = 20;
            if (GUI.Button(rect, "", (GUIStyle)"SearchCancelButton"))
            {
                searchField = "";
                GUI.SetNextControlName("");
                GUI.FocusControl("");
            }
        }
        void DrawList(Rect rect, List<Sequence> list)
        {
            scrollPosition = GUI.BeginScrollView(rect, scrollPosition, new Rect(0, 0, position.width - 20, settings.locations.Count * 20));
            for (int i = 0; i < list.Count; i++)
            {
                Sequence sequence = list[i];
                if (sequence == null) continue;
                if (!sequence.name.ToLower().Contains(searchField.ToLower())) continue;
                DrawElement(sequence);
            }
            GUI.EndScrollView();
        }
        void DrawList(Rect rect, SerializedProperty prop)
        {
            scrollPosition = GUI.BeginScrollView(rect, scrollPosition, new Rect(0, 0, rect.width - 20, prop.arraySize * 25));
            for (int i = 0; i < prop.arraySize; i++)
            {
                var element = prop.GetArrayElementAtIndex(i);
                bool remove = DrawElement(i, element);
                if (remove)
                {
                    prop.DeleteArrayElementAtIndex(i);
                    return;
                }               
            }
            GUI.EndScrollView();
        }

        void DrawElement(Sequence sequence)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5);
            BuildSettingsControl(sequence.Scene);

            GUILayout.Label(sequence.Name);
            GUILayout.FlexibleSpace();

            // SEARCH
            if (GUILayout.Button(new GUIContent("", "Find location object"), skin.FindStyle("Search"), GUILayout.Width(20), GUILayout.Height(20)))
            {
                EditorGUIUtility.PingObject(sequence);
            }

            LoadSceneControl(sequence.Scene);
            EditorGUILayout.EndHorizontal();
        }
        bool DrawElement(int index, SerializedProperty element)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5);

            var additive = settings.AdditiveScenes[index];

            var sceneProp = element.FindPropertyRelative("Scene");
            var sceneAssetProp = sceneProp.FindPropertyRelative("asset");
            var sceneNameProp = sceneProp.FindPropertyRelative("name");
            var persistentProp = element.FindPropertyRelative("DontDestroyOnLoad");

            BuildSettingsControl(additive.Scene);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(sceneAssetProp, GUIContent.none);

            if (EditorGUI.EndChangeCheck())
            {
                sceneNameProp.stringValue = sceneAssetProp.objectReferenceValue.name;
            }

            GUILayout.FlexibleSpace();

            EditorGUILayout.PropertyField(persistentProp, GUIContent.none, GUILayout.Width(20));
            GUILayout.Label(new GUIContent("Persistent", "Enable it to don't destroy on load."));

            GUILayout.FlexibleSpace();

            if (DrawLayoutButton(skin.FindStyle("Delete"), false, new GUIContent("", "Remove")))
            {
                return true;
            }

            LoadSceneControl(additive.Scene);

            GUILayout.Space(20);
            EditorGUILayout.EndHorizontal();
            return false;
        }

        void BuildSettingsControl(SceneAsset scene)
        {
            bool inBuildSettings = buildScenes.Contains(scene);
            GUIStyle buildSettingButton = skin.FindStyle(inBuildSettings ? "ON" : "OFF");
            if (DrawLayoutButton(buildSettingButton, scene == null || scene.Asset == null, new GUIContent("", inBuildSettings ? "Remove from build settings" : "Add to build settings")))
            {
                if (inBuildSettings) buildScenes.Remove(scene);
                else buildScenes.Add(scene);
            }
        }        
        void LoadSceneControl(SceneAsset scene)
        {
            bool isLoaded = IsSceneLoaded(scene);
            if (DrawLayoutButton(skin.FindStyle(isLoaded ? "Close" : "Open"), scene == null || scene.Asset == null, new GUIContent("", "Open Scene")))
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                var scenePath = AssetDatabase.GetAssetPath(scene.Asset);
                OpenSceneMode mode = Event.current.shift ? OpenSceneMode.Additive : OpenSceneMode.Single;
                EditorSceneManager.OpenScene(scenePath, mode);
            }
        }      
        bool DrawLayoutButton(GUIStyle style, bool hide, GUIContent label = null)
        {
            if (label == null) label = GUIContent.none;

            if (hide)
            {
                GUILayout.Label(label, skin.label, GUILayout.Width(20));
                return false;
            }
            return GUILayout.Button(label, style, GUILayout.Width(20), GUILayout.Height(20));
        }

        void LoadSettings()
        {
            settings = GetSCEMA();
        }
        void Refresh()
        {
            LoadSettings();
            SearchForProblems();

            string[] locationGUIDs = AssetDatabase.FindAssets("t:Sequence");
            settings.locations.Clear();

            foreach (var guid in locationGUIDs)
            {
                settings.locations.Add(AssetUtilities.LoadAsset<Sequence>(guid));
            }
        }

        bool IsSceneLoaded(SceneAsset scene)
        {
            bool result = false;
            if (scene != null)
            {
                for (int cnt = 0; cnt < EditorSceneManager.sceneCount; cnt++)
                {
                    var s = EditorSceneManager.GetSceneAt(cnt);
                    if (scene.Asset != null && s.name == scene.Asset.name)
                    {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }

        #region Searching for Errors
        ErrorMessage currentError;
        enum ErrorMessage
        {
            NoProblem, 

            BootstrapMissing,
            BootstrapNotInBuildSettings, 

            SCEMAMissing,
        }
        void SearchForProblems()
        {
            bootstrapScene = GetBootstrap();
            var scema = GetSCEMA();

            if (bootstrapScene == null)
                currentError = ErrorMessage.BootstrapMissing;
            else if (!buildScenes.Contains(bootstrapScene))
                currentError = ErrorMessage.BootstrapNotInBuildSettings;
            else if (scema == null)
                currentError = ErrorMessage.SCEMAMissing;
            else
                currentError = ErrorMessage.NoProblem;
        }

        SceneAsset GetBootstrap()
        {
            string[] bootstrapGUID = AssetDatabase.FindAssets("Bootstrap");
            SceneAsset bootstrap = null;
            if (bootstrapGUID.Length > 0)
            {
                bootstrap = new SceneAsset(AssetUtilities.LoadAsset<UnityEditor.SceneAsset>(bootstrapGUID[0]));
            }
            return bootstrap;
        }
        SCEMA GetSCEMA()
        {
            string[] settingsGUID = AssetDatabase.FindAssets("t:SCEMA");
            if (settingsGUID.Length > 0)
                return AssetUtilities.LoadAsset<SCEMA>(settingsGUID[0]);
            return null;
        }
        #endregion        
    }


  
}