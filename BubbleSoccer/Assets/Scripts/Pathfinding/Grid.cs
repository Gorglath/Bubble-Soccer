using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BubbleSoccer
{
    public class Grid : MonoBehaviour
    {
        public Vector2 GridWorldSize;
        public float NodeRadius;
        public LayerMask UnwalkableMask;
        public TerrainType[] walkableRegions;
        public int obstacleProximityPenalty = 10;
        [SerializeField]
        private bool displayGridGizmos = false;

        //helpers
        private Node[,] grid;
        private Node playerNode;
        private List<Node> neighbours;
        private LayerMask walkableMask;
        private Vector3 worldButtomLeft;
        private Vector3 worldPoint;
        private float nodeDiameter;
        private float percentXforWorldPositionConvertion;
        private float percentZforWorldPositionConvertion;
        private Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();
        private int gridSizeX;
        private int gridSizeY;
        private int x;
        private int z;
        private int checkXNeighbours;
        private int checkYNeighbours;
        private int penaltyMin = int.MaxValue;
        private int penaltyMax = int.MinValue;
        private bool isNodeWalkable;
        private void Awake()
        {
            nodeDiameter = NodeRadius * 2;

            foreach (TerrainType terrainType in walkableRegions)
            {
                walkableMask.value |= terrainType.TerrainMask.value;
                walkableRegionsDictionary.Add((int)Mathf.Log(terrainType.TerrainMask.value, 2), terrainType.TerrainPenalty);
            }
        }

        public int MaxSize
        {
            get
            {
                return gridSizeX * gridSizeY;
            }
        }
        public void CreateGrid()
        {
            gridSizeX = Mathf.RoundToInt(GridWorldSize.x / nodeDiameter);
            gridSizeY = Mathf.RoundToInt(GridWorldSize.y / nodeDiameter);

            grid = new Node[gridSizeX, gridSizeY];
            worldButtomLeft = transform.position - Vector3.right * GridWorldSize.x / 2 - Vector3.forward * GridWorldSize.y / 2;

            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    worldPoint = worldButtomLeft + Vector3.right * (x * nodeDiameter + NodeRadius) + Vector3.forward * (y * nodeDiameter + NodeRadius);
                    isNodeWalkable = !(Physics.CheckSphere(worldPoint, NodeRadius, UnwalkableMask));
                    int movementPanalty = 0;

                    Ray ray = new Ray(worldPoint + Vector3.up * 150, Vector3.down);
                    RaycastHit hit;
                    
                    if (Physics.Raycast(ray, out hit, 200, walkableMask))
                    {
                        walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPanalty);
                    }
                    else
                    {
                        isNodeWalkable = false;
                    }

                    if (!isNodeWalkable)
                    {
                        movementPanalty += obstacleProximityPenalty;
                    }
                    grid[x, y] = new Node(isNodeWalkable, worldPoint, x, y, movementPanalty);
                }
            }
            BlurPenaltyMap(3);
        }

        private void BlurPenaltyMap(int blurSize)
        {
            int kernelSize = blurSize * 2 + 1;
            int kernalExtents = (kernelSize - 1) / 2;

            int[,] penaltiesHorizontalPass = new int[gridSizeX, gridSizeY];
            int[,] penaltiesVerticalPass = new int[gridSizeX, gridSizeY];

            for (int y = 0; y < gridSizeY; y++)
            {
                for (int x = -kernalExtents; x <= kernalExtents; x++)
                {
                    int sampleX = Mathf.Clamp(x, 0, kernalExtents);
                    penaltiesHorizontalPass[0, y] += grid[sampleX, y].MovementPanalty;
                }

                for (int x = 1; x < gridSizeX; x++)
                {
                    int removeIndex = Mathf.Clamp(x - kernalExtents - 1, 0, gridSizeX);
                    int addIndex = Mathf.Clamp(x + kernalExtents, 0, gridSizeX - 1);

                    penaltiesHorizontalPass[x, y] = penaltiesHorizontalPass[x - 1, y] - grid[removeIndex, y].MovementPanalty + grid[addIndex, y].MovementPanalty;
                }
            }

            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = -kernalExtents; y <= kernalExtents; y++)
                {
                    int sampleY = Mathf.Clamp(y, 0, kernalExtents);

                    penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass[x, sampleY];
                }

                int blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, 0] / (kernelSize * kernelSize));
                grid[x, 0].MovementPanalty = blurredPenalty;

                for (int y = 1; y < gridSizeY; y++)
                {
                    int removeIndex = Mathf.Clamp(y - kernalExtents - 1, 0, gridSizeY);
                    int addIndex = Mathf.Clamp(y + kernalExtents, 0, gridSizeY - 1);

                    penaltiesVerticalPass[x, y] = penaltiesVerticalPass[x, y - 1] - penaltiesHorizontalPass[x, removeIndex] + penaltiesHorizontalPass[x, addIndex];
                    blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, y] / (kernelSize * kernelSize));
                    grid[x, y].MovementPanalty = blurredPenalty;

                    if (blurredPenalty > penaltyMax)
                    {
                        penaltyMax = blurredPenalty;
                    }
                    if (blurredPenalty < penaltyMin)
                    {
                        penaltyMin = blurredPenalty;
                    }
                }
            }
        }
        public List<Node> GetNeighbours(Node node)
        {
            neighbours = new List<Node>();
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                    {
                        continue;
                    }
                    checkXNeighbours = node.GridXPosition + x;
                    checkYNeighbours = node.GridYPosition + y;

                    if (checkXNeighbours >= 0 && checkXNeighbours < gridSizeX && checkYNeighbours >= 0 && checkYNeighbours < gridSizeY)
                    {
                        neighbours.Add(grid[checkXNeighbours, checkYNeighbours]);
                    }
                }
            }

            return neighbours;
        }
        public Node NodeFromWorldPoint(Vector3 worldPosition)
        {
            percentXforWorldPositionConvertion = (worldPosition.x - transform.position.x) / GridWorldSize.x + 0.5f - (NodeRadius / GridWorldSize.x);
            percentZforWorldPositionConvertion = (worldPosition.z - transform.position.z) / GridWorldSize.y + 0.5f - (NodeRadius / GridWorldSize.y);


            percentXforWorldPositionConvertion = Mathf.Clamp01(percentXforWorldPositionConvertion);
            percentZforWorldPositionConvertion = Mathf.Clamp01(percentZforWorldPositionConvertion);


            x = Mathf.RoundToInt((gridSizeX - 1) * percentXforWorldPositionConvertion);
            z = Mathf.RoundToInt((gridSizeY - 1) * percentZforWorldPositionConvertion);

            return grid[x, z];
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(GridWorldSize.x, 1, GridWorldSize.y));

            if (grid != null && displayGridGizmos)
            {
                foreach (Node node in grid)
                {
                    Gizmos.color = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(penaltyMin, penaltyMax, node.MovementPanalty));

                    Gizmos.color = (node.Walkable) ? Gizmos.color : Color.red;
                    //Gizmos.DrawSphere(node.WorldPosition, NodeRadius);
                    Gizmos.DrawCube(node.WorldPosition, Vector3.one * (nodeDiameter));
                }
            }
        }

        [System.Serializable]
        public class TerrainType
        {
            public LayerMask TerrainMask;
            public int TerrainPenalty;
        }
    }
}
