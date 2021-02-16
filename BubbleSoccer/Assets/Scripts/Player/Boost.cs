using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleSoccer
{
    public enum BoostType
    {
        MovementBoost,
        ShieldBoost,
        PushingBoost
    }

    public class Boost : MonoBehaviour
    {

        public GameObject ShadowReference;

        public float BoostMultiplier = 1f;

        public BoostType Type;

        [SerializeField]
        private ParticleSystem particleSystemToAttract;

        [SerializeField]
        private ParticleSystem idleParticles;
        //helpers
        private bool appliedBoost = false;

        private void OnDestroy()
        {
            if (ShadowReference)
            {
                DestroyImmediate(ShadowReference);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.tag == "Player" && !appliedBoost)
            {
                appliedBoost = true;
                idleParticles.Stop();
                GetComponent<MeshRenderer>().enabled = false;
                Destroy(ShadowReference);
                other.GetComponent<PlayerController>().ApplyBoost(this,particleSystemToAttract);
            }
        }
    }

    
}
