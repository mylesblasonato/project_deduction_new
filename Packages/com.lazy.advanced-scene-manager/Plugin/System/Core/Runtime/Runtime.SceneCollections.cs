using System;
using System.Collections.Generic;
using System.Linq;
using AdvancedSceneManager.Callbacks.Events;
using AdvancedSceneManager.Models;
using AdvancedSceneManager.Utility;
using UnityEngine;

namespace AdvancedSceneManager.Core
{

    static class SceneOperationExtensions
    {

        public static SceneOperation TrackCollectionCallback(this SceneOperation operation, SceneCollection collection, bool isAdditive = false)
        {

            bool isClosingCollection = SceneManager.openCollection;

            operation.RegisterCallback<SceneClosePhaseEvent>(e =>
            {

                if (!collection)
                    return;

                SceneManager.runtime.Untrack(collection, isAdditive);

                //Make sure additive collection is removed when it is opened non-additively
                if (!isAdditive)
                    SceneManager.runtime.Untrack(collection, true);

                e.WaitFor(e.operation.InvokeCallback(CollectionCloseEvent.GetPooled(collection)));

            }, When.After);

            operation.RegisterCallback<SceneOpenPhaseEvent>(e =>
            {
                SceneManager.runtime.Track(collection, isAdditive);
                e.WaitFor(e.operation.InvokeCallback(CollectionOpenEvent.GetPooled(collection)));
            }, When.After);

            return operation;

        }

        public static SceneOperation UntrackCollectionCallback(this SceneOperation operation, SceneCollection collection, bool isAdditive = false)
        {
            operation.RegisterCallback<SceneClosePhaseEvent>(e => SceneManager.runtime.Untrack(collection, isAdditive), When.After);
            return operation;
        }

        public static SceneOperation UntrackAllCollectionsCallback(this SceneOperation operation)
        {
            operation.RegisterCallback<SceneClosePhaseEvent>(e => SceneManager.runtime.UntrackCollections(), When.After);
            return operation;
        }

    }

    partial class Runtime
    {

        private SceneCollection m_openCollection
        {
            get => SceneManager.settings.project.openCollection;
            set => SceneManager.settings.project.openCollection = value;
        }

        /// <summary>Gets the collections that are opened as additive.</summary>
        public IEnumerable<SceneCollection> openAdditiveCollections => SceneManager.settings.project.openAdditiveCollections.NonNull().Distinct();

        /// <summary>Gets the collection that is currently open.</summary>
        public SceneCollection openCollection => m_openCollection;

        #region Checks and scene list evaluation

        /// <summary>Checks if collection is open.</summary>
        /// <param name="additive">Checks both if null.</param>
        internal bool IsOpen(SceneCollection collection, bool? additive = null)
        {

            if (!collection || !collection.scenes.Any(IsOpen))
                return false;

            if (additive is null && !IsOpenNonAdditive() && !IsOpenAdditive())
                return false;

            else if (additive == false && !IsOpenNonAdditive())
                return false;

            else if (additive == true && !IsOpenAdditive())
                return false;

            if (!collection.scenes.Any(s => s.isOpenInHierarchy))
                return false;

            return true;

            bool IsOpenNonAdditive() => openCollection == collection;
            bool IsOpenAdditive() => openAdditiveCollections.Contains(collection);

        }

        /// <summary>Evaluate the scenes that would close.</summary>
        public IEnumerable<Scene> EvalScenesToClose(SceneCollection closeCollection = null, SceneCollection nextCollection = null, SceneCollection additiveCloseCollection = null)
        {

            var list = additiveCloseCollection
                ? additiveCloseCollection.scenes.Distinct().Where(s => !openCollection || !openCollection.Contains(s))
                : openScenes.Where(s => !openAdditiveCollections.Any(c => c.Contains(s)));

            list = list.Where(s => IsValid(s) && IsOpen(s) && NotLoadingScreen(s) && NotPersistent(s, closeCollection, nextCollection));

            if (SceneManager.settings.project.reverseUnloadOrderOnCollectionClose)
                list = list.Reverse();

            return list;

        }

        /// <summary>Evaluate the scenes that would open.</summary>
        public IEnumerable<Scene> EvalScenesToOpen(SceneCollection collection, bool openAll = false) =>
            collection
            ? collection.scenes.Distinct().Where(s => IsValid(s) && IsClosed(s) && CanOpen(s, collection, openAll))
            : Enumerable.Empty<Scene>();

        #endregion
        #region Open

        public SceneOperation Open(SceneCollection collection, bool openAll = false) =>
            Open(SceneOperation.Queue(), collection, openAll);

