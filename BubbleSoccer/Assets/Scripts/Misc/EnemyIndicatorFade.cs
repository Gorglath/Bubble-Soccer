using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BubbleSoccer
{
    public class EnemyIndicatorFade : MonoBehaviour
    {
        [HideInInspector]
        public RawImage parentIndicatorImage;
        [SerializeField]
        private float timeToCompleteAnimation = 1f;

        [SerializeField]
        private float timeToCompleteFade = 1f;

        [SerializeField]
        private Color enemyDeadColor;
        private void OnEnable()
        {
            StartCoroutine("FadeAnimation");
        }

        IEnumerator FadeAnimation()
        {
            float timer = 0f;
            while (timer <= timeToCompleteAnimation)
            {
                timer += Time.deltaTime;
                transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 2f, timer / timeToCompleteAnimation);
                yield return null;
            }
            parentIndicatorImage.color = Color.gray;
            timer = 0f;
            RawImage fadeImage = GetComponent<RawImage>();
            
            while (timer <= timeToCompleteFade)
            {
                timer += Time.deltaTime;
                fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, Mathf.Lerp(1, 0, timer / timeToCompleteFade));
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
