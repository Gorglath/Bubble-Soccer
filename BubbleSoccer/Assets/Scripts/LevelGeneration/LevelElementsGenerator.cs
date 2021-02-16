using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleSoccer
{
    public class LevelElementsGenerator : MonoBehaviour
    {
        [SerializeField]
        private DeathFloor deathFloor;

        [SerializeField]
        private GameObject[] holeCorners;

        [SerializeField]
        private BallIndicator ballIndicator;
        //helpers
        private GameObject shadowGameobjectPlaceHolder;
        private GameObject gameObjectPlaceHolder;
        private GameObject cornerToReturn;
        private Vector3 cornerRotation;
        private Vector3 cornerPositionOffset;
        private TileSet currentTileSet;
        private List<GameObject> trapTriggers;
        private List<GameObject> springTraps = new List<GameObject>();
        private int enemyCounter = 0;
        public void SpawnElements(List<PanelData> panelDatas,TileSet tileSet)
        {
            currentTileSet = tileSet;
            enemyCounter = 0;
            foreach (PanelData panelData in panelDatas)
            {
                SortElement(panelData);
            }
            GameManager.Instance.UIManager.InitializeEnemyIndicators(enemyCounter);
        }

        private void SortElement(PanelData panelData)
        {
            if (panelData.SmallEnemyPanel || panelData.BigEnemyPanel)
            {
                enemyCounter++;
                if (panelData.BigEnemyPanel)
                {
                    gameObjectPlaceHolder = InstantiateObject(currentTileSet.BigEnemy, panelData, Quaternion.Euler(0,180,0), Vector3.up * 0.353f, true);
                    gameObjectPlaceHolder.transform.localScale += Vector3.one * 0.5f;
                    gameObjectPlaceHolder.GetComponent<BaseEnemy>().IsEnemyBig = true;

                    shadowGameobjectPlaceHolder = Instantiate(currentTileSet.Shadow, transform.position, Quaternion.Euler(90, 0, 0));

                    shadowGameobjectPlaceHolder.GetComponent<ShadowStayOnGround>().Parent = gameObjectPlaceHolder.transform.GetChild(3);
                    shadowGameobjectPlaceHolder.GetComponent<ShadowStayOnGround>().ShadowOwner = gameObjectPlaceHolder.transform;
                    shadowGameobjectPlaceHolder.transform.localScale += Vector3.one * 0.25f;


                }
                else
                {
                    gameObjectPlaceHolder = InstantiateObject(currentTileSet.SmallEnemy, panelData, Quaternion.Euler(0, 180, 0), Vector3.up * 0.353f, true);

                    shadowGameobjectPlaceHolder = Instantiate(currentTileSet.Shadow, transform.position, Quaternion.Euler(90, 0, 0));

                    shadowGameobjectPlaceHolder.GetComponent<ShadowStayOnGround>().Parent = gameObjectPlaceHolder.transform.GetChild(3);
                    shadowGameobjectPlaceHolder.GetComponent<ShadowStayOnGround>().ShadowOwner = gameObjectPlaceHolder.transform;
                }

                gameObjectPlaceHolder.GetComponent<BaseEnemy>().ShadowReference = shadowGameobjectPlaceHolder;
            }
            else if (panelData.SpringTrapPanel)
            {
                InstantiateObject(currentTileSet.SpringTrap, panelData, Quaternion.identity, Vector3.one * 0.21f, false);
                panelData.Panel.GetComponent<MeshRenderer>().enabled = false;
            }
            else if (panelData.SpikedBallTrapPanel)
            {
                gameObjectPlaceHolder = InstantiateObject(currentTileSet.SpikedBallTrap, panelData, Quaternion.identity, Vector3.zero, false);
                
            }
            else if (panelData.TrapTriggerPanel)
            {
                InstantiateObject(currentTileSet.TrapButton, panelData, Quaternion.identity, Vector3.one * 0.25f, false);
            }
            else if (panelData.MovementBoostPanel || panelData.ShieldBoostPanel || panelData.PushBoostPanel)
            {
                if (panelData.MovementBoostPanel)
                {
                    gameObjectPlaceHolder = InstantiateObject(currentTileSet.MovementBoost, panelData, Quaternion.identity, Vector3.up / 1.8f, true);
                }
                else if (panelData.ShieldBoostPanel)
                {
                    gameObjectPlaceHolder = InstantiateObject(currentTileSet.ShieldBoost, panelData, Quaternion.identity, Vector3.up / 1.8f, true);
                }
                else if (panelData.PushBoostPanel)
                {
                    gameObjectPlaceHolder = InstantiateObject(currentTileSet.PushBoost, panelData, Quaternion.identity, Vector3.up / 1.8f, true);
                }

                shadowGameobjectPlaceHolder = Instantiate(currentTileSet.Shadow, transform.position, Quaternion.Euler(90, 0, 0));

                shadowGameobjectPlaceHolder.GetComponent<ShadowStayOnGround>().Parent = gameObjectPlaceHolder.transform.GetChild(0);
                shadowGameobjectPlaceHolder.GetComponent<ShadowStayOnGround>().ShadowOwner = gameObjectPlaceHolder.transform;
                shadowGameobjectPlaceHolder.transform.localScale -= Vector3.one * 0.1f;


                gameObjectPlaceHolder.GetComponent<Boost>().ShadowReference = shadowGameobjectPlaceHolder;
            }
            else if (panelData.ConePanel)
            {
                InstantiateObject(currentTileSet.Cone, panelData, Quaternion.identity, Vector3.up * 0.21f, false);
            }
            else if (panelData.HolePanel)
            {
                GameObject holeReference = ApplyHoleCorners(panelData);
                MeshRenderer panelMeshRenderer = holeReference.GetComponent<MeshRenderer>();
                MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

                //if (!panelData.DetectedEastNorth && !panelData.DetectedEastSouth && !panelData.DetectedWestNorth && !panelData.DetectedWestSouth)
                //{
                    panelMeshRenderer.GetPropertyBlock(propertyBlock, 0);
                    propertyBlock.SetColor("_Color", currentTileSet.FenceColor);
                    panelMeshRenderer.SetPropertyBlock(propertyBlock, 0);

                    panelMeshRenderer.GetPropertyBlock(propertyBlock, 1);
                    propertyBlock.SetColor("_LighterColor", currentTileSet.GroundLightColor);
                    panelMeshRenderer.SetPropertyBlock(propertyBlock, 1);

                panelMeshRenderer.GetPropertyBlock(propertyBlock, 1);
                propertyBlock.SetColor("_DarkerColor", currentTileSet.GroundDarkColor);
                panelMeshRenderer.SetPropertyBlock(propertyBlock, 1);
                //}
                //else
                //{
                //    panelMeshRenderer.GetPropertyBlock(propertyBlock, 1);
                //    propertyBlock.SetColor("_Color", currentTileSet.FenceColor);
                //    panelMeshRenderer.SetPropertyBlock(propertyBlock, 1);

                //    panelMeshRenderer.GetPropertyBlock(propertyBlock, 0);
                //    propertyBlock.SetColor("_Color", currentTileSet.GroundColor);
                //    panelMeshRenderer.SetPropertyBlock(propertyBlock, 0);
                //}

                panelData.gameObject.GetComponent<MeshRenderer>().enabled = false;
                panelData.gameObject.GetComponent<BoxCollider>().enabled = false;
            }
            else if (panelData.PlayerPanel)
            {
                gameObjectPlaceHolder = InstantiateObject(currentTileSet.Player, panelData, Quaternion.identity, Vector3.up * 0.353f, false);

                shadowGameobjectPlaceHolder = Instantiate(currentTileSet.Shadow, transform.position, Quaternion.Euler(90, 0, 0));

                shadowGameobjectPlaceHolder.GetComponent<ShadowStayOnGround>().Parent = gameObjectPlaceHolder.transform.GetChild(3);
                shadowGameobjectPlaceHolder.GetComponent<ShadowStayOnGround>().ShadowOwner = gameObjectPlaceHolder.transform;

                gameObjectPlaceHolder.GetComponent<PlayerController>().ShadowReference = shadowGameobjectPlaceHolder;
            }
            else if (panelData.GoalPanel)
            {
                GameObject goalReference;
                if (panelData.IsEnemyGoal)
                {
                    goalReference = InstantiateObject(currentTileSet.Goal, panelData, Quaternion.Euler(0, -180, 0), Vector3.up * 0.15f, false);
                    goalReference.transform.position = new Vector3(panelData.Panel.transform.position.x, goalReference.transform.position.y, panelData.Panel.transform.position.z - 0.3f);
                    goalReference.tag = "EnemyGoal";
                }
                else
                {
                    goalReference = InstantiateObject(currentTileSet.Goal, panelData, Quaternion.identity, Vector3.up * 0.15f, false);
                    goalReference.transform.position = new Vector3(panelData.Panel.transform.position.x, goalReference.transform.position.y, panelData.Panel.transform.position.z + 0.3f);
                    goalReference.tag = "PlayerGoal";
                }
            }
            else if (panelData.BallPanel)
            {
                GameObject currentLevelBall = InstantiateObject(currentTileSet.Ball, panelData, Quaternion.identity, Vector3.up, false);
                deathFloor.currentLevelBall = currentLevelBall;
                ballIndicator.SetTarget(currentLevelBall.transform);
                shadowGameobjectPlaceHolder = Instantiate(currentTileSet.Shadow, transform.position, Quaternion.Euler(90, 0, 0));

                shadowGameobjectPlaceHolder.GetComponent<ShadowStayOnGround>().Parent = currentLevelBall.transform.GetChild(0);
                shadowGameobjectPlaceHolder.GetComponent<ShadowStayOnGround>().ShadowOwner = currentLevelBall.transform;
                shadowGameobjectPlaceHolder.transform.localScale -= Vector3.one * 0.1f;


                currentLevelBall.GetComponent<Ball>().ShadowReference = shadowGameobjectPlaceHolder;
            }
        }
        
        private GameObject ApplyHoleCorners(PanelData panelData)
        {
            switch (panelData.NumberOfCorners)
            {
                case 1:
                    cornerToReturn = holeCorners[0];
                    cornerPositionOffset = new Vector3(0, 0.0077f, 0);
                    if (panelData.DetectedWestNorth)
                    {
                        cornerRotation = new Vector3(90, 180, 0);
                    }
                    else if (panelData.DetectedWestSouth)
                    {
                        cornerRotation = new Vector3(90, 90, 0);
                    }
                    else if (panelData.DetectedEastNorth)
                    {
                        cornerRotation = new Vector3(90, 270, 0);
                    }
                    else if (panelData.DetectedEastSouth)
                    {
                        cornerRotation = new Vector3(90, 0, 0);
                    }
                    break;
                case 2:
                    cornerToReturn = holeCorners[1];
                    cornerPositionOffset = new Vector3(0, -0.0044f, 0);
                    if (panelData.DetectedWestNorth)
                    {
                        if (panelData.DetectedWestSouth)
                        {
                            cornerRotation = new Vector3(90, -90, 0);
                        }
                        else if (panelData.DetectedEastNorth)
                        {
                            cornerRotation = new Vector3(90, 0, 0);
                        }
                        else if (panelData.DetectedEastSouth)
                        {
                            cornerToReturn = holeCorners[4];
                            cornerPositionOffset = new Vector3(0, 0.0077f, 0);
                            cornerRotation = new Vector3(90, 0, 0);
                        }
                    }
                    else if (panelData.DetectedWestSouth)
                    {
                        if (panelData.DetectedEastSouth)
                        {
                            cornerRotation = new Vector3(90, 180, 0);
                        }
                        else if (panelData.DetectedEastNorth)
                        {
                            cornerToReturn = holeCorners[4];
                            cornerPositionOffset = new Vector3(0, 0.0077f, 0);
                            cornerRotation = new Vector3(90, 90, 0);
                        }
                    }
                    else if (panelData.DetectedEastNorth)
                    {
                        if (panelData.DetectedEastSouth)
                        {
                            cornerRotation = new Vector3(90, 90, 0);
                        }
                    }
                    break;
                case 3:
                    cornerToReturn = holeCorners[2];
                    cornerPositionOffset = new Vector3(0, 0.0077f, 0);
                    if (panelData.DetectedWestNorth)
                    {
                        if (panelData.DetectedWestSouth)
                        {
                            if (panelData.DetectedEastNorth)
                            {
                                cornerRotation = new Vector3(90, 180, 0);
                            }
                            else if (panelData.DetectedEastSouth)
                            {
                                cornerRotation = new Vector3(90, 90, 0);
                            }
                        }
                        else if (panelData.DetectedEastNorth)
                        {
                            cornerRotation = new Vector3(90, 270, 0);
                        }
                    }
                    else if (panelData.DetectedWestSouth)
                    {
                        cornerRotation = new Vector3(90, 0, 0);
                    }
                    break;
                case 4:
                    cornerPositionOffset = new Vector3(0, 0.0077f, 0);
                    cornerToReturn = holeCorners[3];
                    break;
                default:
                    cornerToReturn = currentTileSet.Hole;
                    cornerRotation = new Vector3(90, 0, 0);
                    cornerPositionOffset = new Vector3(0f, 0.0077f, 0f);
                    break;
            }
            return Instantiate(cornerToReturn, panelData.transform.position + cornerPositionOffset, Quaternion.Euler(cornerRotation), panelData.transform);
        }
        private GameObject InstantiateObject(GameObject objectToInstantiate, PanelData panelData,Quaternion rotationToApply,Vector3 offset,bool randomXY)
        {
            Vector3 startingPosition;
            if (randomXY)
            {
                startingPosition = new Vector3(panelData.Panel.transform.position.x/* + Random.Range(-0.1f, 0.1f)*/,
                      panelData.Panel.transform.position.y + offset.y, panelData.Panel.transform.position.z + Random.Range(-0.1f, 0.1f));
 }
            else
            {
                startingPosition = new Vector3(panelData.Panel.transform.position.x,
                      panelData.Panel.transform.position.y + offset.y, panelData.Panel.transform.position.z);
            }

            return Instantiate(objectToInstantiate, startingPosition, rotationToApply, panelData.transform);
        }
    }
}
