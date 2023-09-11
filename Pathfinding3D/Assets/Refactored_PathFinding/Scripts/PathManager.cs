// PathManager.cs

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Refactored_PathFinding.Scripts.Helpers;
using UnityEngine;

namespace Refactored_PathFinding.Scripts
{
    public class PathManager : SingletonBase<PathManager>
    {
        [SerializeField]
        private float refreshInterval = -1;

        public static Vector3 VolumeOffset { get; private set; }

        private PathPoint[][][] _grid;
        private Bounds _boundsOfWorld;
        private Vector3Int _pointCounts;

        private IEnumerator Start()
        {
            while (true)
            {
                if (_boundsOfWorld != default && refreshInterval <= 0)
                {
                    yield return null;
                    continue;
                }

                yield return new WaitForSeconds(refreshInterval);
                var obstacles = FindObjectsOfType<PathFinderObstacle>();
                _boundsOfWorld = obstacles.Aggregate(new Bounds(), (current, obstacle) =>
                {
                    current.Encapsulate(obstacle.MyCollider.bounds);
                    return current;
                });
                _pointCounts = PathPoint.GetPointCounts(_boundsOfWorld);
                VolumeOffset = _boundsOfWorld.extents - _boundsOfWorld.center;
                InitializeGrid(obstacles);
            }
        }

        private void InitializeGrid(IEnumerable<PathFinderObstacle> obstacles)
        {
            PathPoint.Clear();
            _grid = null;
            LoopVolume((width, height, depth) =>
            {
                var newPoint = new PathPoint(new Vector3Int(width, height, depth));
                newPoint.SetValidity(obstacles);
                _grid[width][height][depth] = newPoint;
            });

            LoopVolume((width, height, depth) =>
            {
                var isLeftBorder = width == 0;
                var isRightBorder = width == _pointCounts.x - 1;
                var isBottomBorder = height == 0;
                var isTopBorder = height == _pointCounts.y - 1;
                var isFrontBorder = depth == 0;
                var isBackBorder = depth == _pointCounts.z - 1;
                var currentPoint = _grid[width][height][depth];

                if (!isLeftBorder) currentPoint.AddNeighbour(_grid[width - 1][height][depth]);
                if (!isRightBorder) currentPoint.AddNeighbour(_grid[width + 1][height][depth]);
                if (!isBottomBorder) currentPoint.AddNeighbour(_grid[width][height - 1][depth]);
                if (!isTopBorder) currentPoint.AddNeighbour(_grid[width][height + 1][depth]);
                if (!isFrontBorder) currentPoint.AddNeighbour(_grid[width][height][depth - 1]);
                if (!isBackBorder) currentPoint.AddNeighbour(_grid[width][height][depth + 1]);
            });
        }

        private void LoopVolume(Action<int, int, int> loopAction)
        {
            var isInitialized = _grid != null;
            if (!isInitialized) _grid = new PathPoint[_pointCounts.x][][];
            for (var width = 0; width < _pointCounts.x; width++)
            {
                if (!isInitialized) _grid[width] = new PathPoint[_pointCounts.y][];
                for (var height = 0; height < _pointCounts.y; height++)
                {
                    if (!isInitialized) _grid[width][height] = new PathPoint[_pointCounts.z];
                    for (var depth = 0; depth < _pointCounts.z; depth++)
                        loopAction(width, height, depth);
                }
            }
        }

        private PathPoint GetClosestPoint(Vector3 worldPos)
        {
            var nearbyPoints = PathPoint.NearbyPoints(worldPos);
            var closestPoint = nearbyPoints.Aggregate((current, point) =>
                (current.WorldPosition - worldPos).sqrMagnitude <
                (point.WorldPosition - worldPos).sqrMagnitude
                    ? current
                    : point);

            return closestPoint;
        }

        private void OnDrawGizmos()
        {
            if (_boundsOfWorld == default) return;

            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(_boundsOfWorld.center, _boundsOfWorld.size);

            foreach (var point in PathPoint.NearbyPoints(transform.position, 10, true))
            {
                Gizmos.color = point.IsInvalid ? Color.red : Color.green;
                Gizmos.DrawSphere(point.WorldPosition, .1f);
            }
        }
    }
}