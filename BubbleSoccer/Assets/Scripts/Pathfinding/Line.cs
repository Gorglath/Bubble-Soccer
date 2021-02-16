using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BubbleSoccer
{
    public struct Line
    {
        const float verticalLineGradient = 1e5f;

        float gradient;
        float y_intercept;
        Vector2 pointOnLine_1;
        Vector2 pointOnLine_2;
        float gradientPerpendicular;
        bool approachedSide;
        public Line(Vector2 pointOnLine, Vector2 pointPerpendicularToLine)
        {
            float dx = pointOnLine.x - pointPerpendicularToLine.x;
            float dy = pointOnLine.y - pointPerpendicularToLine.y;

            if (dx == 0)
            {
                gradientPerpendicular = verticalLineGradient;
            }
            else
            {
                gradientPerpendicular = dy / dx;
            }

            if (gradientPerpendicular == 0)
            {
                gradient = verticalLineGradient;
            }
            else
            {
                gradient = -1 / gradientPerpendicular;
            }

            y_intercept = pointOnLine.y - gradient * pointOnLine.x;
            pointOnLine_1 = pointOnLine;
            pointOnLine_2 = pointOnLine + new Vector2(1, gradient);

            approachedSide = false;
            approachedSide = GetSide(pointPerpendicularToLine);
        }

        private bool GetSide(Vector2 point)
        {
            return (point.x - pointOnLine_1.x) * (pointOnLine_2.y - pointOnLine_1.y) > (point.y - pointOnLine_1.y) * (pointOnLine_2.x - pointOnLine_1.x);
        }

        public bool HasCrossedLine(Vector2 point)
        {
            return GetSide(point) != approachedSide;
        }

        public float DistanceFromPoint(Vector2 point)
        {
            float y_InterceptPerpendicular = point.y - gradientPerpendicular * point.x;
            float intersectX = (y_InterceptPerpendicular - y_intercept) / (gradient - gradientPerpendicular);
            float intersectY = gradient * intersectX + y_intercept;

            return Vector2.Distance(point, new Vector2(intersectX, intersectY));
        }
        public void DrawGizmos(float lenght)
        {
            Vector3 lineDirection = new Vector3(1, 0, gradient).normalized;
            Vector3 lineCenter = new Vector3(pointOnLine_1.x, 0, pointOnLine_1.y) + Vector3.up;

            Gizmos.DrawLine(lineCenter - lineDirection * lenght / 2f, lineCenter + lineDirection * lenght / 2);
        }
    }
}
