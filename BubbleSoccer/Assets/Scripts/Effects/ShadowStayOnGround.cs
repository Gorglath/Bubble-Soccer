using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleSoccer
{
    public class ShadowStayOnGround : MonoBehaviour
    {
        [Header("Settings")]
        public Transform Parent;
        public Transform ShadowOwner;
        public Vector3 ParentOffset = new Vector3(0f, 0.01f, 0f);
        public LayerMask GroundLayerMask;

        //helpers
        private Ray ray;
        private RaycastHit hitInfo;



        void Update()
        {
            if (!Parent)
            {
                DestroyImmediate(gameObject);
            }
            else
            {
                ray = new Ray(Parent.position + Vector3.up * 10, -Vector3.up);
            }

            Parent.position = ShadowOwner.position;
            if (Physics.Raycast(ray, out hitInfo, 100f, GroundLayerMask))
            {
                // Positio
                transform.position = new Vector3(Parent.position.x, hitInfo.point.y + ParentOffset.y, Parent.position.z);
            }
            else
            {
                // If raycast not hitting (air beneath feet), position it far away
                transform.position = new Vector3(0f, 110f, 0f);
            }
        }
    }
}
