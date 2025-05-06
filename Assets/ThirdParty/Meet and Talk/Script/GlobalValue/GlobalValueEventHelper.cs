using System.Collections;
using System.Collections.Generic;
using MeetAndTalk.GlobalValue;
using UnityEngine;

namespace MeetAndTalk.GlobalValue
{
    public class GlobalValueEventHelper : MonoBehaviour
    {
        public string GlobalValueName;
        GlobalValueManager manager;

        private void Awake()
        {
             manager = Resources.Load<GlobalValueManager>("GlobalValue");
        }

        public void Increase(string value)
        {
            manager.Set(GlobalValueName, GlobalValueOperations.Add, value);
        }

        public void Decrease(string value)
        {
            manager.Set(GlobalValueName, GlobalValueOperations.Subtract, value);

        }

        public void Set(string value)
        {
            manager.Set(GlobalValueName, value);
        }
    }


}
