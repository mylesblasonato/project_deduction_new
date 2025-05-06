namespace AdvancedSceneManager.Editor.UI.Compatibility
{

#if UNITY_6000_0_OR_NEWER 

    [UnityEngine.UIElements.UxmlElement]
    public partial class _IntegerField : UnityEngine.UIElements.IntegerField
    { }

#elif UNITY_2022_1_OR_NEWER

    public class _IntegerField : UnityEngine.UIElements.IntegerField
    {
        public new class UxmlFactory : UnityEngine.UIElements.UxmlFactory<_IntegerField, UxmlTraits> { }
        public new class UxmlTraits : UnityEngine.UIElements.IntegerField.UxmlTraits { }
    }

#elif UNITY_2021_3_OR_NEWER

    public class _IntegerField : UnityEditor.UIElements.IntegerField
    {
        public new class UxmlFactory : UnityEngine.UIElements.UxmlFactory<_IntegerField, UxmlTraits> { }
        public new class UxmlTraits : UnityEditor.UIElements.IntegerField.UxmlTraits { }
    }

#endif

}
