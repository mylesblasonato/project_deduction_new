using System;
using System.Collections.Generic;
using System.Linq;
using AdvancedSceneManager.Models;

namespace AdvancedSceneManager.Core
{

    partial class Runtime
    {

        void InitializeSceneLoaders()
        {
            AddSceneLoader<RuntimeSceneLoader>();

        }

        internal List<SceneLoader> sceneLoaders = new();

        /// <summary>Gets a list of all added scene loaders that can be toggled scene by scene.</summary>
        public IEnumerable<SceneLoader> GetToggleableSceneLoaders() =>
            sceneLoaders.Where(l => !l.isGlobal && !string.IsNullOrWhiteSpace(l.sceneToggleText));

        /// <summary>Gets the loader for <paramref name="scene"/>.</summary>
        public SceneLoader GetLoaderForScene(Scene scene)
        {
            SceneLoader globalLoader = null;

            foreach (var loader in sceneLoaders)
            {
                // skip if cant be activated
                if (!loader.canBeActivated)
                    continue;

                // return first found
                if (Match(loader, scene))
                    return loader;

                // Track global to use if we don't find a match
                if (globalLoader == null && loader.isGlobal && loader.CanHandleScene(scene))
                    globalLoader = loader;
            }

            return globalLoader;
        }

        /// <summary>Returns the scene loader with the specified key.</summary>
        public SceneLoader GetSceneLoader(string sceneLoader) =>
            sceneLoaders.FirstOrDefault(l => l.Key == sceneLoader);

        /// <summary>Returns the scene loader type with the specified key.</summary>
        public Type GetSceneLoaderType(string sceneLoader) =>
            GetSceneLoader(sceneLoader)?.GetType();

        bool Match(SceneLoader loader, Scene scene) =>
            loader.GetType().FullName == scene.sceneLoader && loader.CanHandleScene(scene);

        /// <summary>Adds a scene loader.</summary>
        public void AddSceneLoader<T>() where T : SceneLoader, new()
        {
            var key = SceneLoader.GetKey<T>();
            sceneLoaders.RemoveAll(l => l.Key == key);
            sceneLoaders.Add(new T());
        }

        /// <summary>Removes a scene loader.</summary>
        public void RemoveSceneLoader<T>() =>
            sceneLoaders.RemoveAll(l => l is T);

    }

}
