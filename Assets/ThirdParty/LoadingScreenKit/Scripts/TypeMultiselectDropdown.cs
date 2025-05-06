#nullable enable

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

namespace LoadingScreenKit.Scripts
{
    public class TypeMultiselectDropdown : MultiselectDropdown<Type>
    {

        public TypeMultiselectDropdown(IEnumerable<Type> types, IEnumerable<Type> selectedTypes, Func<Type, string>? getDisplayString = null) : base(types, selectedTypes, getDisplayString)
        { }

    }

    public class TypePropertyDrawerAttribute : PropertyAttribute
    {

        public enum DisplayTypeEnum
        {
            Name, Fullname, AssemblyQualifiedName
        }

        public Type Type { get; }
        public DisplayTypeEnum DisplayType { get; }

        public TypePropertyDrawerAttribute(Type type, DisplayTypeEnum displayType = DisplayTypeEnum.Name) : base(applyToCollection: true)
        {
            Type = type;
            DisplayType = displayType;
        }

    }

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(TypePropertyDrawerAttribute))]
    public class TypeArrayPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {

            property.serializedObject.Update();

            var attr = (TypePropertyDrawerAttribute)attribute;
            var types = TypeUtility.GetTypesAssignableFrom(attr.Type);

            var selectedTypes = new List<Type>();
            for (int i = 0; i < property.arraySize; i++)
            {
                var typeName = UnityWebRequest.UnEscapeURL(property.GetArrayElementAtIndex(i).stringValue);
                //Debug.Log(property.GetArrayElementAtIndex(i).stringValue);
                if (!string.IsNullOrEmpty(typeName))
                {
                    var type = Type.GetType(typeName, throwOnError: false);
                    if (type != null)
                    {
                        selectedTypes.Add(type);
                    }
                }
            }

            var field = new TypeMultiselectDropdown(types, selectedTypes, t => GetDisplayString(t, attr));
            field.label = property.displayName;

            field.RegisterValueChangedCallback(e =>
            {

                selectedTypes.Clear();
                selectedTypes.AddRange(e);

                property.arraySize = selectedTypes.Count;
                for (int i = 0; i < property.arraySize; i++)
                {
                    //Debug.LogWarning(selectedTypes[i].AssemblyQualifiedName);
                    property.GetArrayElementAtIndex(i).stringValue = UnityWebRequest.EscapeURL(selectedTypes[i].AssemblyQualifiedName);
                }

                property.serializedObject.ApplyModifiedProperties();

            });

            return field;

        }

        string GetDisplayString(Type type, TypePropertyDrawerAttribute attribute)
        {
            if (attribute.DisplayType == TypePropertyDrawerAttribute.DisplayTypeEnum.Fullname)
                return type.FullName;
            else if (attribute.DisplayType == TypePropertyDrawerAttribute.DisplayTypeEnum.AssemblyQualifiedName)
                return type.AssemblyQualifiedName;
            else
                return type.Name;
        }

    }

#endif

}
