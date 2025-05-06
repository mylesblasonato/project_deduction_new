namespace AdvancedSceneManager.Editor.UI.Compatibility
{

#if UNITY_6000_0_OR_NEWER 

    [UnityEngine.UIElements.UxmlElement]
    public partial class _FloatField : UnityEngine.UIElements.FloatField
    { }

#elif UNITY_2022_1_OR_NEWER

    public class _FloatField : UnityEngine.UIElements.FloatField
    {
        public new class UxmlFactory : UnityEngine.UIElements.UxmlFactory<_FloatField, UxmlTraits> { }
        public new class UxmlTraits : UnityEngine.UIElements.FloatField.UxmlTraits { }
    }

#elif UNITY_2021_3_OR_NEWER

    public class _FloatField : UnityEditor.UIElements.FloatField
    {
        public new class UxmlFactory : UnityEngine.UIElements.UxmlFactory<_FloatField, UxmlTraits> { }
        public new class UxmlTraits : UnityEditor.UIElements.FloatField.UxmlTraits { }
    }

#endif

}
