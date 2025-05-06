using System;
using System.Collections.Generic;
using LoadingScreenKit.Interfaces;
using UnityEngine;
using UnityEngine.UIElements;

namespace LoadingScreenKit.Components.Controls
{
  
    [UxmlElement]
    public abstract partial class ValueController<T> : VisualController, INotifyValueChanged<T>
   
    {
        public static readonly BindingId valueProperty = "value";

        private T m_value;

        /// <summary>
        /// Determines whether two values are considered equal.
        /// </summary>
        /// <returns>True if the values are considered equal; otherwise, false.</returns>
        protected virtual bool AreValuesEqual(T newValue, T oldValue) => 
            EqualityComparer<T>.Default.Equals(newValue, oldValue);

        [UxmlAttribute]
        public T value
        {
            get => m_value;
            set
            {
                var oldValue = m_value;
                var newValue = value;

                if (AreValuesEqual(newValue, oldValue)) return;

                //If not attached to a panel, skip further logic
                if (base.panel == null)
                    return;

                SetValueWithoutNotify(newValue);

                // Traverse children to propagate value changes
                PropagateValueChange(oldValue, newValue);

                // Notify property changed listeners
                NotifyPropertyChanged(in valueProperty);
            }
        }

        public void SetValueWithoutNotify(T newValue) =>
            m_value = newValue;

        private void PropagateValueChange(T oldValue, T newValue)
        {
            Stack<VisualElement> stack = new();
            stack.Push(this);

            while (stack.Count > 0)
            {
                VisualElement current = stack.Pop();

                foreach (var child in current.Children())
                {
                    if (child is IController)
                    {
                        stack.Push(child); // Traverse further down
                    }

                    if (child is IControllerListener listener)
                    {
                        // Send the change event to the listener
                        using ChangeEvent<T> changeEvent = ChangeEvent<T>.GetPooled(oldValue, newValue);
                        changeEvent.target = child;
                        SendEvent(changeEvent);
                    }
                }
            }
        }  
    }   
}