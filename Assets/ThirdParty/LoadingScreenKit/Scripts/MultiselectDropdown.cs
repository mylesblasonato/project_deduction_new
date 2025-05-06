#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace LoadingScreenKit.Scripts
{
    public class MultiselectDropdown<T> : DropdownField
    {

        #region Properties

        private List<T> _choices = null!;
        private List<T> _selectedChoices = null!;
        private Func<T, string> _getDisplayString;

        public new IEnumerable<T> choices
        {
            get => _choices;
            set
            {
                _choices = value.ToList();
                UpdateText();
            }
        }

        public IEnumerable<T> selectedChoices
        {
            get => _selectedChoices;
            set
            {
                _selectedChoices = value.ToList();
                UpdateText();
            }
        }

        public new IEnumerable<T> value
        {
            get => selectedChoices;
            set => selectedChoices = value;
        }

        public Func<T, string> getDisplayString
        {
            get => _getDisplayString;
            set
            {
                _getDisplayString = value;
                UpdateText();
            }
        }

        public bool isAllSelected => _choices.All(_selectedChoices.Contains);
        public bool isNoneSelected => !_selectedChoices.Any();

        string _noneText = "None";
        string _allText = "Everything";

        [UxmlAttribute]
        public string noneText
        {
            get => _noneText;
            set
            {
                _noneText = value;
                UpdateText();
            }
        }

        [UxmlAttribute]
        public string allText
        {
            get => _allText;
            set
            {
                _allText = value;
                UpdateText();
            }
        }

        #endregion
        #region Context menu

        private void BuildContextualMenu(ContextualMenuPopulateEvent e)
        {

            e.menu.AppendAction("Nothing", SelectNone, IsNoneSelected);
            e.menu.AppendAction("Everything", SelectAll, IsAllSelected);
            e.menu.AppendSeparator();

            foreach (var choice in _choices)
                e.menu.AppendAction(getDisplayString?.Invoke(choice), Select, IsSelected, userData: choice);

        }

        DropdownMenuAction.Status IsAllSelected(DropdownMenuAction e)
        {
            return isAllSelected ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal;
        }

        DropdownMenuAction.Status IsNoneSelected(DropdownMenuAction e)
        {
            return isNoneSelected ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal;
        }

        DropdownMenuAction.Status IsSelected(DropdownMenuAction e)
        {
            return _selectedChoices.Contains((T)e.userData) ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal;
        }

        void SelectAll(DropdownMenuAction e)
        {
            _selectedChoices.Clear();
            _selectedChoices.AddRange(_choices);
            UpdateText();
        }

        void SelectNone(DropdownMenuAction e)
        {
            _selectedChoices.Clear();
            UpdateText();
        }

        void Select(DropdownMenuAction e)
        {
            if (_selectedChoices.Contains((T)e.userData))
                _selectedChoices.Remove((T)e.userData);
            else
                _selectedChoices.Add((T)e.userData);

            UpdateText();
        }

        #endregion
        #region Value changed 

        private readonly Dictionary<EventCallback<IEnumerable<T>>, EventCallback<ChangeEvent<string>>> callbacks = new();

        public bool RegisterValueChangedCallback(EventCallback<IEnumerable<T>> callback)
        {

            //Base class uses ChangeEvent<string>,
            //we need to override that with ChangeEvent<IEnumerable<T>>

            var stringCallback = new EventCallback<ChangeEvent<string>>((e) => callback.Invoke(selectedChoices));

            if (INotifyValueChangedExtensions.RegisterValueChangedCallback(this, stringCallback))
            {
                //Save both callbacks, to support unregistering
                callbacks.Add(callback, stringCallback);
                return true;
            }

            return false;

        }

        public bool UnregisterValueChangedCallback(EventCallback<IEnumerable<T>> callback)
        {

            if (callbacks.TryGetValue(callback, out var stringCallback))
            {
                return INotifyValueChangedExtensions.UnregisterValueChangedCallback(this, stringCallback);
            }

            return false;

        }

        #endregion

        public MultiselectDropdown(IEnumerable<T> choices, IEnumerable<T>? selectedChoices = null, Func<T, string>? getDisplayString = null)
        {

            getDisplayString ??= o => o is not null ? o.ToString() : "null";

            _choices = choices.ToList();
            _selectedChoices = selectedChoices.ToList() ?? new();
            _getDisplayString = getDisplayString;

            var manipulator = new ContextualMenuManipulator(BuildContextualMenu);
            manipulator.activators.Add(new() { button = MouseButton.LeftMouse });
            this.AddManipulator(manipulator);

            UpdateText();

        }

        void UpdateText()
        {
            base.value = GetSelectionAsString();
        }

        public string GetSelectionAsString(bool userChoicesOnly = false)
        {
            if (isAllSelected && !userChoicesOnly)
                return allText;
            else if (isNoneSelected && !userChoicesOnly)
                return noneText;
            else
                return string.Join(", ", _choices.Where(_selectedChoices.Contains).Select(getDisplayString));
        }

    }

}
