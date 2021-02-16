using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Threading;

namespace BubbleSoccer
{
    public class PathRequestManager : MonoBehaviour
    {
        //helpers
        private Queue<PathResult> results = new Queue<PathResult>();
        private static PathRequestManager _Instance;
        private Pathfinding pathfinding;


        private void Awake()
        {
            _Instance = this;
            pathfinding = GetComponent<Pathfinding>();
        }
        private void Update()
        {
            if (results.Count > 0)
            {
                int itemsInQueue = results.Count;
                lock (results)
                {
                    for (int i = 0; i < itemsInQueue; i++)
                    {
                        PathResult result = results.Dequeue();
                        result.callBack(result.path, result.success);
                    }
                }
            }
        }
        public static void RequestPath(PathRequest request)
        {
            ThreadStart threadStart = delegate
            {
                _Instance.pathfinding.FindPath(request, _Instance.FinishedProcessingPath);
            };
            threadStart.Invoke();
        }


        public void FinishedProcessingPath(PathResult result)
        {
            lock (results)
            {
                results.Enqueue(result);
            }
        }
    }

    public struct PathResult
    {
        public Vector3[] path;
        public bool success;
        public Action<Vector3[], bool> callBack;

        public PathResult(Vector3[] path, bool success, Action<Vector3[], bool> callBack)
        {
            this.path = path;
            this.success = success;
            this.callBack = callBack;
        }
    }

    public struct PathRequest
    {
        public Vector3 pathStart;
        public Vector3 pathEnd;
        public Action<Vector3[], bool> callBack;

        public PathRequest(Vector3 start, Vector3 end, Action<Vector3[], bool> _callBack)
        {
            pathStart = start;
            pathEnd = end;
            callBack = _callBack;
        }
    }
}
