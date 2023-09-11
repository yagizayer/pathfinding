// GridPoint.cs

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Refactored_PathFinding.Scripts.Helpers
{
    [Serializable]
    public class GridPoint
    {
        private static List<GridPoint> _allPoints = new List<GridPoint>();

        public const float Distance = 1f;
        public Vector3Int Coords { get; private set; }
        public Vector3 WorldPosition { get; private set; }
        public bool IsInvalid { get; set; }
        public List<GridPoint> Neighbours { get; protected set; }

        #region Static Methods

        public static Vector3Int GetPointCounts(Bounds worldBounds) =>
            new Vector3Int(
                Mathf.CeilToInt(worldBounds.size.x / Distance) + 1,
                Mathf.CeilToInt(worldBounds.size.y / Distance) + 1,
                Mathf.CeilToInt(worldBounds.size.z / Distance) + 1
            );

        public static IEnumerable<GridPoint> NearbyPoints(Vector3 worldPos, float radius = Distance * 2,
            bool includeInvalids = false) =>
            _allPoints.Where(point =>
            {
                var distance = (point.WorldPosition - worldPos).magnitude;
                return distance <= radius && (includeInvalids || !point.IsInvalid);
            });

        public static void Clear() => _allPoints.Clear();

        #endregion

        public GridPoint(Vector3Int coords)
        {
            Coords = coords;
            WorldPosition = new Vector3(Coords.x, Coords.y, Coords.z) * Distance - PathManager.VolumeOffset;
            _allPoints.Add(this);
            Neighbours = new List<GridPoint>();
        }

        public void SetValidity(IEnumerable<PathObstacle> obstacles) =>
            IsInvalid = obstacles.Any(obstacle => obstacle.MyCollider.bounds.Contains(WorldPosition));
        
    }
}