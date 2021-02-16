using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleSoccer
{
    public class SpikedBall : MonoBehaviour
    {
        public GameObject ShadowReference;

        [SerializeField]
        private float pushForce = 10f;

        //helpers
        private PlayerController playerControllerPlaceHolder;
        private BaseEnemy baseEnemyPlaceHolder;
        private Rigidbody rigidbody;
        private Vector3 initialVelocity;
        private bool pushedPlayer = false;
        private bool startBuldozing = false;
        private bool alreadyInvokedStartBuldoze = false;
        private void OnEnable()
        {
            rigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (startBuldozing)
            {
                rigidbody.velocity = new Vector3(initialVelocity.x,rigidbody.velocity.y,initialVelocity.z);
            }
        }
        private void InitializeBuldozing()
        {
            startBuldozing = true;
            initialVelocity = rigidbody.velocity;
        }
        private void OnTriggerEnter(Collider other)
        {
            if(other.tag == "Player" && !pushedPlayer)
            {
                pushedPlayer = true;
                Destroy(other.transform.GetChild(1).gameObject);
                playerControllerPlaceHolder = other.GetComponent<PlayerController>();
                Vector3 newPos = (other.transform.position - transform.position).normalized;
                newPos.y = 0;
                playerControllerPlaceHolder.GotPushed(newPos, pushForce);
            }
            else if(other.tag == "Enemy" && other.GetComponent<BaseEnemy>())
            {
                if (!other.GetComponent<BaseEnemy>().Dead)
                {
                    Destroy(other.transform.GetChild(1).gameObject);
                    baseEnemyPlaceHolder = other.GetComponent<BaseEnemy>();
                    baseEnemyPlaceHolder.Dead = true;
                    Vector3 newPos = (other.transform.position - transform.position).normalized;
                    newPos.y = 0;
                    baseEnemyPlaceHolder.GotPushed(newPos, pushForce);
                }

            }
            else if(other.tag == "Untagged" && !alreadyInvokedStartBuldoze)
            {
                alreadyInvokedStartBuldoze = true;
                Invoke("InitializeBuldozing", 0.5f);
            }
        }
        private void OnDestroy()
        {
            DestroyImmediate(ShadowReference);
        }
    }
}
