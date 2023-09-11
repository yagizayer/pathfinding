// PathPoint.cs

using System.Collections.Generic;

namespace Refactored_PathFinding.Scripts.Helpers
{
    public class PathPoint : GridPoint
    {
        public float DistanceFromStartSqr { get; set; }
        public float DistanceToEndSqr { get; set; }
        public float TotalDistance => DistanceFromStartSqr + DistanceToEndSqr;
        public PathPoint PreviousPoint { get; set; }

        public PathPoint(GridPoint gridPoint) : base(gridPoint.Coords)
        {
            IsInvalid = gridPoint.IsInvalid;
            Neighbours = gridPoint.Neighbours;
        }
    }
}