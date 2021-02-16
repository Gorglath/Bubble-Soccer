using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleSoccer
{
    public enum PanelType
    {
        None,
        OneNeighbor,
        TwoNeighbors,
        ThreeNeighbors,
        FourNeighbors,
    }

    public class PanelData : MonoBehaviour
    {
        public GameObject Panel;

        public bool SpringTrapPanel;

        public bool SpikedBallTrapPanel;

        public bool TrapTriggerPanel;

        public bool BlockPanel;

        public bool Bridge;

        public bool SmallEnemyPanel;

        public bool BigEnemyPanel;

        public bool MovementBoostPanel;

        public bool ShieldBoostPanel;

        public bool PushBoostPanel;

        public bool ConePanel;

        public bool HolePanel;

        public bool PlayerPanel;

        public bool GoalPanel;

        public bool IsEnemyGoal;

        public bool BallPanel;

        public PanelType Type;

        public GameObject[] Corners;
        
        public bool DetectedWestNorth = false, DetectedWestSouth = false, DetectedEastNorth = false, DetectedEastSouth = false;
      
        public int NumberOfCorners = 0;
    }
}
