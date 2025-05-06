using System.Collections.Generic;
using UnityEngine;

namespace AdvancedSceneManager.Utility
{

    /// <summary>Contains helpers for dealing with multiple versions of unity.</summary>
    public static class UnityCompatibiltyHelper
    {

#if UNITY_2023_1_OR_NEWER
        /// <inheritdoc cref="Object.FindFirstObjectByType"/>
#else
        /// <inheritdoc cref="Object.FindObjectOfType"/>
#endif
        public static T FindFirstObjectByType<T>() where T : Object
        {
#if UNITY_2023_1_OR_NEWER
            return Object.FindFirstObjectByType<T>();
#else
            // Fallback for older versions
            return Object.FindObjectOfType<T>();
#endif
        }

#if UNITY_2023_1_OR_NEWER
        /// <inheritdoc cref="Object.FindObjectsByType"/>
#else
        /// <inheritdoc cref="Object.FindObjectsOfType"/>
#endif
        public static IEnumerable<T> FindObjectsByType<T>() where T : Object
        {
#if UNITY_2023_1_OR_NEWER
            // Use FindObjectsByType for Unity 2023.1.0 or newer
            return Object.FindObjectsByType<T>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#else
            // Fallback for older Unity versions
            return Object.FindObjectsOfType<T>();
#endif
        }

    }

}
