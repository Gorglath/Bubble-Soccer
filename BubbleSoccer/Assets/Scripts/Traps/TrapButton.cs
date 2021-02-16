using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleSoccer
{
    public class TrapButton : MonoBehaviour
    {

        //helpers
        private GameObject[] trapsPlaceHolder;
        private Trap connectedTrap;
        private float minDistancePlaceHolder = -1f;
        private bool pressedButton = false;
        private void Start()
        {
            Invoke("FindClosestTrap", 1f);
        }

        private void FindClosestTrap()
        {
            trapsPlaceHolder = GameObject.FindGameObjectsWithTag("Trap");

            foreach (GameObject trap in trapsPlaceHolder)
            {
                if(minDistancePlaceHolder < 0)
                {
                    minDistancePlaceHolder = Mathf.Abs(Vector3.Distance(trap.transform.position, transform.position));
                    connectedTrap = trap.GetComponent<Trap>();
                }
                else if(minDistancePlaceHolder > Mathf.Abs(Vector3.Distance(trap.transform.position, transform.position)))
                {
                    minDistancePlaceHolder = Mathf.Abs(Vector3.Distance(trap.transform.position, transform.position));
                    connectedTrap = trap.GetComponent<Trap>();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if((other.tag == "Player" || other.tag == "Enemy") && !pressedButton)
            {
                pressedButton = true;
                connectedTrap.ActivateTrap();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if ((other.tag == "Player" || other.tag == "Enemy"))
            {
                pressedButton = false;
            }
        }
    }
}
