#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using AdvancedSceneManager.Core;
using AdvancedSceneManager.Loading;
using AdvancedSceneManager.Models;
using AdvancedSceneManager.Models.Internal;
using AdvancedSceneManager.Utility;
using UnityEngine;
using static AdvancedSceneManager.Callbacks.Events.EventCallbackUtility;

namespace AdvancedSceneManager.Callbacks.Events
{

    #region Base

    /// <summary>The base class for all scene operation event callbacks.</summary>
    public abstract class SceneOperationEventBase
    {

        /// <summary>The operation that invoked this event callback.</summary>
        /// <remarks>Might be null in some circumstances.</remarks>
        public SceneOperation? operation { get; set; }

        /// <summary>Specifies when this event callback was invoked, before or after the action it represents. If applicable.</summary>
        public When when { get; set; }

        #region WaitFor

        /// <summary>A list of coroutines that <see cref="operation"/> should wait for. It will not proceed until all coroutines are done.</summary>
        /// <remarks>Note that all callbacks will be invoked, and all coroutines will be collected in a list, before ASM waits (and potentially invokes) for coroutines to complete.</remarks>
        public List<Func<IEnumerator>> waitFor { get; private set; } = new();

        /// <summary>Specifies a coroutine that the operation should wait for.</summary>
        /// <remarks>
        /// Note that all callbacks will be invoked, and all coroutines will be collected in a list, before ASM waits (and potentially invokes) for coroutines to complete.
        /// <br/><br/>
        /// Note that this <paramref name="coroutine"/> <b>will</b> be started immediately with this overload.
        /// </remarks>
        public void WaitFor(IEnumerator coroutine) => waitFor.Add(() => coroutine);

        /// <summary>Specifies a coroutine that the operation should wait for.</summary>
        /// <remarks>
        /// Note that all callbacks will be invoked, and all coroutines will be collected in a list, before ASM waits (and potentially invokes) for coroutines to complete.
        /// <br/><br/>
        /// Note that this <paramref name="coroutine"/> <b>will not</b> be started immediately with this overload.
        /// </remarks>
        public void WaitFor(Func<IEnumerator> coroutine) => waitFor.Add(coroutine);

        /// <summary>Specifies a coroutine that the operation should wait for.</summary>
        /// <remarks>
        /// Note that all callbacks will be invoked, and all coroutines will be collected in a list, before ASM waits (and potentially invokes) for coroutines to complete.
        /// <br/><br/>
        /// Note that this <paramref name="coroutine"/> <b>will</b> be started immediately with this overload.
        /// </remarks>
        public void WaitFor(GlobalCoroutine coroutine) => waitFor.Add(() => coroutine);

        /// <summary>Specifies a coroutine that the operation should wait for.</summary>
        /// <remarks>
        /// Note that all callbacks will be invoked, and all coroutines will be collected in a list, before ASM waits (and potentially invokes) for coroutines to complete.
        /// <br/><br/>
        /// Note that this <paramref name="coroutine"/> <b>will not</b> be started immediately with this overload.
        /// </remarks>
        public void WaitFor(Func<GlobalCoroutine> coroutine) => waitFor.Add(coroutine);

#if UNITY_6000_0_OR_NEWER

        /// <summary>Specifies a coroutine that the operation should wait for.</summary>
        /// <remarks>
        /// Note that all callbacks will be invoked, and all coroutines will be collected in a list, before ASM waits (and potentially invokes) for coroutines to complete.
        /// <br/><br/>
        /// Note that this <paramref name="coroutine"/> <b>will</b> be started immediately with this overload.
        /// </remarks>
        public void WaitFor(Awaitable awaitable) => waitFor.Add(() => awaitable);

        /// <summary>Specifies a coroutine that the operation should wait for.</summary>
        /// <remarks>
        /// Note that all callbacks will be invoked, and all coroutines will be collected in a list, before ASM waits (and potentially invokes) for coroutines to complete.
        /// <br/><br/>
        /// Note that this <paramref name="coroutine"/> <b>will not</b> be started immediately with this overload.
        /// </remarks>
        public void WaitFor(Func<Awaitable> awaitable) => waitFor.Add(awaitable);

#endif

