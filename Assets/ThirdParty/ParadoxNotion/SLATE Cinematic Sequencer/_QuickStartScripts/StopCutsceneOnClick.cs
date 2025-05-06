using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Slate
{

    [AddComponentMenu("SLATE/Stop Cutscene On Click")]
    public class StopCutsceneOnClick : MonoBehaviour
    {

        public Cutscene cutscene;
        public Cutscene.StopMode stopMode;

        void OnMouseDown() {
            if ( cutscene == null ) {
                Debug.LogError("Cutscene is not provided", gameObject);
                return;
            }

            cutscene.Stop(stopMode);
        }

        void Reset() {
            gameObject.GetAddComponent<Collider>();
        }
    }
}