using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BubbleSoccer
{
    public class Node : IHeapItem<Node>
    {
        public bool Walkable = true;
        public Vector3 WorldPosition;
        public int GridXPosition;
        public int GridYPosition;

        public int MovementPanalty;
        public int GCost;
        public int HCost;
        public Node Parent;

        //helpers
        private int heapIndex;
        private int compareValue;
        public Node(bool walkable, Vector3 worldPosition, int gridXPosition, int gridYPosition, int panalty)
        {
            MovementPanalty = panalty;
            GridXPosition = gridXPosition;
            GridYPosition = gridYPosition;
            WorldPosition = worldPosition;
            Walkable = walkable;
        }

        public int fcost
        {
            get
            {
                return GCost + HCost;
            }
        }

        public int HeapIndex
        {
            get
            {
                return heapIndex;
            }
            set
            {
                heapIndex = value;
            }
        }

        public int CompareTo(Node nodeToCompare)
        {
            compareValue = fcost.CompareTo(nodeToCompare.fcost);
            if (compareValue == 0)
            {
                compareValue = HCost.CompareTo(nodeToCompare.HCost);
            }

            return -compareValue;
        }
    }
}
