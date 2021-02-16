using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleSoccer
{
    public class CustomPath
    {
        public readonly Vector3[] LookPoints;
        public readonly Line[] TurnBoundaries;
        public readonly int FinishLineIndex;
        public readonly int slowDownIndex;

        public CustomPath(Vector3[] wayPoints, Vector3 startPosition, float turnDistance, float stoppingDistance)
        {
            LookPoints = wayPoints;
            TurnBoundaries = new Line[LookPoints.Length];
            FinishLineIndex = TurnBoundaries.Length - 1;

            Vector2 previousPoint = V3ToV2(startPosition);

            for (int i = 0; i < LookPoints.Length; i++)
            {
                Vector2 currentPoint = V3ToV2(LookPoints[i]);
                Vector2 directionToCurrentPoint = (currentPoint - previousPoint).normalized;
                Vector2 turnBoundaryPoint = (i == FinishLineIndex) ? currentPoint : currentPoint - directionToCurrentPoint * turnDistance;

                TurnBoundaries[i] = new Line(turnBoundaryPoint, previousPoint - directionToCurrentPoint * turnDistance);

                previousPoint = turnBoundaryPoint;
            }

            float distanceFromEndPoint = 0;
            for (int i = LookPoints.Length - 1; i > 0; i--)
            {
                distanceFromEndPoint += Vector3.Distance(LookPoints[i], LookPoints[i - 1]);
                if (distanceFromEndPoint > stoppingDistance)
                {
                    slowDownIndex = i;
                    break;
                }
            }
        }

        Vector2 V3ToV2(Vector3 v3)
        {
            return new Vector2(v3.x, v3.z);
        }

        public void DrawWithGizmos()
        {
            Gizmos.color = Color.black;

            foreach (Vector3 point in LookPoints)
            {
                Gizmos.DrawCube(point + Vector3.up, Vector3.one);
            }

            Gizmos.color = Color.white;

            foreach (Line line in TurnBoundaries)
            {
                line.DrawGizmos(10);
            }
        }
    }
}
