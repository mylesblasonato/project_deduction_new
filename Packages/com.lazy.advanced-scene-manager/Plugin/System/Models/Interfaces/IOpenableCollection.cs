using AdvancedSceneManager.Core;

namespace AdvancedSceneManager.Models.Interfaces
{

    /// <summary>Defines members for openable collections.</summary>
    public interface IOpenableCollection : IOpenable
    {

        /// <summary>Opens the <see cref="IOpenableCollection"/> as additive.</summary>
        public SceneOperation OpenAdditive(bool openAll = false);
        public void _OpenAdditive();

    }

    /// <inheritdoc cref="IOpenableCollection" />
    public interface IOpenableCollection<T> : IOpenable<SceneCollection>
    {

        /// <summary>Opens the <see cref="IOpenableCollection"/> as additive.</summary>
        public SceneOperation OpenAdditive(T model, bool openAll = false);
        public void _OpenAdditive(T model);

    }

}