        /// <inheritdoc cref="Open(SceneCollection, bool)"/>
        internal SceneOperation Open(SceneOperation operation, SceneCollection collection, bool openAll = false)
        {

            if (IsOpen(collection))
                return SceneOperation.done;

            var scenesToOpen = EvalScenesToOpen(collection, openAll);
            var scenesToClose = EvalScenesToClose(nextCollection: collection);

            if (!scenesToOpen.Any() && !scenesToClose.Any())
            {
                Track(collection);
                SceneOperation.InvokeGlobalCallback(CollectionOpenEvent.GetPooled(collection)).StartCoroutine();
                return SceneOperation.done;
            }

            return operation.
                With(collection, true).
                TrackCollectionCallback(collection).
                Close(scenesToClose).
                Open(scenesToOpen);

        }

        /// <summary>Opens the collection without closing existing scenes.</summary>
        /// <param name="collection">The collection to open.</param>
        /// <param name="openAll">Specifies whatever all scenes should open, regardless of open flag.</param>
        public SceneOperation OpenAdditive(SceneCollection collection, bool openAll = false)
        {

            if (!collection)
                return SceneOperation.done;

            if (m_openCollection == collection)
            {
                Debug.LogError("Cannot open collection as additive if it is already open normally.");
                return SceneOperation.done;
            }

            if (IsOpen(collection))
                return SceneOperation.done;

            var scenesToOpen = EvalScenesToOpen(collection, openAll);

            if (!scenesToOpen.Any())
            {
                Track(collection, isAdditive: true);
                SceneOperation.InvokeGlobalCallback(CollectionOpenEvent.GetPooled(collection)).StartCoroutine();
                return SceneOperation.done;
            }

            return SceneOperation.Queue().
                With(collection, collection.setActiveSceneWhenOpenedAsAdditive).
                TrackCollectionCallback(collection, true).
                WithoutLoadingScreen().
                Open(scenesToOpen);

        }

        /// <summary>Opens the collection without closing existing scenes.</summary>
        /// <remarks>No effect if no additive collections could be opened. Note that <paramref name="activeCollection"/> will be removed from <paramref name="collections"/> if it is contained within.</remarks>
        public SceneOperation OpenAdditive(IEnumerable<SceneCollection> collections, SceneCollection activeCollection = null, Scene loadingScene = null)
        {

            collections = collections.Where(c => !c.isOpen).Except(activeCollection).NonNull();

            if (!collections.Any())
                return SceneOperation.done;

            var operation = SceneOperation.Queue().
                With(activeCollection, activeCollection.setActiveSceneWhenOpenedAsAdditive).
                With(loadingScene: loadingScene, useLoadingScene: loadingScene).
                Open(collections.SelectMany(c => c.scenes.
                    Distinct().
                    Where(IsValid).
                    Where(IsClosed).
                    Where(s => CanOpen(s, c, false))));

            if (activeCollection)
                operation.TrackCollectionCallback(activeCollection, true);

            return operation;

        }

        #endregion
        #region Close

        /// <summary>Closes <paramref name="collection"/>.</summary>
        public SceneOperation Close(SceneCollection collection) =>
            Close(SceneOperation.Queue(), collection);

        /// <inheritdoc cref="Close(SceneCollection)"/>
        public SceneOperation Close(SceneOperation operation, SceneCollection collection)
        {

            if (!collection)
                return SceneOperation.done;

            var scenes = EvalScenesToClose(collection, additiveCloseCollection: collection.isOpenAdditive ? collection : null);

            if (!scenes.Any())
            {
                if (collection.isOpen)
                {
                    SceneOperation.InvokeGlobalCallback(CollectionOpenEvent.GetPooled(collection)).StartCoroutine();
                    Untrack(collection, isAdditive: collection.isOpenAdditive);
                }
                return SceneOperation.done;
            }

            return operation.
                With(collection, isCloseOperation: true).
                UntrackCollectionCallback(collection, collection.isOpenAdditive).
                Close(scenes);

        }

        #endregion
        #region Toggle

        public SceneOperation ToggleOpen(SceneCollection collection, bool openAll = false) =>
            IsOpen(collection)
            ? Close(collection)
            : Open(collection, openAll);

        #endregion
        #region Preload

        public SceneOperation Preload(SceneCollection collection, bool openAll = false) =>
            PreloadInternal(collection, openAll, isAdditive: false);

        public SceneOperation PreloadAdditive(SceneCollection collection, bool openAll = false) =>
            PreloadInternal(collection, openAll, isAdditive: true);

        #endregion
        #region Reopen

        /// <summary>Reopens the collection.</summary>
        public SceneOperation Reopen(SceneCollection collection, bool openAll = false)
        {

            if (collection.isOpenAdditive)
            {
                Debug.LogError("Additive collections cannot currently be reopned. Please close and then open manually.");
                return SceneOperation.done;
            }

            var scenesToClose = collection.scenes.Distinct().Where(s => s.isOpen && !s.keepOpenWhenCollectionsClose);
            var scenesToOpen = (openAll ? collection.scenes.Distinct() : collection.scenesToAutomaticallyOpen).Where(s => !s.isOpen);

            return SceneOperation.Queue().
                With(collection, true).
                Close(scenesToClose).
                Open(scenesToOpen).
                UntrackCollectionCallback(collection).
                TrackCollectionCallback(collection);

        }

        #endregion

    }

}
