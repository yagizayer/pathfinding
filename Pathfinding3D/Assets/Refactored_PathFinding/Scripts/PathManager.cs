// PathManager.cs

using System;
using System.Collections.Generic;
using System.Linq;
using Refactored_PathFinding.Scripts.Helpers;
using UnityEngine;

namespace Refactored_PathFinding.Scripts
{
    public class PathManager : SingletonBase<PathManager>
    {
        public static Vector3 VolumeOffset => Instance._boundsOfWorld.extents - Instance._boundsOfWorld.center;
        public static Bounds WorldBounds => Instance._boundsOfWorld;

        private GridPoint[][][] _grid;
        private Bounds _boundsOfWorld;
        private Vector3Int _pointCounts;

        private void Awake()
        {
            var obstacles = FindObjectsOfType<PathObstacle>();
            _boundsOfWorld = obstacles.Aggregate(new Bounds(), (current, obstacle) =>
            {
                current.Encapsulate(obstacle.MyCollider.bounds);
                return current;
            });
            _pointCounts = GridPoint.GetPointCounts(_boundsOfWorld);
            InitializeGrid(obstacles);
        }

        private void OnDrawGizmos()
        {
            if (_boundsOfWorld == default) return;

            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(_boundsOfWorld.center, _boundsOfWorld.size);

            foreach (var point in GridPoint.NearbyPoints(transform.position, 10, true))
            {
                Gizmos.color = point.IsInvalid ? Color.red : Color.green;
                Gizmos.DrawSphere(point.WorldPosition, .1f);
            }
        }


        private void InitializeGrid(IEnumerable<PathObstacle> obstacles)
        {
            GridPoint.Clear();
            _grid = null;
            LoopGrid((width, height, depth) =>
            {
                var newPoint = new GridPoint(new Vector3Int(width, height, depth));
                newPoint.SetValidity(obstacles);
                _grid![width][height][depth] = newPoint;
            });

            LoopGrid((width, height, depth) =>
            {
                var isLeftBorder = width == 0;
                var isRightBorder = width == _pointCounts.x - 1;
                var isBottomBorder = height == 0;
                var isTopBorder = height == _pointCounts.y - 1;
                var isFrontBorder = depth == 0;
                var isBackBorder = depth == _pointCounts.z - 1;
                var currentPoint = _grid[width][height][depth];

                if (!isLeftBorder) currentPoint.Neighbours.Add(_grid[width - 1][height][depth]);
                if (!isRightBorder) currentPoint.Neighbours.Add(_grid[width + 1][height][depth]);
                if (!isBottomBorder) currentPoint.Neighbours.Add(_grid[width][height - 1][depth]);
                if (!isTopBorder) currentPoint.Neighbours.Add(_grid[width][height + 1][depth]);
                if (!isFrontBorder) currentPoint.Neighbours.Add(_grid[width][height][depth - 1]);
                if (!isBackBorder) currentPoint.Neighbours.Add(_grid[width][height][depth + 1]);
            });
        }

        private void LoopGrid(Action<int, int, int> loopAction)
        {
            var isInitialized = _grid != null;
            if (!isInitialized) _grid = new GridPoint[_pointCounts.x][][];
            for (var width = 0; width < _pointCounts.x; width++)
            {
                if (!isInitialized) _grid[width] = new GridPoint[_pointCounts.y][];
                for (var height = 0; height < _pointCounts.y; height++)
                {
                    if (!isInitialized) _grid[width][height] = new GridPoint[_pointCounts.z];
                    for (var depth = 0; depth < _pointCounts.z; depth++)
                        loopAction(width, height, depth);
                }
            }
        }

        public static GridPoint GetClosestPoint(Vector3 worldPos)
        {
            var nearbyPoints = GridPoint.NearbyPoints(worldPos);
            var closestPoint = nearbyPoints.Aggregate((closest, me) =>
            {
                var closestDistance = (closest.WorldPosition - worldPos).sqrMagnitude;
                var myDistance = (me.WorldPosition - worldPos).sqrMagnitude;

                return closestDistance < myDistance ? closest : me;
            });

            return closestPoint;
        }

        public static Vector3 GetRandomWorldPosition()
        {
            var randomPoint = Instance._boundsOfWorld.center;
            randomPoint.x +=
                UnityEngine.Random.Range(-Instance._boundsOfWorld.extents.x, Instance._boundsOfWorld.extents.x);
            randomPoint.y +=
                UnityEngine.Random.Range(-Instance._boundsOfWorld.extents.y, Instance._boundsOfWorld.extents.y);
            randomPoint.z +=
                UnityEngine.Random.Range(-Instance._boundsOfWorld.extents.z, Instance._boundsOfWorld.extents.z);
            return randomPoint;
        }

        public static Queue<PathPoint> PathTo(Vector3 from, Vector3 to)
        {
            var startGridPoint = GetClosestPoint(from);
            var endGridPoint = GetClosestPoint(to);

            var path = new List<PathPoint>();
            var openList = new List<PathPoint>();
            var closedList = new List<PathPoint>();
            var currentPoint = new PathPoint(startGridPoint);

            openList.Add(currentPoint);

            while (openList.Count > 0)
            {
                currentPoint = openList.OrderBy(point => point.TotalDistance).First();
                if (currentPoint == endGridPoint)
                    break;

                openList.Remove(currentPoint);
                closedList.Add(currentPoint);
                foreach (var neighbor in currentPoint.Neighbours)
                {
                    if (neighbor.IsInvalid) continue;
                    var neighborPoint = new PathPoint(neighbor);
                    var isNeighborChecked = closedList.Contains(neighborPoint);

                    if (isNeighborChecked) continue;
                    if (!openList.Contains(neighborPoint))
                    {
                        neighborPoint.DistanceFromStartSqr =
                            (currentPoint.DistanceFromStartSqr + 1) * (currentPoint.DistanceFromStartSqr + 1);
                        neighborPoint.DistanceToEndSqr =
                            (neighbor.WorldPosition - endGridPoint.WorldPosition).sqrMagnitude;
                        neighborPoint.PreviousPoint = currentPoint;
                        openList.Add(neighborPoint);
                    }
                    else
                    {
                        var tempDistanceFromStart =
                            (currentPoint.DistanceFromStartSqr + 1) * (currentPoint.DistanceFromStartSqr + 1);
                        if (tempDistanceFromStart >= neighborPoint.DistanceFromStartSqr) continue;

                        neighborPoint.DistanceFromStartSqr = tempDistanceFromStart;
                        neighborPoint.PreviousPoint = currentPoint;
                    }
                }
            }

            while (currentPoint != null)
            {
                path.Add(currentPoint);
                currentPoint = currentPoint.PreviousPoint;
            }

            path.Reverse();
            
            var pathQueue = new Queue<PathPoint>();
            foreach (var pathPoint in path) pathQueue.Enqueue(pathPoint);
            
            return pathQueue;
        }
    }
}