using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BubbleSoccer
{
    public class CrowdPeople : MonoBehaviour
    {
        [HideInInspector]
        public MeshRenderer HeadMeshRenderer;
        
        [HideInInspector]
        public MeshRenderer BodyMeshRenderer;

        //helpers
        private float timeToCompleteJump = 1f;
        private void Awake()
        {
            HeadMeshRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
            BodyMeshRenderer = transform.GetChild(1).GetComponent<MeshRenderer>();
        }
        public void Jump()
        {

        }

        IEnumerator DoTheJump()
        {
            while (true)
            {

            }
        }
    }
}
