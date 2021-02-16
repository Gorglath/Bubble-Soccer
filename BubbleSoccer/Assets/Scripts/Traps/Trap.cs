using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleSoccer
{
    public class Trap : MonoBehaviour
    {
        private void Start()
        {
            InitializeTrap();
        }
        protected virtual void InitializeTrap() { }
        public virtual void ActivateTrap() { }
    }
}
