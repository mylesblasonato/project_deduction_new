using System;
using AdvancedSceneManager.Callbacks.Events;
using AdvancedSceneManager.Models;

namespace AdvancedSceneManager.Core
{

    partial class Runtime
    {

        /// <summary>Occurs when a scene is opened.</summary>
        public event Action<Scene> sceneOpened;

        /// <summary>Occurs when a scene is closed.</summary>
        public event Action<Scene> sceneClosed;

        /// <summary>Occurs when a collection is opened.</summary>
        public event Action<SceneCollection> collectionOpened;

        /// <summary>Occurs when a collection is closed.</summary>
        public event Action<SceneCollection> collectionClosed;

        /// <summary>Occurs when a scene is preloaded.</summary>
        public event Action<Scene> scenePreloaded;

        /// <summary>Occurs when a previously preloaded scene is opened.</summary>
        public event Action<Scene> scenePreloadFinished;

        /// <summary>Occurs when the last user scene closes.</summary>
        /// <remarks> 
        /// <para>This usually happens by mistake, and likely means that no user code would run, this is your chance to restore to a known state (return to main menu, for example), or crash to desktop.</para>
        /// <para>Returning to main menu can be done like this:<code>SceneManager.app.Restart()</code></para>
        /// </remarks>
        public Action onAllScenesClosed;

        internal void OnAllScenesClosed() =>
            onAllScenesClosed?.Invoke();

        /// <inheritdoc cref="SceneOperation.RegisterCallback{TEventType}(EventCallback{TEventType}, When, string)"/>
        public void RegisterCallback<TEventType>(EventCallback<TEventType> callback, When when = When.Unspecified) where TEventType : SceneOperationEventBase, new() =>
            SceneOperation.RegisterGlobalCallback(callback, when);

        /// <inheritdoc cref="SceneOperation.UnregisterCallback{TEventType}(EventCallback{TEventType}, When, string)"/>
        public void UnregisterCallback<TEventType>(EventCallback<TEventType> callback, When when = When.Unspecified) where TEventType : SceneOperationEventBase, new() =>
            SceneOperation.UnregisterGlobalCallback(callback, when);

        internal void InvokeGlobalCallback<T>(T e) where T : SceneOperationEventBase, new() =>
            SceneOperation.InvokeGlobalCallback(e);

    }

}
