using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace Game.Prototype_Code
{
    public class AIServerLauncher : MonoBehaviour
    {
        private static AIServerLauncher instance;
        private Process aiProcess;
        private string aiServerPath;

        void Awake()
        {
            // ✅ Ensure only ONE instance of this script exists
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject); // Keep it running across scenes
        }

        void Start()
        {
            aiServerPath = Path.Combine(Application.dataPath, "AI", "dist", "ai_server.exe");

            if (IsProcessRunning("ai_server"))
            {
                UnityEngine.Debug.Log("🟢 AI Server is already running. No new instance will be launched.");
                return;
            }

            if (File.Exists(aiServerPath))
            {
                aiProcess = new Process();
                aiProcess.StartInfo.FileName = aiServerPath;
                aiProcess.StartInfo.UseShellExecute = false;
                aiProcess.StartInfo.CreateNoWindow = true;
                aiProcess.Start();

                UnityEngine.Debug.Log("🚀 AI Server launched successfully.");
            }
            else
            {
                UnityEngine.Debug.LogError("❌ AI Server executable not found!");
            }
        }

        void OnApplicationQuit()
        {
            if (aiProcess != null && !aiProcess.HasExited)
            {
                aiProcess.Kill();
                UnityEngine.Debug.Log("🛑 AI Server process terminated.");
            }
        }

        private bool IsProcessRunning(string processName)
        {
            return Process.GetProcessesByName(processName).Length > 0;
        }
    }
}