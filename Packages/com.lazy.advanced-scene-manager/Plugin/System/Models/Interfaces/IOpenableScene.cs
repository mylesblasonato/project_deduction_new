using AdvancedSceneManager.Core;

namespace AdvancedSceneManager.Models.Interfaces
{

    /// <summary>Defines methods for openable scenes.</summary>
    public interface IOpenableScene : IOpenable
    {

        /// <summary>Opens the <see cref="IOpenableScene"/> with the specified <paramref name="loadingScene"/>.</summary>
        public SceneOperation OpenWithLoadingScreen(Scene loadingScene);

        /// <summary>Closes the <see cref="IOpenableScene"/> with the specified <paramref name="loadingScene"/>.</summary>
        public SceneOperation CloseWithLoadingScreen(Scene loadingScene);

        /// <summary>Opens the <see cref="IOpenableScene"/> and activates it.</summary>
        public SceneOperation OpenAndActivate();
        public void _OpenAndActivate();

        /// <summary>Activates the <see cref="IOpenableScene"/>.</summary>
        public void Activate();
        public void _Activate();

    }

    /// <inheritdoc cref="IOpenableScene"/>
    public interface IOpenableScene<T> : IOpenable<T> where T : Scene
    {

        /// <inheritdoc cref="IOpenableScene.OpenWithLoadingScreen(Scene)"/>
        public SceneOperation OpenWithLoadingScreen(T scene, Scene loadingScene);

        /// <inheritdoc cref="IOpenableScene.CloseWithLoadingScreen(Scene)"/>
        public SceneOperation CloseWithLoadingScreen(T scene, Scene loadingScene);

        /// <inheritdoc cref="IOpenableScene.OpenAndActivate"/>
        public SceneOperation OpenAndActivate(T scene);
        public void _OpenAndActivate(T scene);

        /// <inheritdoc cref="IOpenableScene.Activate"/>
        public void Activate(T scene);
        public void _Activate(T scene);

    }

}
