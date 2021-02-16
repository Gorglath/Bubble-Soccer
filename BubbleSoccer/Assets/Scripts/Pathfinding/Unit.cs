using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BubbleSoccer
{
    public class Unit : MonoBehaviour
    {
        const float minPathUpdateTime = 0.2f;
        const float pathUpdateMoveThreshold = 0.5f;

        public Transform Target;
        public float Speed = 20f;
        public float TurnSpeed = 3f;
        public float TurnDistance = 5f;
        public float StoppingDistance = 10f;

        private CustomPath path;

        private void Start()
        {
            StartCoroutine(UpdatePath());
        }

        public void OnPathFound(Vector3[] wayPoints, bool pathSuccessful)
        {
            if (pathSuccessful)
            {
                path = new CustomPath(wayPoints, transform.position, TurnDistance, StoppingDistance);
                StopCoroutine("FollowPath");
                StartCoroutine("FollowPath");
            }
        }

        IEnumerator UpdatePath()
        {
            if (Time.timeSinceLevelLoad < 0.3f)
            {
                yield return new WaitForSeconds(0.3f);
            }
            PathRequestManager.RequestPath(new PathRequest(transform.position, Target.position, OnPathFound));

            float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
            Vector3 targetPosOld = Target.position;

            while (true)
            {
                yield return new WaitForSeconds(minPathUpdateTime);
                if ((Target.position - targetPosOld).sqrMagnitude > sqrMoveThreshold)
                {
                    PathRequestManager.RequestPath(new PathRequest(transform.position, Target.position, OnPathFound));
                    targetPosOld = Target.position;
                }

            }
        }
        IEnumerator FollowPath()
        {
            bool followingPath = true;
            int pathIndex = 0;
            transform.LookAt(path.LookPoints[0]);

            float speedPercent = 1;

            while (followingPath)
            {
                Vector2 position2D = new Vector2(transform.position.x, transform.position.z);
                while (path.TurnBoundaries[pathIndex].HasCrossedLine(position2D))
                {
                    if (pathIndex == path.FinishLineIndex)
                    {
                        followingPath = false;
                        break;
                    }
                    else
                    {
                        pathIndex++;
                    }
                }

                if (followingPath)
                {
                    if (pathIndex >= path.slowDownIndex && StoppingDistance > 0)
                    {
                        speedPercent = Mathf.Clamp01(path.TurnBoundaries[path.FinishLineIndex].DistanceFromPoint(position2D) / StoppingDistance);
                        if (speedPercent < 0.01)
                        {
                            followingPath = false;
                        }
                    }
                    Quaternion targetRotation = Quaternion.LookRotation(path.LookPoints[pathIndex] - transform.position);
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * TurnSpeed);

                    transform.Translate(Vector3.forward * Time.deltaTime * Speed * speedPercent, Space.Self);
                }
                yield return null;
            }
        }

        private void OnDrawGizmos()
        {
            if (path != null)
            {
                path.DrawWithGizmos();
            }
        }
    }
}