        #endregion
        #region Pooling

        private static readonly Dictionary<Type, Queue<SceneOperationEventBase>> eventPools = new();

        /// <summary>Pooled event retrieval with automatic property assignment.</summary>
        public static T GetPooled<T>(params Action<T>[] propertySetters) where T : SceneOperationEventBase, new()
        {
            T evt;
            if (eventPools.TryGetValue(typeof(T), out var pool) && pool.Count > 0)
            {
                evt = (T)pool.Dequeue();
            }
            else
            {
                evt = new T();
            }

            // Apply property setters
            foreach (var setter in propertySetters)
            {
                setter(evt);
            }

            return evt;
        }

        /// <summary>Releases an event back into the pool for reuse.</summary>
        public static void Release(SceneOperationEventBase e)
        {
            e.Reset();

            if (!eventPools.TryGetValue(e.GetType(), out var pool))
            {
                pool = new Queue<SceneOperationEventBase>();
                eventPools[e.GetType()] = pool;
            }

            pool.Enqueue(e);
        }

        /// <summary>Reset event data to default values.</summary>
        protected virtual void Reset()
        {
            operation = null;
            when = default;
            waitFor.Clear();
        }

        #endregion
        #region ToString

        public override string ToString() =>
            ToString(extraData: null);

        protected virtual string ToString(ASMModelBase? extraData)
        {
            var extraStr = extraData ? $":{extraData} " : null;
            return $"[{GetType().Name}] ({when})" + extraStr;
        }

        #endregion

    }

    /// <summary>The base class for scene event callbacks.</summary>
    /// <remarks>See <see cref="SceneOpenEvent"/>, <see cref="SceneCloseEvent"/>.</remarks>
    public abstract class SceneEvent : SceneOperationEventBase
    {
        /// <summary>The scene that this event callback was invoked for.</summary>
        public Scene scene { get; set; } = null!;

        public override string ToString() =>
            ToString(scene);

        protected override void Reset() =>
            scene = null!;
    }

    /// <summary>The base class for scene phase event callbacks.</summary>
    /// <remarks>See <see cref="SceneClosePhaseEvent"/>, <see cref="SceneOpenPhaseEvent"/>, <see cref="ScenePreloadPhaseEvent"/>.</remarks>
    public abstract class ScenePhaseEvent : SceneOperationEventBase
    {
        /// <summary>The evaluated list of scenes to close.</summary>
        public IEnumerable<Scene> scenes { get; set; } = null!;

        protected override void Reset() =>
            scenes = null!;
    }

    /// <summary>The base class for collection event callbacks.</summary>
    /// <remarks>See <see cref="CollectionOpenEvent"/>, <see cref="CollectionCloseEvent"/>.</remarks>
    public abstract class CollectionEvent : SceneOperationEventBase
    {
        /// <summary>The collection that this event callback was invoked for.</summary>
        public SceneCollection collection { get; set; } = null!;

        public override string ToString() =>
            ToString(collection);

        protected override void Reset() =>
            collection = null!;
    }

    /// <summary>The base class for loading screen phase event callbacks.</summary>
    /// <remarks>See <see cref="LoadingScreenOpenPhaseEvent"/>, <see cref="LoadingScreenClosePhaseEvent"/>.</remarks>
    public abstract class LoadingScreenPhaseEvent : SceneOperationEventBase
    {
        /// <summary>The loading scene that was opened.</summary>
        /// <remarks>Will be <see langword="null"/>, if none was set.</remarks>
        public Scene? loadingScene { get; set; }

        /// <summary>The loading screen that <see cref="loadingScene"/> contained.</summary>
        /// <remarks>Will be <see langword="null"/>, if <see cref="loadingScene"/> was not set, was not valid, or could not be opened.</remarks>
        public LoadingScreen? openedLoadingScreen { get; set; }

        protected override void Reset()
        {
            loadingScene = null;
            openedLoadingScreen = null;
        }
    }

