using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BubbleSoccer
{
    public class Crowd : MonoBehaviour
    {
        [SerializeField]
        private CrowdPeople[] crowdPeoples;

        [SerializeField]
        private Color[] headColor;

        [SerializeField]
        private Color[] bodyColor;

        //helpers
        private Color previousHeadColor, previousBodyColor;
        private Color currentHeadColor, currentBodyColor;
        private MaterialPropertyBlock propertyBlock;
        
        public void InitializeCrowd()
        {
            propertyBlock = new MaterialPropertyBlock();
            foreach (CrowdPeople people in crowdPeoples)
            {
                do
                {
                    currentHeadColor = headColor[Random.Range(0, headColor.Length)];
                } while (currentHeadColor == previousHeadColor);

                do
                {
                    currentBodyColor = bodyColor[Random.Range(0, bodyColor.Length)];
                } while (currentBodyColor == previousBodyColor);

                people.HeadMeshRenderer.GetPropertyBlock(propertyBlock, 0);
                propertyBlock.SetColor("_EmissionColor", currentHeadColor);
                people.HeadMeshRenderer.SetPropertyBlock(propertyBlock, 0);

                people.BodyMeshRenderer.GetPropertyBlock(propertyBlock, 0);
                propertyBlock.SetColor("_EmissionColor", currentBodyColor);
                people.BodyMeshRenderer.SetPropertyBlock(propertyBlock, 0);

                previousBodyColor = currentBodyColor;
                previousHeadColor = currentHeadColor;
            }
        }

        IEnumerator RandomCrowdPeopleCheer()
        {
            yield return null;
        }
    }
}
