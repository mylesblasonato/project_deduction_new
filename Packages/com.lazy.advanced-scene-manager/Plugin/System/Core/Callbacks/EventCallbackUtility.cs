#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AdvancedSceneManager.Core;
using AdvancedSceneManager.Utility;

namespace AdvancedSceneManager.Callbacks.Events
{

    /// <summary>Provides utility functions for working with event callbacks.</summary>
    public static class EventCallbackUtility
    {

        [AttributeUsage(AttributeTargets.Class)]
        public class CalledForAttribute : Attribute
        {

            public When[] when { get; }

            public CalledForAttribute(params When[] when)
            {
                this.when = when;
            }

        }

        [AttributeUsage(AttributeTargets.Class)]
        public class InvokationOrderAttribute : Attribute
        {

            public int order { get; }

            public InvokationOrderAttribute(int order)
            {
                this.order = order;
            }

        }

        /// <summary>Enumerates all callback types.</summary>
        public static IEnumerable<Type> GetCallbackTypes() =>
            TypeUtility.FindSubclasses<SceneOperationEventBase>(includeAbstract: false);

        /// <summary>Releases the event callback instance back into the pool.</summary>
        public static void ReleaseBackToPool<TEventType>(TEventType e) where TEventType : SceneOperationEventBase, new() =>
            SceneOperationEventBase.Release(e);

        #region Is when applicable

        /// <inheritdoc cref="IsWhenApplicable(Type)"/>
        public static bool IsWhenApplicable<TEventType>() where TEventType : SceneOperationEventBase, new() =>
            IsWhenApplicable(typeof(TEventType));

        /// <summary>Gets if the specified callback event uses <see cref="When"/> enum.</summary>
        public static bool IsWhenApplicable(Type type)
        {
            if (type == null)
                return false; // If no type is given, When is not applicable

            var attribute = type.GetCustomAttribute<CalledForAttribute>();
            return attribute != null && !attribute.when.Contains(When.Unspecified);
        }

        #endregion
        #region Get invokation order

        /// <inheritdoc cref="GetInvokationOrder(Type)"/>
        public static int GetInvokationOrder<TEventType>() where TEventType : SceneOperationEventBase, new() =>
            GetInvokationOrder(typeof(TEventType));

        /// <summary>Gets the invokation order of the event callback type.</summary>
        public static int GetInvokationOrder(Type type)
        {
            var attribute = type?.GetCustomAttribute<InvokationOrderAttribute>();
            return attribute?.order ?? -1;
        }

        #endregion
        #region Register all

        /// <summary>Registers callback for all events.</summary>
        public static SceneOperation RegisterAllCallbacks(string key, SceneOperation operation, EventCallback<SceneOperationEventBase> callback, When when = When.Unspecified)
        {

            var types = GetCallbackTypes().ToArray();
            foreach (var type in types)
                operation.RegisterCallback(key, type, callback, when);

            return operation;

        }

        /// <summary>Registers callback for all events.</summary>
        public static void RegisterAllCallbacksGlobal(string key, EventCallback<SceneOperationEventBase> callback, When when = When.Unspecified)
        {

            var types = GetCallbackTypes().ToArray();
            foreach (var type in types)
                SceneOperation.RegisterGlobalCallback(key, type, callback, when);

        }

        /// <summary>Unregisters callback using <paramref name="key"/>.</summary>
        public static SceneOperation UnregisterCallback(string key, SceneOperation operation)
        {

            var types = GetCallbackTypes().ToArray();
            foreach (var type in types)
                operation.UnregisterCallback(key, null, null);

            return operation;

        }

        /// <summary>Unregisters callback using <paramref name="key"/>.</summary>
        public static void UnregisterCallbackGlobal(string key)
        {

            var types = GetCallbackTypes().ToArray();
            foreach (var type in types)
                SceneOperation.UnregisterGlobalCallback(key, null, null);

        }

        #endregion

    }

}
