using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;

namespace BubbleSoccer
{
    public class Pathfinding : MonoBehaviour
    {

        //helpers
        private Grid grid;
        private Heap<Node> openSet;
        private List<Node> path;
        private HashSet<Node> closedSet;
        private Node startingNode;
        private Node targetNode;
        private Node currentNode;
        private Vector3[] waypoints;
        private bool pathSuccess;
        private int xDistance;
        private int yDistance;
        private int newMovementCostOfNeighbour;

        private void Awake()
        {
            grid = GetComponent<Grid>();
        }

        public void FindPath(PathRequest request, Action<PathResult> callBack)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            waypoints = new Vector3[0];
            pathSuccess = false;

            startingNode = grid.NodeFromWorldPoint(request.pathStart);
            targetNode = grid.NodeFromWorldPoint(request.pathEnd);

            if (startingNode.Walkable && targetNode.Walkable)
            {
                openSet = new Heap<Node>(grid.MaxSize);
                closedSet = new HashSet<Node>();

                openSet.Add(startingNode);
                while (openSet.Count > 0)
                {
                    currentNode = openSet.RemoveFirstItem();

                    closedSet.Add(currentNode);

                    if (currentNode == targetNode)
                    {
                        sw.Stop();

                        pathSuccess = true;
                        break;
                    }

                    foreach (Node neighbour in grid.GetNeighbours(currentNode))
                    {
                        if (!neighbour.Walkable || closedSet.Contains(neighbour))
                        {
                            continue;
                        }

                        newMovementCostOfNeighbour = currentNode.GCost + GetDistanceBetweenNodes(currentNode, neighbour) + neighbour.MovementPanalty;

                        if (newMovementCostOfNeighbour < neighbour.GCost || !openSet.Contains(neighbour))
                        {
                            neighbour.GCost = newMovementCostOfNeighbour;
                            neighbour.HCost = GetDistanceBetweenNodes(neighbour, targetNode);
                            neighbour.Parent = currentNode;

                            if (!openSet.Contains(neighbour))
                            {
                                openSet.Add(neighbour);
                            }
                            else
                            {
                                openSet.UpdateItem(neighbour);
                            }
                        }
                    }
                }

            }
            if (pathSuccess)
            {
                waypoints = RetracePath(startingNode, targetNode,request.pathEnd);
                pathSuccess = waypoints.Length > 0;
            }
            
            callBack(new PathResult(waypoints, pathSuccess, request.callBack));
        }

        private Vector3[] RetracePath(Node startNode, Node endNode,Vector3 LastPosition)
        {
            path = new List<Node>();

            currentNode = endNode;

            while (currentNode != startingNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.Parent;
            }
            Vector3[] wayPoints = SimplifyPath(path, LastPosition);
            Array.Reverse(wayPoints);
            return wayPoints;
        }

        private Vector3[] SimplifyPath(List<Node> path,Vector3 LastPosition)
        {
            List<Vector3> wayPoints = new List<Vector3>();
            Vector2 directionOld = Vector2.zero;

            wayPoints.Add(LastPosition);
            for (int i = 1; i < path.Count; i++)
            {
                Vector2 directionNew = new Vector2(path[i - 1].GridXPosition - path[i].GridXPosition, path[i - 1].GridYPosition - path[i].GridYPosition);
                if (directionNew != directionOld)
                {
                    wayPoints.Add(path[i].WorldPosition);
                }
                directionOld = directionNew;
            }
            return wayPoints.ToArray();
        }
        private int GetDistanceBetweenNodes(Node a, Node b)
        {
            xDistance = Mathf.Abs(a.GridXPosition - b.GridXPosition);
            yDistance = Mathf.Abs(a.GridYPosition - b.GridYPosition);

            if (xDistance > yDistance)
            {
                return 14 * yDistance + 10 * (xDistance - yDistance);
            }
            else
            {
                return 14 * xDistance + 10 * (yDistance - xDistance);
            }
        }
    }
}
