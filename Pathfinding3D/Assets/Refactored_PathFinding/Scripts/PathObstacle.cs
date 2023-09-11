// PathFollower.cs

using UnityEngine;

namespace Refactored_PathFinding.Scripts
{
    [RequireComponent(typeof(Collider))]
    public class PathObstacle : MonoBehaviour
    {
        [SerializeField]
        private Collider myCollider;
        
        public Collider MyCollider => myCollider;
    }
}