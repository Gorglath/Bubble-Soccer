using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleSoccer
{
    public class SpringTrap : Trap
    {
        [SerializeField]
        private Transform topPlatform;


        [SerializeField]
        private float radiusToPush = 1f;


        [SerializeField]
        private float forceToPush = 10f;


        [SerializeField]
        private LayerMask enemyLayer;

        [SerializeField]
        private LayerMask playerLayer;

        //helpers
        private Animator animator;
        private bool CanReleaseSpring = true;
        protected override void InitializeTrap()
        {
            animator = GetComponent<Animator>();
        }
        public override void ActivateTrap()
        {
            ActivateSpring();
        }

        private void ActivateSpring()
        {
            if (CanReleaseSpring)
            {
                CanReleaseSpring = false;
                animator.SetBool("Activate", true);
                StartCoroutine("ReleaseSpring");
            }
        }
        IEnumerator ReleaseSpring()
        {
            float random;
            do
            {
                random = Random.Range(-1, 2);
            } while (random == 0);

            Vector3 direction = Vector3.right * 3 * random + Vector3.up * 5;
            Collider[] enemiesToPush = Physics.OverlapBox(topPlatform.position, (Vector3.one * radiusToPush) / 2,Quaternion.identity,enemyLayer);
            foreach (Collider enemy in enemiesToPush)
            {
                enemy.GetComponent<BaseEnemy>().springed = true;
                enemy.GetComponent<BaseEnemy>().GotPushed(direction, forceToPush);
                Destroy(enemy.gameObject, 1f);
            }
            Collider[] player = Physics.OverlapBox(topPlatform.position, (Vector3.one * radiusToPush) / 2,Quaternion.identity,playerLayer);
            if (player.Length > 0)
            {
                player[0].GetComponent<PlayerController>().IsSpringed = true;
                player[0].GetComponent<PlayerController>().GotPushed(direction, forceToPush);
               
            }

            yield return null;
        }

        public void ResetSpring()
        {
            CanReleaseSpring = true;
            animator.SetBool("Activate", false);
        }
    }
}
