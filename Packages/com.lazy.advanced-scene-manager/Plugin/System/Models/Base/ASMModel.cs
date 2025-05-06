using AdvancedSceneManager.Core;
using AdvancedSceneManager.Models.Interfaces;
using AdvancedSceneManager.Models.Internal;
using AdvancedSceneManager.Utility;

namespace AdvancedSceneManager.Models
{

    public abstract class ASMModel : ASMModelBase, IOpenable, IPreloadable
    {

        #region IOpenable

        public abstract bool isOpen { get; }
        public abstract bool isQueued { get; }

        public abstract SceneOperation Open();
        public abstract SceneOperation Reopen();
        public abstract SceneOperation ToggleOpen();
        public abstract SceneOperation Close();

        public virtual void _Open() => SpamCheck.EventMethods.Execute(() => Open());
        public virtual void _Reopen() => SpamCheck.EventMethods.Execute(() => Reopen());
        public virtual void _ToggleOpen() => SpamCheck.EventMethods.Execute(() => ToggleOpen());
        public virtual void _Close() => SpamCheck.EventMethods.Execute(() => Close());

        #endregion
        #region IPreloadable

        public abstract bool isPreloaded { get; }

        public abstract SceneOperation Preload();
        public virtual SceneOperation FinishPreload() => SceneManager.runtime.FinishPreload();
        public virtual SceneOperation CancelPreload() => SceneManager.runtime.CancelPreload();

        public virtual void _Preload() => SpamCheck.EventMethods.Execute(() => Preload());
        public virtual void _FinishPreload() => SpamCheck.EventMethods.Execute(() => FinishPreload());
        public virtual void _CancelPreload() => SpamCheck.EventMethods.Execute(() => CancelPreload());

        #endregion

    }

}
