using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BubbleSoccer
{
    public class LevelManager : MonoBehaviour
    {
        
        public int CurrentLevel = -1;
        public int CurrentTileSet = 0;
        public TileSet[] Tilesets;

        [SerializeField]
        private LevelGenerator levelGenerator;

        [SerializeField]
        private Texture2D[] levels;


        [SerializeField]
        private DeathFloor deathFloor;
        //helpers
        private bool readyToLoadNextLevel = false;
        private GameObject levelPanelsHolder;
        
        public void LoadNextLevel()
        {
            GameManager.Instance.LevelStarted = false;
            CurrentLevel++;
            if (CurrentLevel >= levels.Length)
            {
                CurrentLevel = -1;
                LoadNextLevelWithDifferentTileSet();
            }
            else
            {
                DeleteCurrenntLevel();
                Invoke("DelayLevelLoading", 0.02f);
            }
        }

        public void LoadPreviousLevel()
        {
            GameManager.Instance.LevelStarted = false;
            CurrentLevel--;
            if (CurrentLevel <= -1)
            {
                CurrentLevel = levels.Length;
                LoadPreviousLevelWithDifferentTileSet();
            }
            else
            {
                DeleteCurrenntLevel();
                Invoke("DelayLevelLoading", 0.02f);
            }
        }
        public void LoadPreviousLevelWithDifferentTileSet()
        {
            CurrentLevel--;

            if (CurrentLevel <= -1)
            {
                CurrentLevel = levels.Length - 1;
            }

            CurrentTileSet--;

            if (CurrentTileSet <= -1)
            {
                CurrentTileSet = Tilesets.Length - 1;
            }
            DeleteCurrenntLevel();
            Invoke("DelayLevelLoading", 0.02f);
        }
        public void LoadCurrentLevel()
        {
            DeleteCurrenntLevel();
            Invoke("DelayLevelLoading", 0.02f);
        }
        public void LoadNextLevelWithDifferentTileSet()
        {
            CurrentLevel++;

            if (CurrentLevel >= levels.Length)
            {
                CurrentLevel = 0;
            }
            
            CurrentTileSet++;
            
            if(CurrentTileSet >= Tilesets.Length)
            {
                CurrentTileSet = 0;
            }
            DeleteCurrenntLevel();
            Invoke("DelayLevelLoading", 0.02f);
        }
        private void DelayLevelLoading()
        {
            GameManager.Instance.UIManager.UpdateLevelUI(CurrentLevel);
            deathFloor.ChangingLevel = false;
            levelGenerator.GenerateLevel(levels[CurrentLevel], Tilesets[CurrentTileSet]);
        }
        public void DeleteCurrenntLevel()
        {
            if(transform.childCount > 0)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    Destroy(transform.GetChild(i).gameObject);
                }
            }

            if (GameObject.FindGameObjectWithTag("Ball"))
            {
                Destroy(GameObject.FindGameObjectWithTag("Ball"));
            }
        }
        
    }
}
