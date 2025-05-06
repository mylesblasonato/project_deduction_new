using System;
using System.Collections.Generic;
using System.Linq;
using AdvancedSceneManager.Models;
using AdvancedSceneManager.Utility;

namespace AdvancedSceneManager.Core
{

    partial class Runtime
    {

        void InitializeQueue()
        {
            QueueUtility<SceneOperation>.queueFilled += () => startedWorking?.Invoke();
            QueueUtility<SceneOperation>.queueEmpty += () =>
            {
                stoppedWorking?.Invoke();
                if (SceneUtility.unitySceneCount == 1 && FallbackSceneUtility.isOpen)
                    OnAllScenesClosed();
            };
        }

        /// <summary>Occurs when ASM has started working and is running scene operations.</summary>
        public event Action startedWorking;

        /// <summary>Occurs when ASM has finished working and no scene operations are running.</summary>
        public event Action stoppedWorking;

        /// <summary>Gets whatever ASM is busy with any scene operations.</summary>
        public bool isBusy => QueueUtility<SceneOperation>.isBusy;

        /// <summary>The currently running scene operations.</summary>
        public IEnumerable<SceneOperation> runningOperations =>
            QueueUtility<SceneOperation>.running;

        /// <summary>Gets the current scene operation queue.</summary>
        public IEnumerable<SceneOperation> queuedOperations =>
            QueueUtility<SceneOperation>.queue;

        /// <summary>Gets the current active operation in the queue.</summary>
        public SceneOperation currentOperation =>
            QueueUtility<SceneOperation>.queue.FirstOrDefault();

        /// <summary>Gets if this collection is currently queued to be opened.</summary>
        public bool IsQueued(SceneCollection collection) =>
            SceneManager.runtime.runningOperations.Concat(SceneManager.runtime.queuedOperations).Any(o => o.collection == collection && !o.isCollectionCloseOperation);

        /// <summary>Gets if this scene is queued to be opened.</summary>
        public bool IsQueued(Scene scene) =>
            SceneManager.runtime.runningOperations.Concat(SceneManager.runtime.queuedOperations).SelectMany(o => o.open.Concat(o.preload)).Contains(scene);

    }

}
