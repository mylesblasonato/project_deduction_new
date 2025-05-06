namespace AdvancedSceneManager.Editor.UI.Compatibility
{

#if UNITY_6000_0_OR_NEWER 

    [UnityEngine.UIElements.UxmlElement]
    public partial class _EnumField : UnityEngine.UIElements.EnumField
    {
        public _EnumField() { }
        public _EnumField(System.Enum defaultValue) : base(defaultValue) { }
    }

#elif UNITY_2022_1_OR_NEWER

    public class _EnumField : UnityEngine.UIElements.EnumField
    {
        public new class UxmlFactory : UnityEngine.UIElements.UxmlFactory<_EnumField, UxmlTraits> { }
        public new class UxmlTraits : UnityEngine.UIElements.EnumField.UxmlTraits { }

        public _EnumField() { }
        public _EnumField(System.Enum defaultValue) : base(defaultValue) { }
    }

#elif UNITY_2021_3_OR_NEWER

    public class _EnumField : UnityEditor.UIElements.EnumField
    {
        public new class UxmlFactory : UnityEngine.UIElements.UxmlFactory<_EnumField, UxmlTraits> { }
        public new class UxmlTraits : UnityEditor.UIElements.EnumField.UxmlTraits { }

        public _EnumField() { }
        public _EnumField(System.Enum defaultValue) : base(defaultValue) { }
    }

#endif

}