    #endregion
    #region Scene events

    /// <summary>Occurs when a scene is opened.</summary>
    /// <remarks>Called when: <see cref="When.Before"/>, <see cref="When.After"/>.</remarks>
    [CalledFor(When.Before, When.After), InvokationOrder(6)]
    public class SceneOpenEvent : SceneEvent
    {
        [Obsolete("Not recommended, use GetPooled() instead.")]
        public SceneOpenEvent()
        { }

        public static SceneOpenEvent GetPooled(Scene scene) => GetPooled<SceneOpenEvent>(e => e.scene = scene);
    }

    /// <summary>Occurs when a scene is preloaded.</summary>
    /// <remarks>Called when: <see cref="When.Before"/>, <see cref="When.After"/>.</remarks>
    [CalledFor(When.Before, When.After), InvokationOrder(9)]
    public class ScenePreloadEvent : SceneEvent
    {
        [Obsolete("Not recommended, use GetPooled() instead.")]
        public ScenePreloadEvent()
        { }

        public static ScenePreloadEvent GetPooled(Scene scene) => GetPooled<ScenePreloadEvent>(e => e.scene = scene);
    }

    /// <summary>Occurs when a scene is closed.</summary>
    /// <remarks>Called when: <see cref="When.Before"/>, <see cref="When.After"/>.</remarks>
    [CalledFor(When.Before, When.After), InvokationOrder(3)]
    public class SceneCloseEvent : SceneEvent
    {
        [Obsolete("Not recommended, use GetPooled() instead.")]
        public SceneCloseEvent()
        { }

        public static SceneCloseEvent GetPooled(Scene scene) => GetPooled<SceneCloseEvent>(e => e.scene = scene);
    }

    #endregion
    #region Collection callbacks

    /// <summary>Occurs when a collection is opened.</summary>
    /// <remarks>Called when: <see cref="When.Unspecified"/> (it will be ignored).</remarks>
    [CalledFor(When.Unspecified), InvokationOrder(7)]
    public class CollectionOpenEvent : CollectionEvent
    {
        [Obsolete("Not recommended, use GetPooled() instead.")]
        public CollectionOpenEvent()
        { }

        public static CollectionOpenEvent GetPooled(SceneCollection collection) => GetPooled<CollectionOpenEvent>(e => e.collection = collection);
    }

    /// <summary>Occurs when a collection is closed.</summary>
    /// <remarks>Called when: <see cref="When.Unspecified"/> (it will be ignored).</remarks>
    [CalledFor(When.Unspecified), InvokationOrder(4)]
    public class CollectionCloseEvent : CollectionEvent
    {
        [Obsolete("Not recommended, use GetPooled() instead.")]
        public CollectionCloseEvent()
        { }

        public static CollectionCloseEvent GetPooled(SceneCollection collection) => GetPooled<CollectionCloseEvent>(e => e.collection = collection);
    }

    #endregion
    #region Phase callbacks

    /// <summary>Occurs before operation has begun working, but after it has started.</summary>
    /// <remarks>Properties has not been frozen at this point, and can be changed.
    /// <br/><br/>
    /// Called when: <see cref="When.Unspecified"/> (it will be ignored).</remarks>
    [CalledFor(When.Unspecified), InvokationOrder(0)]
    public class StartPhaseEvent : SceneOperationEventBase
    {
        [Obsolete("Not recommended, use GetPooled() instead.")]
        public StartPhaseEvent()
        { }

#pragma warning disable CS0618 // Type or member is obsolete
        private static readonly StartPhaseEvent instance = new();
#pragma warning restore CS0618 // Type or member is obsolete
        public static StartPhaseEvent GetPooled() => instance;
    }

    /// <summary>Occurs when a loading screen is opened.</summary>
    /// <remarks>Called regardless if operation actually opens one or not.
    /// <br/><br/>
    /// Called when: <see cref="When.Before"/>, <see cref="When.After"/>.</remarks>
    [CalledFor(When.Before, When.After), InvokationOrder(1)]
    public class LoadingScreenOpenPhaseEvent : LoadingScreenPhaseEvent
    {
        [Obsolete("Not recommended, use GetPooled() instead.")]
        public LoadingScreenOpenPhaseEvent()
        { }

