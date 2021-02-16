using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BubbleSoccer
{
    public class ParticleAtrractor : MonoBehaviour
    {
        public ParticleSystem ParticleSystemToAttract;


        [SerializeField]
        private float attractionSpeed = 2.0f;
        //helpers
        private Transform cacheTransform;

        void OnEnable()
        {
            cacheTransform = transform;
        }

        void LateUpdate()
        {
            if (!ParticleSystemToAttract)
            {
                enabled = false;
            }
            else
            {
                ParticleSystem.Particle[] particles =
                  new ParticleSystem.Particle[ParticleSystemToAttract.particleCount];

                ParticleSystemToAttract.GetParticles(particles);

                for (int i = 0; i < particles.Length; i++)
                {
                    ParticleSystem.Particle p = particles[i];

                    Vector3 particleWorldPosition = p.position;

                    Vector3 directionToTarget = (cacheTransform.position - particleWorldPosition).normalized;
                    Vector3 seekForce = (directionToTarget * attractionSpeed
                        * Mathf.Clamp(Vector3.Distance(cacheTransform.position, p.position), 2f, 4f)) * Time.deltaTime;

                    p.velocity += seekForce;

                    particles[i] = p;

                }

                ParticleSystemToAttract.SetParticles(particles, particles.Length);
            }
        }

        }
}
