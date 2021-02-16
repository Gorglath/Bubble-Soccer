using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleSoccer
{
    public class Cone : MonoBehaviour
    {
        [SerializeField]
        private float forceToPushForward = 3f;

        //helpers
        private bool isActive = true;
        private Vector3 newPos;
        private void OnTriggerEnter(Collider other)
        {
            if(other.tag == "Player" && isActive)
            {
                isActive = false;
                newPos = (transform.position - other.transform.position).normalized;
                other.GetComponent<PlayerController>().GotPushed(newPos, forceToPushForward);


                newPos.y = transform.position.y + Vector3.up.y;
                
                GetComponent<MeshCollider>().isTrigger = false;
                GetComponent<Rigidbody>().isKinematic = false;
                GetComponent<Rigidbody>().AddForce(newPos * 3f, ForceMode.Impulse);

                Invoke("ResetConeActive", 1f);
            }
            else if(other.tag == "Enemy" && isActive)
            {
                isActive = false;
                newPos = (transform.position - other.transform.position).normalized;
                other.GetComponent<BaseEnemy>().GotPushed(newPos, forceToPushForward);

                newPos.y = transform.position.y + Vector3.up.y;

                GetComponent<MeshCollider>().isTrigger = false;
                GetComponent<Rigidbody>().isKinematic = false;
                GetComponent<Rigidbody>().AddForce(newPos * 3f, ForceMode.Impulse);

                Invoke("ResetConeActive", 1f);
            }
        }

        private void ResetConeActive()
        {
            isActive = true;
        }
    }
}
