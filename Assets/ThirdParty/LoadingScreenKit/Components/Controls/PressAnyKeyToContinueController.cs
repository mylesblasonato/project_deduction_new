using System;
using AdvancedSceneManager;
using AdvancedSceneManager.Callbacks.Events;
using UnityEngine;
using UnityEngine.UIElements;
using LoadingScreenKit.Interfaces;
using System.Collections;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem;
#endif

namespace LoadingScreenKit.Components.Controls
{
    [UxmlElement]
    public partial class PressAnyKeyToContinueController : VisualElement, IController
    {
        public static readonly string ussClassName = "press-any-key";

#if ENABLE_INPUT_SYSTEM
        private IDisposable inputEventListener;
#endif
        private IVisualElementScheduledItem inputScheduler;

        public PressAnyKeyToContinueController()
        {
            AddToClassList(ussClassName);

            RegisterCallbackOnce<AttachToPanelEvent>(OnAttach);
            RegisterCallbackOnce<DetachFromPanelEvent>(OnDetach);
        }

        #region Unity Event Handlers

        private void OnAttach(AttachToPanelEvent evt)
        {
            // Prevent issues with preopened loading screens in editor, where the press any key would block right away.
            if (!SceneManager.app.isStartupFinished)
                return;

            SetEnabled(false);

            SceneManager.runtime?.RegisterCallback<LoadingScreenClosePhaseEvent>(OnLoadingScreenClose, When.Before);
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            SceneManager.runtime?.UnregisterCallback<LoadingScreenClosePhaseEvent>(OnLoadingScreenClose, When.Before);
            DisposeInputEventListener();
            StopScheduler();
        }

        #endregion

        private void OnLoadingScreenClose(LoadingScreenClosePhaseEvent evt)
        {
            bool continuePressed = false;

            SetEnabled(true);

            evt.WaitFor(check);

            IEnumerator check()
            {
                // Wait until opacity is full
                yield return new WaitUntil(() => resolvedStyle.opacity == 1);

                // Start Accepting input
#if ENABLE_INPUT_SYSTEM
                inputEventListener = InputSystem.onAnyButtonPress.CallOnce(_ => continuePressed = true);
#else
                inputScheduler = schedule.Execute(_ => continuePressed = CheckAnyInput()).Every(16).StartingIn(0);
#endif

                // Wait for press
                yield return new WaitUntil(() => continuePressed);
            }
        }

#if !ENABLE_INPUT_SYSTEM
        private bool CheckAnyInput() => 
            Input.anyKeyDown || IsMouseOrTouchInput() || IsJoystickButtonPressed();

        private bool IsMouseOrTouchInput() => 
            Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2) || Input.touchCount > 0;

        private bool IsJoystickButtonPressed()
        {
            string[] joysticks = Input.GetJoystickNames();

            if (joysticks.Length == 0 || string.IsNullOrEmpty(joysticks[0]))
            {
                return false;
            }

            for (int i = 1; i <= joysticks.Length; i++)
            {
                for (int button = 0; button < 20; button++)
                {
                    if (Input.GetKeyDown($"Joystick{i}Button{button}"))
                        return true;
                }
            }
            return false;
        }

#endif

        private void DisposeInputEventListener()
        {
#if ENABLE_INPUT_SYSTEM
            inputEventListener?.Dispose();
            inputEventListener = null;
#endif
        }

        private void StopScheduler()
        {
            inputScheduler?.Pause();
            inputScheduler = null;
        }
    }
}
