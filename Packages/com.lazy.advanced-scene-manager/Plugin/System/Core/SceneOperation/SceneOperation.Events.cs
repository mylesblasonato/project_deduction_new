using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AdvancedSceneManager.Callbacks.Events;
using AdvancedSceneManager.Utility;

namespace AdvancedSceneManager.Core
{

    partial class SceneOperation
    {

        static readonly Dictionary<Type, List<(Delegate callback, When when, string key)>> globalRegisteredCallbacks = new();
        readonly Dictionary<Type, List<(Delegate callback, When when, string key)>> registeredCallbacks = new();

        #region Register

        /// <summary>Registers an event callback for when an event occurs in a operation.</summary>
        /// <typeparam name="TEventType">The event callback type.</typeparam>
        /// <param name="callback">The callback to be invoked.</param>
        /// <param name="when">Specifies that the callback should only be called either only for that time. If <see langword="null"/> then callback will be called both times. For events using <see cref="When.NotApplicable"/>, this is ignored.</param>
        /// <param name="key">Specifies a key that can be used to unregister, if needed. Optional.</param>
        public SceneOperation RegisterCallback<TEventType>(EventCallback<TEventType> callback, When when = When.Unspecified, string key = null) where TEventType : SceneOperationEventBase, new()
        {

            if (registeredCallbacks.ContainsKey(typeof(TEventType)))
                registeredCallbacks[typeof(TEventType)].Add((callback, when, key));
            else
                registeredCallbacks.Add(typeof(TEventType), new() { (callback, when, key) });

            return this;

        }

        /// <inheritdoc cref="RegisterCallback{TEventType}(EventCallback{TEventType}, When, string)"/>
        public static void RegisterGlobalCallback<TEventType>(EventCallback<TEventType> callback, When when = When.Unspecified, string key = null) where TEventType : SceneOperationEventBase, new()
        {

            if (globalRegisteredCallbacks.ContainsKey(typeof(TEventType)))
                globalRegisteredCallbacks[typeof(TEventType)].Add(((callback, when, key)));
            else
                globalRegisteredCallbacks.Add(typeof(TEventType), new() { (callback, when, key) });
        }

        internal SceneOperation RegisterCallback(string key, Type eventType, EventCallback<SceneOperationEventBase> callback, When when = When.Unspecified)
        {

            if (!typeof(SceneOperationEventBase).IsAssignableFrom(eventType) || eventType.IsAbstract)
                throw new ArgumentException(eventType.FullName + " cannot be used as event callback.");

            if (registeredCallbacks.ContainsKey(eventType))
                registeredCallbacks[eventType].Add((callback, when, key));
            else
                registeredCallbacks.Add(eventType, new() { (callback, when, key) });

            return this;

        }

        internal static void RegisterGlobalCallback(string key, Type eventType, EventCallback<SceneOperationEventBase> callback, When when = When.Unspecified)
        {

            if (!typeof(SceneOperationEventBase).IsAssignableFrom(eventType) || eventType.IsAbstract)
                throw new ArgumentException(eventType.FullName + " cannot be used as event callback.");

            if (globalRegisteredCallbacks.ContainsKey(eventType))
                globalRegisteredCallbacks[eventType].Add((callback, when, key));
            else
                globalRegisteredCallbacks.Add(eventType, new() { (callback, when, key) });

        }

        #endregion
        #region Unregister

        static bool MatchCallback((Delegate callback, When when, string key) c, Delegate callback, When when = When.Unspecified, string key = null)
        {

            if (string.IsNullOrEmpty(c.key))
                return c.callback.Method == callback.Method &&
                    c.callback.Target == callback.Target &&
                    c.when == when;
            else
                return c.key == key;

        }

        /// <summary>Unregisters a registered event callback.</summary>
        /// <typeparam name="TEventType">The event callback type.</typeparam>
        /// <param name="callback">The callback that was to be invoked.</param>
        public SceneOperation UnregisterCallback<TEventType>(EventCallback<TEventType> callback, When when = When.Unspecified, string key = null) where TEventType : SceneOperationEventBase, new()
        {
            if (registeredCallbacks.ContainsKey(typeof(TEventType)))
                registeredCallbacks[typeof(TEventType)].RemoveAll(c => MatchCallback(c, callback, when, key));
            return this;
        }

