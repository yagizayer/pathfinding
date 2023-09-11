// Point.cs

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Refactored_PathFinding.Scripts.Helpers
{
    [Serializable]
    public class PathPoint
    {
        private static List<PathPoint> _allPoints = new List<PathPoint>();

        public const float Distance = 1f;
        public Vector3Int Coords { get; private set; }
        public Vector3 WorldPosition { get; private set; }
        public bool IsInvalid { get; set; }
        public List<PathPoint> Neighbours { get; private set; }

        public PathPoint(Vector3Int coords)
        {
            Coords = coords;
            Neighbours = new List<PathPoint>();
            WorldPosition = new Vector3(Coords.x, Coords.y, Coords.z) * Distance - PathManager.VolumeOffset;
            _allPoints.Add(this);
        }

        public static Vector3Int GetPointCounts(Bounds worldBounds) =>
            new Vector3Int(
                Mathf.CeilToInt(worldBounds.size.x / Distance) + 1,
                Mathf.CeilToInt(worldBounds.size.y / Distance) + 1,
                Mathf.CeilToInt(worldBounds.size.z / Distance) + 1
            );

        public void SetValidity(IEnumerable<PathFinderObstacle> obstacles) =>
            IsInvalid = obstacles.Any(obstacle => obstacle.MyCollider.bounds.Contains(WorldPosition));

        public void AddNeighbour(PathPoint neighbour) => Neighbours.Add(neighbour);

        public static IEnumerable<PathPoint> NearbyPoints(Vector3 worldPos, float radius = Distance * 2,
            bool includeInvalids = false) =>
            _allPoints.Where(point =>
            {
                var distance = (point.WorldPosition - worldPos).magnitude;
                return distance <= radius && (includeInvalids || !point.IsInvalid);
            });

        public static void Clear() => _allPoints.Clear();
    }
}