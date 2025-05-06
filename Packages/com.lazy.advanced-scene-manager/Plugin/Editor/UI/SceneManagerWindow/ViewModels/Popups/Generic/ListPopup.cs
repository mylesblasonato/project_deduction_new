using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AdvancedSceneManager.Editor.UI.Interfaces;
using AdvancedSceneManager.Editor.UI.Utility;
using AdvancedSceneManager.Models.Internal;
using UnityEngine.UIElements;

namespace AdvancedSceneManager.Editor.UI.Views.Popups
{

    abstract class ListPopup<T> : ExtendableViewModel, IPopup where T : ASMModelBase
    {

        public override VisualElement ExtendableButtonContainer => null;
        public override bool IsOverflow => false;
        public override ElementLocation Location => ElementLocation.Header;

        VisualTreeAsset listItem => viewLocator.items.list;

        public abstract void OnAdd();
        public abstract void OnSelected(T item);

        public virtual void OnRemove(T item) { }
        public virtual void OnRename(T item) { }
        public virtual void OnDuplicate(T item) { }

        public virtual bool displayRenameButton { get; }
        public virtual bool displayRemoveButton { get; }
        public virtual bool displayDuplicateButton { get; }
        public virtual bool displaySortButton { get; }

        public abstract string noItemsText { get; }
        public abstract string headerText { get; }

        public abstract IEnumerable<T> items { get; }

        public virtual IEnumerable<T> Sort(IEnumerable<T> items, ListSortDirection sortDirection) =>
            items;

        T[] list;

        VisualElement container;
        Button sortButton;
        public override void OnAdded()
        {

            base.OnAdded();

            this.container = view;

            container.BindToSettings();

            container.Q<Label>("text-header").text = headerText;
            container.Q<Label>("text-no-items").text = noItemsText;

            container.Q<Button>("button-add").clicked += OnAdd;

            var list = container.Q<ListView>();

            list.makeItem = () => Instantiate(listItem);

            list.unbindItem = Unbind;
            list.bindItem = Bind;

            sortButton = view.Q<Button>("button-sort");
            sortButton.SetVisible(displaySortButton);
            sortButton.text = SceneManager.settings.user.m_sortDirection == ListSortDirection.Ascending ? "" : "";
            sortButton.tooltip = SceneManager.settings.user.m_sortDirection == ListSortDirection.Ascending ? "Sort by: Descending" : "Sort by: Ascending";

            sortButton.RegisterCallback<ClickEvent>(e =>
            {
                SceneManager.settings.user.m_sortDirection = SceneManager.settings.user.m_sortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
                SceneManager.settings.user.Save();
                sortButton.text = SceneManager.settings.user.m_sortDirection == ListSortDirection.Ascending ? "" : "";
                sortButton.tooltip = SceneManager.settings.user.m_sortDirection == ListSortDirection.Ascending ? "Sort by: Descending" : "Sort by: Ascending";
                Reload();
            });

            Reload();

            view.Q<ScrollView>().PersistScrollPosition();

        }

        public void Reload()
        {
            list = Sort(items.Where(o => o), SceneManager.settings.user.m_sortDirection).ToArray();
            container.Q("text-no-items").SetVisible(!list.Any());
            container.Q<ListView>().itemsSource = list;
            container.Q<ListView>().Rebuild();
        }

        void Unbind(VisualElement element, int index)
        {

            var nameButton = element.Q<Button>("button-name");
            var menuButton = element.Q<Button>("button-menu");
            nameButton.userData = null;

            nameButton.UnregisterCallback<ClickEvent>(OnSelect);

        }

        void OnSelect(ClickEvent e)
        {
            if (e.target is Button button && button.userData is T t)
                OnSelected(t);
        }

        void Bind(VisualElement element, int index)
        {

            var item = list.ElementAt(index);
            var nameButton = element.Q<Button>("button-name");
            var menuButton = element.Q<Button>("button-menu");

            nameButton.text = item.name;
            nameButton.RegisterCallback<ClickEvent>(OnSelect);
            menuButton.SetupMenuButton(
                ("Rename", () => OnRename(item), displayRenameButton),
                ("Duplicate", () => OnDuplicate(item), displayDuplicateButton),
                ("Remove", () => OnRemove(item), displayRemoveButton));

            menuButton.SetVisible(displayRenameButton || displayDuplicateButton || displayRemoveButton);
            menuButton.style.opacity = 0;

            nameButton.userData = item;

        }

    }

}
