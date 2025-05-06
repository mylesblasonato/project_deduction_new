using AdvancedSceneManager.Core;

namespace AdvancedSceneManager.Models.Interfaces
{

    /// <summary>Defines members for openable assets.</summary>
    public interface IOpenable
    {

        /// <summary>Gets if this <see cref="IOpenable"/> is open.</summary>
        public bool isOpen { get; }

        /// <summary>Gets if this <see cref="IOpenable"/> is queued to be opened.</summary>
        public bool isQueued { get; }

        /// <summary>Opens this <see cref="IOpenable"/>.</summary>
        public SceneOperation Open();
        public void _Open();

        /// <summary>Reopens this <see cref="IOpenable"/>.</summary>
        public SceneOperation Reopen();
        public void _Reopen();

        /// <summary>Toggles this <see cref="IOpenable"/> open or closed.</summary>
        public SceneOperation ToggleOpen();
        public void _ToggleOpen();

        /// <summary>Closes this <see cref="IOpenable"/>.</summary>
        public SceneOperation Close();
        public void _Close();

    }

    /// <inheritdoc cref="IOpenable"/>
    public interface IOpenable<T> where T : IOpenable
    {

        /// <inheritdoc cref="IOpenable.Open"/>
        public SceneOperation Open(T model);
        public void _Open(T model);

        /// <inheritdoc cref="IOpenable.Reopen"/>
        public SceneOperation Reopen(T model);
        public void _Reopen(T model);

        /// <inheritdoc cref="IOpenable.ToggleOpen"/>
        public SceneOperation ToggleOpen(T model);
        public void _ToggleOpen(T model);

        /// <inheritdoc cref="IOpenable.Close"/>
        public SceneOperation Close(T model);
        public void _Close(T model);

    }

}
