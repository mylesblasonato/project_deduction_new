using AdvancedSceneManager.Core;

namespace AdvancedSceneManager.Models.Interfaces
{

    /// <summary>Defines members related to preloading.</summary>
    public interface IPreloadable
    {

        /// <summary>Gets if this <see cref="IPreloadable"/> is currently preloaded.</summary>
        public bool isPreloaded { get; }

        /// <summary>Preloads this <see cref="IPreloadable"/>.</summary>
        public SceneOperation Preload();
        public void _Preload();

        /// <summary>Finishes the current preloads.</summary>
        /// <remarks>Global operation, finishes all preloads, not just this <see cref="IPreloadable"/>.</remarks>
        public SceneOperation FinishPreload();
        public void _FinishPreload();

        /// <summary>Cancels the current preloads.</summary>
        /// <remarks>Global operation, cancels all preloads, not just this <see cref="IPreloadable"/>.</remarks>
        public SceneOperation CancelPreload();
        public void _CancelPreload();

    }

}
