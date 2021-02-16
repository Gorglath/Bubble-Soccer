using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleSoccer
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        public bool DebugMode = false;
        public bool LevelStarted = false;
        
        public LevelManager LevelManager;
        public UIManager UIManager;
        public Crowd LeftCrowd, RightCrowd;
        [SerializeField]
        private GameObject winPanel, losePanel, standbyPanel;
        //helpers
        private Animator playerAnimatorReference;
        private bool wonLevel, lostLevel; 
        private void Awake()
        {
            Instance = this;
            LevelManager.LoadNextLevel();
        }

        public void StartLevel()
        {
            ResetVariables();
            foreach (BaseEnemy enemy in FindObjectsOfType<BaseEnemy>())
            {
                enemy.InitializeEnemy();
            }
        }

        public void ResetVariables()
        {
            LevelStarted = true;
            wonLevel = false;
            lostLevel = false;
        }
        public void WonLevel()
        {
            if (!lostLevel)
            {
                wonLevel = true;
                Invoke("DelayWin", 1f);
                //Invoke("StopBall", 0.2f);
                playerAnimatorReference.SetBool("Running", false);
                playerAnimatorReference.SetBool("Won", true);
                foreach (BaseEnemy enemy in FindObjectsOfType<BaseEnemy>())
                {
                    enemy.GetComponent<Animator>().SetBool("Running", false);
                    enemy.GetComponent<Animator>().SetBool("Lost", true);
                }
                LevelStarted = false;
            }
        }

        public void LostLevel()
        {
            if (!wonLevel)
            {
                Time.timeScale = 1f;
                lostLevel = true;
                Invoke("DelayLose", 1f);
                //  Invoke("StopBall", 0.5f);
                playerAnimatorReference.SetBool("Running", false);
                playerAnimatorReference.SetBool("Lost", true);
                foreach (BaseEnemy enemy in FindObjectsOfType<BaseEnemy>())
                {
                    enemy.GetComponent<Animator>().SetBool("Running", false);
                    enemy.GetComponent<Animator>().SetBool("Won", true);
                }
                LevelStarted = false;
            }
        }

        public void SetPlayerAnimator(Animator animator)
        {
            playerAnimatorReference = animator;
        }

        //private void StopBall()
        //{
        //    GameObject.FindGameObjectWithTag("Ball").GetComponent<Rigidbody>().velocity = Vector3.zero;
        //}
        private void DelayWin()
        {
            winPanel.SetActive(true);
        }

        private void DelayLose()
        {
            losePanel.SetActive(true);
        }
    }

}
