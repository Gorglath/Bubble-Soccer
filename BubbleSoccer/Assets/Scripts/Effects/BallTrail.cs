using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleSoccer
{
    public class BallTrail : MonoBehaviour
    {
        [SerializeField]
        private Transform ballPosition;

        [SerializeField]
        private Vector3 offsetFromBall;

        [SerializeField]
        private Color idleColor, kickedColor;


        //helpers
        private Transform cacheTransform;
        private TrailRenderer trail;
        private Rigidbody ballRigidbody;
        private Vector3 positionToLookAt;
        private void Awake()
        {
            cacheTransform = transform;
            trail = GetComponentInChildren<TrailRenderer>();
            ballRigidbody = ballPosition.GetComponent<Rigidbody>();
        }
        private void Update()
        {
            positionToLookAt = ballPosition.position + ballRigidbody.velocity;
            positionToLookAt.y = cacheTransform.position.y;
            cacheTransform.rotation = Quaternion.Slerp(cacheTransform.rotation,Quaternion.LookRotation(positionToLookAt),0.1f);
            cacheTransform.position = ballPosition.position + offsetFromBall;
        }

        public void Kicked()
        {
            trail.time = 0.3f;
            trail.endColor = kickedColor;
            trail.startColor = kickedColor;
        }

        public void ResetKicked()
        {
            trail.time = 0.6f;
            trail.endColor = idleColor;
            trail.startColor = idleColor;
        }
    }
}
