using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleSoccer
{
    public class SpikedBallTrap : Trap
    {
        [SerializeField]
        private GameObject spikedBallPrefab;

        [SerializeField]
        private Transform ballInstantiatingLocation;

        [SerializeField]
        private LayerMask panelLayerMask;

        [SerializeField]
        private float ballShootForce = 1f;

        [SerializeField]
        private int maxNumberOfBallsToShoot = 3;

        //helpers
        private GameObject spikedBallPlaceHolder;
        private GameObject shadowGameobjectPlaceHolder;
        private Transform spikedBallsParent;
        private Vector3 shootingDirection;
        private Vector3 positionToCheck;
        private Vector3 newXPos;
        private Ray ray;
        private int currentNumberOfBallAlreadyReleased = 0;
        private int numberOfDetectionAttemps = 1;
        protected override void InitializeTrap()
        {
            spikedBallsParent = transform.parent.parent;
            shootingDirection = (ballInstantiatingLocation.forward + ballInstantiatingLocation.up);
            PositionTrap();
        }

        private void PositionTrap()
        {
            do
            {
                numberOfDetectionAttemps++;
            } while (DetectedPanel(transform.position + Vector3.right * 0.5f * numberOfDetectionAttemps) && numberOfDetectionAttemps <= 20);
            newXPos = transform.position + Vector3.right * 0.5f * numberOfDetectionAttemps;
            transform.position = new Vector3(newXPos.x, transform.position.y, transform.position.z);
        }
        public override void ActivateTrap()
        {
            ShootBall();
        }

        private void ShootBall()
        {
            if(currentNumberOfBallAlreadyReleased == maxNumberOfBallsToShoot)
            {
                return;
            }

            currentNumberOfBallAlreadyReleased++;

            spikedBallPlaceHolder = Instantiate(spikedBallPrefab, ballInstantiatingLocation.position,Quaternion.identity, spikedBallsParent);

            spikedBallPlaceHolder.GetComponent<Rigidbody>().AddForce(shootingDirection * ballShootForce,ForceMode.Impulse);

            shadowGameobjectPlaceHolder = Instantiate(GameManager.Instance.LevelManager.Tilesets[GameManager.Instance.LevelManager.CurrentTileSet].Shadow
                , transform.position, Quaternion.Euler(90, 0, 0));

            shadowGameobjectPlaceHolder.GetComponent<ShadowStayOnGround>().Parent = spikedBallPlaceHolder.transform.GetChild(0);
            shadowGameobjectPlaceHolder.GetComponent<ShadowStayOnGround>().ShadowOwner = spikedBallPlaceHolder.transform;
        }
        private bool DetectedPanel(Vector3 direction)
        {
            Vector3 positionToCheck = direction + Vector3.up;
            ray = new Ray(positionToCheck, Vector3.down);
            if (Physics.Raycast(ray, 100f, panelLayerMask))
            {
                return true;
            }

            return false;
        }
    }
}
