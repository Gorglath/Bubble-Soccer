using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BubbleSoccer
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject pauseMenu;

        [SerializeField]
        private GameObject pauseButton;
        
        [SerializeField]
        private Texture[] levelNumbers;

        [SerializeField]
        private RawImage firstNumber, SecondNumber;

        [SerializeField]
        private GameObject[] enemyIndicator;

        [SerializeField]
        private GameObject indicatorFade;
        [SerializeField]
        private Color enemyInitialColor;
        [SerializeField]
        private float timeForEnemyIndicatorToReachMaxSize = 1f;

        [SerializeField]
        private float timeBetweenIndicatorsAnimation = 1f;
        //helpers
        private int numberOfCurrentEnemies = 0;
        public void PauseGame()
        {
            if (pauseMenu.activeSelf)
            {
                pauseButton.SetActive(true);
                pauseMenu.SetActive(false);
                Time.timeScale = 1f;
            }
            else
            {
                pauseButton.SetActive(false);
                pauseMenu.SetActive(true);
                Time.timeScale = 0f;
            }
        }
        
        public void UpdateLevelUI(int currentLevel)
        {
            ResetEnemyIndicators();
            if (currentLevel < 10)
            {
                if (currentLevel < 9)
                {
                    SecondNumber.gameObject.SetActive(false);
                    firstNumber.texture = levelNumbers[currentLevel + 1];
                }
                else
                {
                    SecondNumber.gameObject.SetActive(true);
                    firstNumber.texture = levelNumbers[1];
                    SecondNumber.texture = levelNumbers[0];
                }
            }
            else if (currentLevel < 20)
            {
                if (currentLevel % 10 < 9)
                {
                    firstNumber.texture = levelNumbers[1];
                    SecondNumber.texture = levelNumbers[currentLevel % 10 + 1];
                }
                else
                {
                    firstNumber.texture = levelNumbers[2];
                    SecondNumber.texture = levelNumbers[0];
                }
            }
            else if(currentLevel < 30)
            {
                if (currentLevel % 10 < 9)
                {
                    firstNumber.texture = levelNumbers[2];
                    SecondNumber.texture = levelNumbers[currentLevel % 10 + 1];
                }
            }
        }

        public void InitializeEnemyIndicators(int numberOfEnemies)
        {
            numberOfCurrentEnemies = numberOfEnemies;
            StartCoroutine("SpawnEnemyIndicators");
        }

        public void RemoveEnemyIndicator()
        {
            numberOfCurrentEnemies--;
           GameObject placeHolder = Instantiate(indicatorFade, enemyIndicator[numberOfCurrentEnemies].transform.position
                , Quaternion.identity, enemyIndicator[numberOfCurrentEnemies].transform);
            placeHolder.GetComponent<EnemyIndicatorFade>().parentIndicatorImage = enemyIndicator[numberOfCurrentEnemies].GetComponent<RawImage>();
        }

        private void ResetEnemyIndicators()
        {
            for (int i = 0; i < enemyIndicator.Length; i++)
            {
                enemyIndicator[i].SetActive(false);
            }
        }

        IEnumerator SpawnEnemyIndicators()
        {
            for (int i = 0; i < numberOfCurrentEnemies; i++)
            {
                float timerPlaceHolder = 0f;
                enemyIndicator[i].SetActive(true);
                enemyIndicator[i].GetComponent<RawImage>().color = enemyInitialColor;
                while (timerPlaceHolder < timeForEnemyIndicatorToReachMaxSize)
                {
                    timerPlaceHolder += Time.deltaTime;
                    enemyIndicator[i].transform.localScale = Vector3.Lerp(Vector3.one * 2f, Vector3.one, timerPlaceHolder / timeForEnemyIndicatorToReachMaxSize);
                    yield return null;
                }
                yield return new WaitForSeconds(timeBetweenIndicatorsAnimation);
            }
        }

    }
}
