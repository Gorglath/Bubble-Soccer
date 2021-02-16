using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleSoccer
{
    enum CornerNeighbours
    {
        None,
        One,
        Two,
        Three,
        Four
    }
    public class LevelArranger : MonoBehaviour
    {
        [SerializeField]
        private LevelElementsGenerator levelElementsGenerator;

        [SerializeField]
        private Grid grid;

        [SerializeField]
        private LayerMask panelLayerMask;

        [SerializeField]
        private float distanceToMoveForPanelDetection = 2f;

        [SerializeField]
        private GameObject[] twoNeigborsCorner, ThreeNeighborsCorner, FourNeighborsCorner;

        [SerializeField]
        private GameObject crowdPrefab;

        [SerializeField]
        private GameObject forwardCrowd;

        [SerializeField]
        private Material groundMaterial;
        //helpers
        private Ray ray;
        private CornerNeighbours cornerNeighbours;
        private TileSet tileSet;
        private PanelData panelDataSwitchPlaceHolder;
        private List<PanelData> newPanelDatas = new List<PanelData>();
        private List<GameObject> crowds = new List<GameObject>();
        private GameObject panelPlaceHolder;
        private GameObject cornerToReturn;
        private GameObject crowdPlaceHolder;
        private Transform MostLeftPanel;
        private Transform MostRightPanel;
        private Transform MostUpPanel;
        private Transform MostDownPanel;
        private Vector3 crowdPosition;
        private Vector3 panelPosition;
        private Vector3 cornerRotation;
        private bool detectedWest = false, detectedSouth = false, detectedNorth = false, detectedEast = false;
        private bool detectedWestNorth = false, detectedWestSouth = false, detectedEastNorth = false, detectedEastSouth = false;
        private bool placeHolder = false;
        private bool invertZScale = false;
        private float panelYOffset = 0f;
        private float distanceToCoverWithCrowd = 0f;
        private int numberOfSuccesfulDetections = 0;
        private int currentLeftPanelIndexLocation = 0;
        private int currentRightPanelIndexLocation = 0;
        private int gridX = 0, gridZ = 0;
        private int numberOfCorners = 0;
        private int cornerNumberPlaceHolder = 0;
        public void ArrangeLevel(List<PanelData> panelDatas, TileSet tileSet)
        {
            newPanelDatas.Clear();
            groundMaterial.SetTextureOffset("_MainTex", new Vector2(UnityEngine.Random.Range(0f, 20f), UnityEngine.Random.Range(0f, 20f)));
            this.tileSet = tileSet;
            foreach (PanelData panelData in panelDatas)
            {
                AssignPanelDataVariables(panelData);
                AssignGameObjectForPanel(panelData);
            }

            SetCrowdLocation();
            levelElementsGenerator.SpawnElements(newPanelDatas, tileSet);

            Invoke("CreatePathfindingGrid", 0.1f);
        }
        private void AssignPanelDataVariables(PanelData panelData)
        {
            ResetScript();

            Vector3 startingPosition = new Vector3(panelData.Panel.transform.position.x,
                panelData.Panel.transform.position.y + panelData.Panel.transform.localScale.y, panelData.Panel.transform.position.z);

            detectedWest = DetectedPanel(startingPosition + Vector3.up + (Vector3.left * distanceToMoveForPanelDetection));
            detectedEast = DetectedPanel(startingPosition + Vector3.up + (Vector3.right * distanceToMoveForPanelDetection));
            detectedNorth = DetectedPanel(startingPosition + Vector3.up + (Vector3.forward * distanceToMoveForPanelDetection));
            detectedSouth = DetectedPanel(startingPosition + Vector3.up + (Vector3.back * distanceToMoveForPanelDetection));

            if (!detectedEast && !detectedNorth && !detectedSouth && !detectedWest)
            {
                Debug.LogError("Not Detecting any Neighbor Panels");
                return;
            }

            if (!detectedNorth && panelData.GoalPanel)
            {
                panelData.IsEnemyGoal = true;
            }
            else if (panelData.GoalPanel && !detectedSouth)
            {
                panelData.IsEnemyGoal = false;
            }
            numberOfSuccesfulDetections += CheckBool(detectedWest);
            numberOfSuccesfulDetections += CheckBool(detectedNorth);
            numberOfSuccesfulDetections += CheckBool(detectedSouth);
            numberOfSuccesfulDetections += CheckBool(detectedEast);

            switch (numberOfSuccesfulDetections)
            {
                case 1:
                    panelData.Panel = tileSet.StartingOrEnding;
                    panelData.Type = PanelType.OneNeighbor;
                    break;
                case 2:
                    if ((detectedWest && detectedEast) || (detectedNorth && detectedSouth))
                    {
                        panelData.Panel = tileSet.Bridge;
                        panelData.Type = PanelType.TwoNeighbors;
                        panelData.Bridge = true;
                    }
                    else
                    {
                        panelData.Panel = tileSet.TopLeft;
                        panelData.Type = PanelType.TwoNeighbors;
                    }
                    break;
                case 3:
                    panelData.Panel = tileSet.TopCenter;
                    panelData.Type = PanelType.ThreeNeighbors;
                    break;
                case 4:
                    if (panelData.BlockPanel)
                    {
                        panelData.Panel = tileSet.Block;
                        panelData.Type = PanelType.FourNeighbors;
                    }
                    else
                    {
                        panelData.Panel = tileSet.MiddleCenter;
                        panelData.Type = PanelType.FourNeighbors;
                    }
                    break;
            }
        }

        private void AssignGameObjectForPanel(PanelData panelData)
        {

            panelPlaceHolder = AssignCorners(panelData);
            if (invertZScale)
            {
                panelPlaceHolder.transform.localScale = new Vector3(panelPlaceHolder.transform.localScale.x
                                , panelPlaceHolder.transform.localScale.y, panelPlaceHolder.transform.localScale.z * -1);
            }
            // panelPlaceHolder = Instantiate(panelData.Panel, panelData.transform.position, Quaternion.identity, transform);
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            MeshRenderer panelMeshRenderer = panelPlaceHolder.GetComponent<MeshRenderer>();

            if (panelData.Type != PanelType.FourNeighbors || numberOfCorners > 0)
            {
                if (panelData.Type == PanelType.ThreeNeighbors && ((numberOfCorners > 0 && CheckIfNotRedHerring(panelData)) || numberOfCorners >= 3))
                {
                    panelMeshRenderer.GetPropertyBlock(propertyBlock, 1);
                    propertyBlock.SetColor("_Color", tileSet.FenceColor);
                    panelMeshRenderer.SetPropertyBlock(propertyBlock, 1);
                    panelMeshRenderer.GetPropertyBlock(propertyBlock, 0);
                    propertyBlock.SetColor("_LighterColor", tileSet.GroundLightColor);
                    panelMeshRenderer.SetPropertyBlock(propertyBlock, 0);

                    panelMeshRenderer.GetPropertyBlock(propertyBlock, 0);
                    propertyBlock.SetColor("_DarkerColor", tileSet.GroundDarkColor);
                    panelMeshRenderer.SetPropertyBlock(propertyBlock, 0);
                }
                else
                {
                    panelMeshRenderer.GetPropertyBlock(propertyBlock, 0);
                    propertyBlock.SetColor("_Color", tileSet.FenceColor);
                    panelMeshRenderer.SetPropertyBlock(propertyBlock, 0);

                    panelMeshRenderer.GetPropertyBlock(propertyBlock, 1);
                    propertyBlock.SetColor("_LighterColor", tileSet.GroundLightColor);
                    panelMeshRenderer.SetPropertyBlock(propertyBlock, 1);

                    panelMeshRenderer.GetPropertyBlock(propertyBlock, 1);
                    propertyBlock.SetColor("_DarkerColor", tileSet.GroundDarkColor);
                    panelMeshRenderer.SetPropertyBlock(propertyBlock, 1);
                }
            }
            else
            {
                panelMeshRenderer.GetPropertyBlock(propertyBlock, 0);
                propertyBlock.SetColor("_LighterColor", tileSet.GroundLightColor);
                panelMeshRenderer.SetPropertyBlock(propertyBlock, 0);

                panelMeshRenderer.GetPropertyBlock(propertyBlock, 0);
                propertyBlock.SetColor("_DarkerColor", tileSet.GroundDarkColor);
                panelMeshRenderer.SetPropertyBlock(propertyBlock, 0);
            }
            panelDataSwitchPlaceHolder = panelPlaceHolder.GetComponent<PanelData>();
            panelDataSwitchPlaceHolder.Panel = panelPlaceHolder;

            CopyPanelData(panelDataSwitchPlaceHolder, panelData);

            CheckPanelPositionForGridSize(panelPlaceHolder.transform);

            AssignPanelRotation(panelPlaceHolder.transform, panelDataSwitchPlaceHolder);
            newPanelDatas.Add(panelDataSwitchPlaceHolder);
            Destroy(panelData.gameObject);
        }
        private void SetCrowdLocation()
        {
            if(crowds.Count <= 0)
            {
                for (int y = 0; y < forwardCrowd.transform.childCount - 1; y++)
                {
                    forwardCrowd.transform.GetChild(y).position += Vector3.forward * UnityEngine.Random.Range(0f, 0.8f) * UnityEngine.Random.Range(-1,1);
                }
            }

            forwardCrowd.transform.position = new Vector3(MostRightPanel.position.x + 
                (MostLeftPanel.position.x - MostRightPanel.position.x) / 2f
                , MostLeftPanel.position.y, MostUpPanel.position.z) + Vector3.forward * 4f;
            distanceToCoverWithCrowd = Vector3.Distance(MostUpPanel.position, MostDownPanel.position);

            forwardCrowd.GetComponent<Crowd>().InitializeCrowd();
            if (crowds.Count < Mathf.RoundToInt(distanceToCoverWithCrowd / 7 + 1) * 2)
            {

                crowdPlaceHolder = Instantiate(crowdPrefab, Vector3.zero, Quaternion.identity);
                crowdPlaceHolder.transform.localScale = new Vector3(crowdPlaceHolder.transform.localScale.x * -1
                    , crowdPlaceHolder.transform.localScale.y, crowdPlaceHolder.transform.localScale.z);
                crowds.Add(crowdPlaceHolder);
                for (int y = 0; y < crowdPlaceHolder.transform.childCount - 1; y++)
                {
                    crowdPlaceHolder.transform.GetChild(y).position += Vector3.forward * UnityEngine.Random.Range(0f, 0.8f);
                }

                crowdPlaceHolder = Instantiate(crowdPrefab, Vector3.zero, Quaternion.identity);
                crowds.Add(crowdPlaceHolder);
                for (int y = 0; y < crowdPlaceHolder.transform.childCount - 1; y++)
                {
                    crowdPlaceHolder.transform.GetChild(y).position += Vector3.forward * UnityEngine.Random.Range(0f, 0.8f);
                }

                currentLeftPanelIndexLocation = 0;
                currentRightPanelIndexLocation = 0;
                for (int i = 0; i < crowds.Count; i++)
                {
                    if (crowds[i].transform.localScale.x < 0f)
                    {
                        crowdPosition = new Vector3(MostLeftPanel.position.x, MostLeftPanel.position.y, MostDownPanel.position.z)
                           + Vector3.left * 3f + Vector3.forward * 3f + Vector3.forward * (7f * currentLeftPanelIndexLocation);
                        currentLeftPanelIndexLocation++;
                    }
                    else
                    {
                        crowdPosition = new Vector3(MostRightPanel.position.x, MostRightPanel.position.y, MostDownPanel.position.z)
                     + Vector3.right * 3f + Vector3.forward * 3f + Vector3.forward * (7f * currentRightPanelIndexLocation);
                        currentRightPanelIndexLocation++;
                    }
                    crowds[i].transform.position = crowdPosition;
                    crowds[i].SetActive(true);
                    crowds[i].GetComponent<Crowd>().InitializeCrowd();
                }
            }
            else if(crowds.Count > Mathf.RoundToInt(distanceToCoverWithCrowd / 7 + 1) * 2)
            {
                float numberOfRowsToSubstract = crowds.Count/2 - Mathf.RoundToInt(distanceToCoverWithCrowd / 7 + 1);
                for (int i = crowds.Count - 1; i < numberOfRowsToSubstract; i -= 2)
                {
                    crowds[i].SetActive(false);
                    crowds[i - 1].SetActive(false);
                }
            }
            else
            {
                currentLeftPanelIndexLocation = 0;
                currentRightPanelIndexLocation = 0;
                for (int i = 0; i < crowds.Count; i++)
                {
                    if (crowds[i].transform.localScale.x < 0f)
                    {
                        crowdPosition = new Vector3(MostLeftPanel.position.x, MostLeftPanel.position.y, MostDownPanel.position.z)
                           + Vector3.left * 3f + Vector3.forward * 3f + Vector3.forward * (7f * currentLeftPanelIndexLocation);
                        currentLeftPanelIndexLocation++;
                    }
                    else
                    {
                        crowdPosition = new Vector3(MostRightPanel.position.x, MostRightPanel.position.y, MostDownPanel.position.z)
                     + Vector3.right * 3f + Vector3.forward * 3f + Vector3.forward * (7f * currentRightPanelIndexLocation);
                        currentRightPanelIndexLocation++;
                    }
                    crowds[i].transform.position = crowdPosition;
                    crowds[i].SetActive(true);
                    crowds[i].GetComponent<Crowd>().InitializeCrowd();
                }
            }

        }
        private void CreatePathfindingGrid()
        {
            grid.transform.position = new Vector3(((MostLeftPanel.position.x - 0.5f) + (MostRightPanel.position.x + 0.5f)) / 2, 0.35f
                , ((MostDownPanel.position.z - 0.5f) + (MostUpPanel.position.z + 0.5f)) / 2);

            gridX = Mathf.RoundToInt((MostRightPanel.position.x + 0.5f) - (MostLeftPanel.position.x - 0.5f));
            gridZ = Mathf.RoundToInt((MostUpPanel.position.z + 0.5f) - (MostDownPanel.position.z - 0.5f));

            //if(gridX > gridZ)
            //{
            //    gridZ = gridX;
            //}
            //else
            //{
            //    gridX = gridZ;
            //}

            grid.GridWorldSize = new Vector2(gridX, gridZ);

            grid.CreateGrid();

        }
        private GameObject AssignCorners(PanelData panelData)
        {
            invertZScale = false;
            panelYOffset = 0f;
            Vector3 startingPosition = new Vector3(panelData.transform.position.x,
                panelData.transform.position.y, panelData.transform.position.z);
            cornerNumberPlaceHolder = CheckCorners(startingPosition, ref panelData);
            switch (panelData.Type)
            {
                case PanelType.TwoNeighbors:
                    if (cornerNumberPlaceHolder == 4 && !panelData.Bridge)
                    {
                        cornerToReturn = twoNeigborsCorner[0];
                    }
                    else
                    {
                        cornerToReturn = panelData.Panel;
                    }
                    break;
                case PanelType.ThreeNeighbors:
                    if (cornerNumberPlaceHolder == 3 || ((cornerNumberPlaceHolder == 1 || cornerNumberPlaceHolder == 2) && CheckIfNotRedHerring(panelData)))
                    {
                        if (!detectedEast && detectedWestSouth)
                        {
                            invertZScale = true;
                        }
                        else if (!detectedNorth && detectedEastSouth)
                        {
                            invertZScale = true;
                        }
                        else if (!detectedWest && detectedEastNorth)
                        {
                            invertZScale = true;
                        }
                        else if (!detectedSouth && detectedWestNorth)
                        {
                            invertZScale = true;
                        }
                        cornerToReturn = ThreeNeighborsCorner[0];
                    }
                    else if (cornerNumberPlaceHolder == 4)
                    {
                        cornerToReturn = ThreeNeighborsCorner[1];
                    }
                    else
                    {
                        cornerToReturn = panelData.Panel;
                    }
                    break;
                case PanelType.FourNeighbors:
                    if (!panelData.BlockPanel)
                    {
                        switch (cornerNumberPlaceHolder)
                        {
                            case 1:
                                cornerToReturn = FourNeighborsCorner[0];
                                if (detectedWestNorth)
                                {
                                    cornerRotation = new Vector3(0, 90, 0);
                                }
                                else if (detectedWestSouth)
                                {
                                    cornerRotation = new Vector3(0, 0, 0);
                                }
                                else if (detectedEastNorth)
                                {
                                    cornerRotation = new Vector3(0, 180, 0);
                                }
                                else if (detectedEastSouth)
                                {
                                    cornerRotation = new Vector3(0, 270, 0);
                                }
                                break;
                            case 2:
                                panelYOffset = -0.0027f;
                                cornerToReturn = FourNeighborsCorner[1];
                                if (detectedWestNorth)
                                {
                                    if (detectedWestSouth)
                                    {
                                        cornerRotation = new Vector3(0, 90, 0);
                                    }
                                    else if (detectedEastNorth)
                                    {
                                        cornerRotation = new Vector3(0, 180, 0);
                                    }
                                    else if (detectedEastSouth)
                                    {
                                        panelYOffset = 0f;
                                        cornerToReturn = FourNeighborsCorner[4];
                                        cornerRotation = new Vector3(0, 90, 0);
                                    }
                                }
                                else if (detectedWestSouth)
                                {
                                    if (detectedEastSouth)
                                    {
                                        cornerRotation = new Vector3(0, 0, 0);
                                    }
                                    else if (detectedEastNorth)
                                    {
                                        panelYOffset = 0f;
                                        cornerToReturn = FourNeighborsCorner[4];
                                        cornerRotation = new Vector3(0, 0, 0);
                                    }
                                }
                                else if (detectedEastNorth)
                                {
                                    if (detectedEastSouth)
                                    {
                                        cornerRotation = new Vector3(0, 270, 0);
                                    }
                                }
                                break;
                            case 3:
                                cornerToReturn = FourNeighborsCorner[2];
                                if (detectedWestNorth)
                                {
                                    if (detectedWestSouth)
                                    {
                                        if (detectedEastNorth)
                                        {
                                            cornerRotation = new Vector3(0, 90, 0);
                                        }
                                        else if (detectedEastSouth)
                                        {
                                            cornerRotation = new Vector3(0, 0, 0);
                                        }
                                    }
                                    else if (detectedEastNorth)
                                    {
                                        cornerRotation = new Vector3(0, 180, 0);
                                    }
                                }
                                else if (detectedWestSouth)
                                {
                                    cornerRotation = new Vector3(0, 270, 0);
                                }
                                break;
                            case 4:
                                cornerToReturn = FourNeighborsCorner[3];
                                break;
                            default:
                                cornerToReturn = panelData.Panel;
                                break;
                        }
                    }
                    else if (panelData.HolePanel)
                    {
                        panelData.DetectedEastNorth = detectedEastNorth;
                        panelData.DetectedEastSouth = detectedEastSouth;
                        panelData.DetectedWestNorth = detectedWestNorth;
                        panelData.DetectedWestSouth = detectedWestSouth;
                        numberOfCorners = cornerNumberPlaceHolder;
                        cornerToReturn = panelData.Panel;
                    }
                    break;
                default:
                    cornerToReturn = panelData.Panel;
                    break;
            }
            panelPosition = new Vector3(panelData.transform.position.x, panelData.transform.position.y + panelYOffset, panelData.transform.position.z);
            return Instantiate(cornerToReturn, panelPosition, Quaternion.Euler(cornerRotation), transform);
        }

        private bool CheckIfNotRedHerring(PanelData panelData)
        {
            if (!detectedSouth && (detectedEastNorth || detectedWestNorth))
            {
                return true;
            }
            else if (!detectedWest && (detectedEastNorth || detectedEastSouth))
            {
                return true;
            }
            else if (!detectedNorth && (detectedEastSouth || detectedWestSouth))
            {
                return true;
            }
            else if (!detectedEast && (detectedWestNorth || detectedWestSouth))
            {
                return true;
            }
            return false;
        }
        private void CheckPanelPositionForGridSize(Transform panel)
        {
            if (MostLeftPanel)
            {
                if (MostLeftPanel.position.x > panel.position.x)
                {
                    MostLeftPanel = panel;
                }
            }
            else
            {
                MostLeftPanel = panel;
            }

            if (MostRightPanel)
            {
                if (MostRightPanel.position.x < panel.position.x)
                {
                    MostRightPanel = panel;
                }
            }
            else
            {
                MostRightPanel = panel;
            }

            if (MostUpPanel)
            {
                if (MostUpPanel.position.z < panel.position.z)
                {
                    MostUpPanel = panel;
                }
            }
            else
            {
                MostUpPanel = panel;
            }

            if (MostDownPanel)
            {
                if (MostDownPanel.position.z > panel.position.z)
                {
                    MostDownPanel = panel;
                }
            }
            else
            {
                MostDownPanel = panel;
            }
        }

        private void CopyPanelData(PanelData copyInto, PanelData toCopy)
        {
            copyInto.SpringTrapPanel = toCopy.SpringTrapPanel;
            copyInto.TrapTriggerPanel = toCopy.TrapTriggerPanel;
            copyInto.SpringTrapPanel = toCopy.SpringTrapPanel;
            copyInto.BlockPanel = toCopy.BlockPanel;
            copyInto.Bridge = toCopy.Bridge;
            copyInto.SmallEnemyPanel = toCopy.SmallEnemyPanel;
            copyInto.BigEnemyPanel = toCopy.BigEnemyPanel;
            copyInto.MovementBoostPanel = toCopy.MovementBoostPanel;
            copyInto.PushBoostPanel = toCopy.PushBoostPanel;
            copyInto.ShieldBoostPanel = toCopy.ShieldBoostPanel;
            copyInto.ConePanel = toCopy.ConePanel;
            copyInto.PlayerPanel = toCopy.PlayerPanel;
            copyInto.GoalPanel = toCopy.GoalPanel;
            copyInto.IsEnemyGoal = toCopy.IsEnemyGoal;
            copyInto.BallPanel = toCopy.BallPanel;
            copyInto.SpikedBallTrapPanel = toCopy.SpikedBallTrapPanel;
            copyInto.ConePanel = toCopy.ConePanel;
            copyInto.HolePanel = toCopy.HolePanel;
            copyInto.Type = toCopy.Type;
            copyInto.DetectedWestSouth = toCopy.DetectedWestSouth;
            copyInto.DetectedWestNorth = toCopy.DetectedWestNorth;
            copyInto.DetectedEastSouth = toCopy.DetectedEastSouth;
            copyInto.DetectedEastNorth = toCopy.DetectedEastNorth;
            copyInto.NumberOfCorners = toCopy.NumberOfCorners;
        }
        private void AssignPanelRotation(Transform panelTransform, PanelData panelData)
        {
            switch (panelData.Type)
            {
                case PanelType.None:
                    break;
                case PanelType.OneNeighbor:
                    panelTransform.rotation = CheckForOneNeighbor();
                    panelTransform.position = new Vector3(panelTransform.position.x, panelTransform.position.y - 0.00295f, panelTransform.position.z);
                    break;
                case PanelType.TwoNeighbors:
                    if (panelData.Bridge)
                    {
                        panelTransform.position = new Vector3(panelTransform.position.x, panelTransform.position.y - 0.003f, panelTransform.position.z);
                    }
                    panelTransform.rotation = CheckForTwoNeighbors(panelData);
                    break;
                case PanelType.ThreeNeighbors:
                    panelTransform.rotation = CheckForThreeNeighbors();
                    panelTransform.position = new Vector3(panelTransform.position.x, panelTransform.position.y - 0.003f, panelTransform.position.z);
                    break;
            }
        }
        private int CheckBool(bool boolToCheck)
        {
            if (boolToCheck)
            {
                return 1;
            }

            return 0;
        }

        private Quaternion CheckForOneNeighbor()
        {
            if (detectedEast)
            {
                return Quaternion.Euler(0, 270, 0);
            }
            else if (detectedNorth)
            {
                return Quaternion.Euler(0, 180, 0);
            }
            else if (detectedSouth)
            {
                return Quaternion.identity;
            }
            else if (detectedWest)
            {
                return Quaternion.Euler(0, 90, 0);
            }

            return Quaternion.Euler(180, 0, 0);
        }
        private Quaternion CheckForTwoNeighbors(PanelData panelData)
        {
            if (panelData.Bridge)
            {
                if (detectedEast && detectedWest)
                {
                    return Quaternion.Euler(0, 90, 0);
                }
                else
                {
                    return Quaternion.identity;
                }
            }
            else
            {
                if (detectedEast)
                {
                    if (detectedEast && detectedNorth)
                    {
                        return Quaternion.Euler(0, 270, 0);
                    }
                    else if (detectedEast && detectedSouth)
                    {
                        return Quaternion.Euler(0, 0, 0);
                    }
                }
                else if (detectedWest)
                {
                    if (detectedWest && detectedNorth)
                    {
                        return Quaternion.Euler(0, 180, 0);
                    }
                    else if (detectedWest && detectedSouth)
                    {
                        return Quaternion.Euler(0, 90, 0);
                    }
                }
            }

            return Quaternion.Euler(180, 0, 0);
        }
        private Quaternion CheckForThreeNeighbors()
        {

            if (!detectedEast)
            {
                return Quaternion.Euler(0, 180, 0);
            }
            else if (!detectedNorth)
            {
                return Quaternion.Euler(0, 90, 0);
            }
            else if (!detectedSouth)
            {
                return Quaternion.Euler(0, 270, 0);
            }
            else if (!detectedWest)
            {
                return Quaternion.identity;
            }

            return Quaternion.Euler(180, 0, 0);
        }
        private int CheckCorners(Vector3 startingPosition, ref PanelData panelData)
        {
            numberOfCorners = 0;
            detectedEastNorth = false;
            detectedEastSouth = false;
            detectedWestNorth = false;
            detectedWestSouth = false;
            placeHolder = DetectedPanel(startingPosition + Vector3.up * 3f + (((Vector3.left + Vector3.forward).normalized - Vector3.one * 0.2f) * distanceToMoveForPanelDetection));
            if (!placeHolder)
            {

                detectedWestNorth = true;
                panelData.DetectedWestNorth = true;
                numberOfCorners++;
            }
            placeHolder = DetectedPanel(startingPosition + Vector3.up * 3f + (((Vector3.left + Vector3.back).normalized - Vector3.one * 0.2f) * distanceToMoveForPanelDetection));
            if (!placeHolder)
            {
                detectedWestSouth = true;
                panelData.DetectedWestSouth = true;
                numberOfCorners++;
            }
            placeHolder = DetectedPanel(startingPosition + Vector3.up * 3f + (((Vector3.right + Vector3.forward).normalized - Vector3.one * 0.2f) * distanceToMoveForPanelDetection));
            if (!placeHolder)
            {
                panelData.DetectedEastNorth = true;
                detectedEastNorth = true;
                numberOfCorners++;
            }
            placeHolder = DetectedPanel(startingPosition + Vector3.up * 3f + (((Vector3.right + Vector3.back).normalized - Vector3.one * 0.2f) * distanceToMoveForPanelDetection));
            if (!placeHolder)
            {
                panelData.DetectedEastSouth = true;
                detectedEastSouth = true;
                numberOfCorners++;
            }

            switch (numberOfCorners)
            {
                case 0:
                    cornerNeighbours = CornerNeighbours.Four;
                    break;
                case 1:
                    cornerNeighbours = CornerNeighbours.Three;
                    break;
                case 2:
                    cornerNeighbours = CornerNeighbours.Two;
                    break;
                case 3:
                    cornerNeighbours = CornerNeighbours.One;
                    break;
                case 4:
                    cornerNeighbours = CornerNeighbours.None;
                    break;
            }
            placeHolder = false;
            panelData.NumberOfCorners = numberOfCorners;
            return numberOfCorners;
        }
        private void ResetScript()
        {
            numberOfSuccesfulDetections = 0;
            detectedEast = false;
            detectedWest = false;
            detectedNorth = false;
            detectedSouth = false;
        }
        private bool DetectedPanel(Vector3 direction)
        {
            Vector3 positionToCheck = direction;
            ray = new Ray(positionToCheck, Vector3.down);
            if (Physics.Raycast(ray, 100f, panelLayerMask))
            {
                return true;
            }

            return false;
        }

        private void OnDrawGizmos()
        {
            //ray = new Ray(new Vector3(15,1,3)  + ((Vector3.right + Vector3.back).normalized * distanceToMoveForPanelDetection),Vector3.down * 100f);
            //Gizmos.DrawRay(ray);
            //ray = new Ray(new Vector3(15, 1, 3)  + ((Vector3.right + Vector3.forward).normalized * distanceToMoveForPanelDetection), Vector3.down);
            //Gizmos.DrawRay(ray);
            //ray = new Ray(new Vector3(15, 1, 3)  + ((Vector3.left + Vector3.back).normalized * distanceToMoveForPanelDetection), Vector3.down * 100f);
            //Gizmos.DrawRay(ray);
            //ray = new Ray(new Vector3(15, 1, 3)  + ((Vector3.left + Vector3.forward).normalized * distanceToMoveForPanelDetection), Vector3.down * 100f);
            //Gizmos.DrawRay(ray);
        }
    }
}
