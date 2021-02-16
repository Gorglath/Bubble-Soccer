using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleSoccer
{
    public class Ball : MonoBehaviour
    {
        public bool Kicked = false;
        public GameObject BallHolder;
        public bool isCurrentlyHeld = true;
        public GameObject ShadowReference;

        [SerializeField]
        private BallTrail trail;

        [SerializeField]
        private ParticleSystem particles;

        [SerializeField]
        private GameObject goalPS;
        //helpers
        private Rigidbody rigidbody;
        private bool changingLevel = false;

        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
        }
        private void GameWon()
        {
            changingLevel = true;
            transform.parent = GameManager.Instance.LevelManager.transform;
            GameManager.Instance.WonLevel();
        }
        private void GameLost()
        {
            changingLevel = true;
            transform.parent = GameManager.Instance.LevelManager.transform;
            GameManager.Instance.LostLevel();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "EnemyGoal" && !changingLevel)
            {
                if (BallHolder)
                {
                    if (BallHolder.tag == "Player")
                    {
                        if (BallHolder.GetComponent<PlayerController>().IsHaveBall)
                        {
                            BallHolder.GetComponent<PlayerController>().KickBall();
                        }
                    }
                }
                Destroy(Instantiate(goalPS, other.ClosestPointOnBounds(transform.position), Quaternion.identity), 2f);
                GameWon();
            }
            if (other.tag == "PlayerGoal" && !changingLevel)
            {
                Destroy(Instantiate(goalPS, other.ClosestPointOnBounds(transform.position), Quaternion.identity), 2f);
                GameLost();
            }
            if (rigidbody.isKinematic)
            {
                return;
            }
            if(other.tag == "Player" && BallHolder != other.gameObject && !Kicked && !isCurrentlyHeld)
            {
                isCurrentlyHeld = true;
                BallHolder = other.gameObject;
                other.GetComponent<PlayerController>().PickUpBall(gameObject);
                particles.Stop();
                trail.GetComponentInChildren<TrailRenderer>().enabled = false;
            }
            else if(other.tag == "Enemy" && BallHolder != other.gameObject && !Kicked && !isCurrentlyHeld)
            {
                isCurrentlyHeld = true;
                BallHolder = other.gameObject;
                other.GetComponent<BaseEnemy>().PickUpBall(gameObject);
                particles.Stop();
                trail.GetComponentInChildren<TrailRenderer>().enabled = false;
            }
        }

        public void GotKicked()
        {
            trail.GetComponentInChildren<TrailRenderer>().enabled = true;
            trail.Kicked();
        }
        public void ResetKicked()
        {
            particles.Play();
            trail.ResetKicked();
            Kicked = false;
            isCurrentlyHeld = false;
            BallHolder = null;
        }
        private void OnDestroy()
        {
            DestroyImmediate(ShadowReference);
        }
    }
}