        /// <inheritdoc cref="UnregisterCallback{TEventType}(EventCallback{TEventType}, When)"/>
        public static void UnregisterGlobalCallback<TEventType>(EventCallback<TEventType> callback, When when = When.Unspecified, string key = null) where TEventType : SceneOperationEventBase, new()
        {
            if (globalRegisteredCallbacks.ContainsKey(typeof(TEventType)))
                globalRegisteredCallbacks[typeof(TEventType)].RemoveAll(c => MatchCallback(c, callback, when, key));
        }

        internal SceneOperation UnregisterCallback(string key, Type eventType, EventCallback<SceneOperationEventBase> callback, When when = When.Unspecified)
        {

            if (!typeof(SceneOperationEventBase).IsAssignableFrom(eventType) || eventType.IsAbstract)
                throw new ArgumentException(eventType.FullName + " cannot be used as event callback.");

            if (registeredCallbacks.ContainsKey(eventType))
                registeredCallbacks[eventType].RemoveAll(c => MatchCallback(c, callback, when, key));

            return this;

        }

        internal static void UnregisterGlobalCallback(string key, Type eventType, EventCallback<SceneOperationEventBase> callback, When when = When.Unspecified)
        {

            if (eventType is null)
                return;

            if (!typeof(SceneOperationEventBase).IsAssignableFrom(eventType) || eventType.IsAbstract)
                throw new ArgumentException(eventType.FullName + " cannot be used as event callback.");

            if (globalRegisteredCallbacks.ContainsKey(eventType))
                globalRegisteredCallbacks[eventType].RemoveAll(c => MatchCallback(c, callback, when, key));

        }

        #endregion
        #region Invoke

        internal static IEnumerator InvokeGlobalCallback<TEventType>(TEventType e, When when = When.Unspecified, SceneOperation operation = null, bool automaticallyReleaseEventInstanceBackToPool = true) where TEventType : SceneOperationEventBase, new()
        {

            e.operation = operation ?? done;
            e.when = when;

            var waitFor = new List<Func<IEnumerator>>();

            if (globalRegisteredCallbacks.TryGetValue(typeof(TEventType), out var globalCallbacks))
                foreach (var (callback, _when, _) in globalCallbacks.ToArray())
                {

                    if (EventCallbackUtility.IsWhenApplicable<TEventType>())
                        if (_when != When.Unspecified && _when != when)
                            continue;

                    callback.DynamicInvoke(e);
                    waitFor.AddRange(e.waitFor);

                }

            if (SceneManager.settings.project.allowLoadingScenesInParallel)
                yield return CoroutineUtility.WaitAll(waitFor.Select(c => c.Invoke().StartCoroutine()));
            else
                yield return CoroutineUtility.Chain(waitFor.Select(c => c.Invoke().StartCoroutine()));

            if (automaticallyReleaseEventInstanceBackToPool)
                EventCallbackUtility.ReleaseBackToPool(e);

        }

        internal IEnumerator InvokeCallback<TEventType>(TEventType e, When when = When.Unspecified, bool automaticallyReleaseEventInstanceBackToPool = true) where TEventType : SceneOperationEventBase, new()
        {

            if (!flags.HasFlag(SceneOperationFlags.EventCallbacks))
                yield break;

            e.operation = this;
            e.when = when;

            var waitFor = new List<Func<IEnumerator>>();

            if (globalRegisteredCallbacks.TryGetValue(typeof(TEventType), out var globalCallbacks))
                foreach (var (callback, _when, _) in globalCallbacks.ToArray())
                {

                    if (EventCallbackUtility.IsWhenApplicable<TEventType>())
                        if (_when != When.Unspecified && _when != when)
                            continue;

                    callback.DynamicInvoke(e);
                    waitFor.AddRange(e.waitFor); //Add coroutine

                }

            if (registeredCallbacks.TryGetValue(typeof(TEventType), out var callbacks))
                foreach (var (callback, _when, _) in callbacks.ToArray())
                {

                    if (EventCallbackUtility.IsWhenApplicable<TEventType>())
                        if (_when != When.Unspecified && _when != when)
                            continue;

                    callback.DynamicInvoke(e);
                    waitFor.AddRange(e.waitFor); //Add coroutine

                }

            //Debug.Log($"Invoke {typeof(TEventType).Name} ({when}): {waitFor.Count} callbacks.");
            if (SceneManager.settings.project.allowLoadingScenesInParallel)
                yield return CoroutineUtility.WaitAll(waitFor.Select(c => c.Invoke().StartCoroutine()));
            else
                yield return CoroutineUtility.Chain(waitFor.Select(c => c.Invoke().StartCoroutine()));

            if (automaticallyReleaseEventInstanceBackToPool)
                EventCallbackUtility.ReleaseBackToPool(e);

        }

        #endregion

    }

}
