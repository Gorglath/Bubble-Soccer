using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleSoccer
{
    public class DeathFloor : MonoBehaviour
    {
        public bool ChangingLevel = false;
        public GameObject currentLevelBall;
        [SerializeField]
        private CameraController cameraController;
        private void GameLost()
        {
            ChangingLevel = true;
            currentLevelBall.transform.parent = GameManager.Instance.LevelManager.transform;
            GameManager.Instance.LostLevel();
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Enemy")
            {
                Destroy(other.gameObject);
            }
            else if (other.tag == "Player" && !ChangingLevel)
            {
                Destroy(other.gameObject,1f);
                cameraController.PlayerFell(other.transform.position);
                GameLost();
            }
        }
    }
}
