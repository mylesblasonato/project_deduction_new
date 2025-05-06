namespace Runemark.SCEMA
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    [HelpURL("https://runemarkstudio.com/scema/user-guide/")]
    public class SCEMA : ScriptableObject
    {
        public int processCount { get; private set; }        
        public int currentProcessIndex { get; private set; }
        public string currentProcessDescription { get; private set; }
        public float currentProcessProgress { get; private set; }        
        public float totalProgress 
        {
            get 
            {
                float progress = ((float)currentProcessIndex-1) / (float)processCount;
                progress += currentProcessProgress / (float)processCount;
                return progress;
            }
        }

        #region Singleton
        public static SCEMA Instance
        {
            get
            {
                if (instance == null) instance = Resources.Load<SCEMA>("SCEMA");
                return instance;
            }
        }
        static SCEMA instance;
        #endregion

        public GameObject LoadingScreenUI;
        public float MinLoadingTime = 0f;

        [HideInInspector]     
        public List<Sequence> locations = new List<Sequence>();
        //[HideInInspector]
        public List<AdditiveScene> AdditiveScenes = new List<AdditiveScene>();

        Sequence _currentSequence;
        LoadingScreenUI loadingScreen;
        List<string> loadedPersistentScenes = new List<string>();

        public Sequence FindLocation(string sceneName)
        {
            foreach (var l in locations)
            {
                if (l.Scene.name == sceneName) return l;
            }
            return null;
        }
        public void LoadLocation(Sequence sequence)
        {
            if (loadingScreen == null)
            {
                var go = Instantiate(LoadingScreenUI);
                loadingScreen = go.GetComponent<LoadingScreenUI>();
                DontDestroyOnLoad(go);
            }
            loadingScreen.Open(sequence.Name, sequence.LoadingScreen);

            _currentSequence = sequence;
            SceneManager.LoadScene("Bootstrap");
        }
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "Bootstrap")
            {
                loadingScreen.StartCoroutine(LoadScene(_currentSequence, () =>
                {
                    loadingScreen.OnLoadingFinished();                    
                }));
            }
            else if ((int)mode == 4) // In Editor
            {
                _currentSequence = FindLocation(scene.name);
                LoadLocation(_currentSequence);
            }
            else
            {
                var additiveScene = AdditiveScenes.Find(x => x.Scene.name == scene.name);
                if (additiveScene != null && additiveScene.DontDestroyOnLoad)
                {
                    Instance.loadedPersistentScenes.Add(additiveScene.Scene.name);
                    foreach (var go in scene.GetRootGameObjects())
                    {
                        MonoBehaviour.DontDestroyOnLoad(go);
                    }
                }            
            }
        }

        void NextProcess(string description)
        {
            currentProcessIndex++;
            currentProcessDescription = description;
            currentProcessProgress = 0f; 
        }

        IEnumerator LoadScene(Sequence sequence, SimpleMethodDelegate onComplete)
        {
            currentProcessIndex = 0;
            yield return new WaitForEndOfFrame();

            List<AsyncOperation> processes = new List<AsyncOperation>();
            float startTime = Time.time;
                       
            var additiveSceneList = AdditiveScenes.FindAll(x =>
                               _currentSequence.additiveScenes.Contains(x.Scene.name) &&
                               !loadedPersistentScenes.Contains(x.Scene.name)
                            );
            processCount = additiveSceneList.Count + 2;

            NextProcess("Loading " + sequence.Name);

            AsyncOperation mainSceneProgress = SceneManager.LoadSceneAsync(sequence.Scene.name, UnityEngine.SceneManagement.LoadSceneMode.Additive);
            mainSceneProgress.allowSceneActivation = false;
            while (!mainSceneProgress.isDone)
            {
                currentProcessProgress = mainSceneProgress.progress;
                if (currentProcessProgress == .9f)
                {
                    processes.Add(mainSceneProgress);
                    break;
                }
                yield return null;
            }

            for (int i = 0; i < additiveSceneList.Count; i++)
            {
                string sceneName = additiveSceneList[i].Scene.name;

                NextProcess("Loading " + sceneName);
                
                AsyncOperation additiveProcess = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                additiveProcess.allowSceneActivation = false;
                while (!additiveProcess.isDone)
                {
                    currentProcessProgress = additiveProcess.progress;
                    if (currentProcessProgress == .9f)
                    {
                        processes.Add(additiveProcess);
                        break;
                    }
                    yield return null;
                }
            }

            NextProcess("Building the level...");
            if (Instance.MinLoadingTime > 0)
            {               
                float waitTime = Instance.MinLoadingTime - (Time.time - startTime);
                float totalWaitTime = waitTime;
                while (waitTime > 0f)
                {
                    waitTime -= Time.deltaTime;
                    currentProcessProgress = 1 - (waitTime / totalWaitTime);
                    yield return null;
                }
            }

            foreach (var process in processes)
            {
                process.allowSceneActivation = true;
            }

            NextProcess("Loading Finished");

            if (onComplete != null)
                onComplete();
        }
     
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnBeforeSceneLoadRuntimeMethod()
        {
            Instance.loadedPersistentScenes.Clear();
   
            SceneManager.sceneLoaded -= Instance.OnSceneLoaded;
            SceneManager.sceneLoaded += Instance.OnSceneLoaded;
        }

        [System.Serializable]
        public class AdditiveScene
        {
            public SceneAsset Scene;           
            public bool DontDestroyOnLoad = false;
        }
        public delegate void SimpleMethodDelegate();
    }
}