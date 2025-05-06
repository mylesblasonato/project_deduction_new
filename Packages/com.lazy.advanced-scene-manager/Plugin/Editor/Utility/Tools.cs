#if ASM_DEV && UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace AdvancedSceneManager.Editor.Utility
{

    public static class Tools
    {

        [MenuItem("Tools/ASM/Recompile...")]
        static void Recompile() =>
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();

        [MenuItem("Tools/ASM/View Unity API timeline...")]
        static void ApiTimeline() =>
            Application.OpenURL("https://ngtools.tech/unityapitimeline.php");

    }

}
#endif
