// PathFollower.cs

using UnityEngine;

namespace Refactored_PathFinding.Scripts
{
    [RequireComponent(typeof(Collider))]
    public class PathFinderObstacle : MonoBehaviour
    {
        [SerializeField]
        private Collider myCollider;
        
        public Collider MyCollider => myCollider;
    }
}