        public static LoadingScreenOpenPhaseEvent GetPooled(Scene? loadingScene, LoadingScreen? loadingScreen) =>
            GetPooled<LoadingScreenOpenPhaseEvent>(e => e.loadingScene = loadingScene, e => e.openedLoadingScreen = loadingScreen);
    }

    /// <summary>Occurs when operation starts and finishes closing scenes.</summary>
    /// <remarks>Called when: <see cref="When.Before"/>, <see cref="When.After"/>.</remarks>
    [CalledFor(When.Before, When.After), InvokationOrder(3)]
    public class SceneClosePhaseEvent : ScenePhaseEvent
    {
        [Obsolete("Not recommended, use GetPooled() instead.")]
        public SceneClosePhaseEvent()
        { }

        public static SceneClosePhaseEvent GetPooled(IEnumerable<Scene> scenes) => GetPooled<SceneClosePhaseEvent>(e => e.scenes = scenes);
    }

    /// <summary>Occurs when operation starts and finishes opening scenes.</summary>
    /// <remarks>Called when: <see cref="When.Before"/>, <see cref="When.After"/>.</remarks>
    [CalledFor(When.Before, When.After), InvokationOrder(5)]
    public class SceneOpenPhaseEvent : ScenePhaseEvent
    {
        [Obsolete("Not recommended, use GetPooled() instead.")]
        public SceneOpenPhaseEvent()
        { }

        public static SceneOpenPhaseEvent GetPooled(IEnumerable<Scene> scenes) => GetPooled<SceneOpenPhaseEvent>(e => e.scenes = scenes);
    }

    /// <summary>Occurs when operation starts and finishes preloading scenes.</summary>
    /// <remarks>Called when: <see cref="When.Before"/>, <see cref="When.After"/>.</remarks>
    [CalledFor(When.Before, When.After), InvokationOrder(8)]
    public class ScenePreloadPhaseEvent : ScenePhaseEvent
    {
        [Obsolete("Not recommended, use GetPooled() instead.")]
        public ScenePreloadPhaseEvent()
        { }

        public static ScenePreloadPhaseEvent GetPooled(IEnumerable<Scene> scenes) => GetPooled<ScenePreloadPhaseEvent>(e => e.scenes = scenes);
    }

    /// <summary>Occurs when a loading screen is closed.</summary>
    /// <remarks>Called regardless if operation actually opens one or not.
    /// <br/><br/>
    /// Called when: <see cref="When.Before"/>, <see cref="When.After"/>.</remarks>
    [CalledFor(When.Before, When.After), InvokationOrder(10)]
    public class LoadingScreenClosePhaseEvent : LoadingScreenPhaseEvent
    {
        [Obsolete("Not recommended, use GetPooled() instead.")]
        public LoadingScreenClosePhaseEvent()
        { }

        public static LoadingScreenClosePhaseEvent GetPooled(Scene? loadingScene, LoadingScreen? loadingScreen) =>
            GetPooled<LoadingScreenClosePhaseEvent>(e => e.loadingScene = loadingScene, e => e.openedLoadingScreen = loadingScreen);
    }

    /// <summary>Occurs before operation has stopped working, but after its practially done.</summary>
    /// <remarks>Called when: <see cref="When.Unspecified"/> (it will be ignored).</remarks>
    [CalledFor(When.Unspecified), InvokationOrder(11)]
    public class EndPhaseEvent : SceneOperationEventBase
    {
        [Obsolete("Not recommended, use GetPooled() instead.")]
        public EndPhaseEvent()
        { }

#pragma warning disable CS0618 // Type or member is obsolete
        private static readonly EndPhaseEvent instance = new();
#pragma warning restore CS0618 // Type or member is obsolete
        public static EndPhaseEvent GetPooled() => instance;
    }

    #endregion

}
