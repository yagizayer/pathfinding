// PathFollower.cs

using System;
using System.Collections.Generic;
using Refactored_PathFinding.Scripts.Helpers;
using UnityEngine;

namespace Refactored_PathFinding.Scripts
{
    public class PathFollower : MonoBehaviour
    {
        [Range(0,100)]
        [SerializeField]
        private float movementSpeed = 5f;
        
        [Range(0,100)]
        [SerializeField]
        private float rotationSpeed = 5f;
        
        private Transform _myTransform;
        private Queue<PathPoint> _path;

        private void Start()
        {
            var getRandomPoint = PathManager.GetRandomWorldPosition();
            _myTransform = transform;
            _path = PathManager.PathTo(_myTransform.position, getRandomPoint);
        }

        private void Update()
        {
            if(_path == null || _path.Count == 0) return;
            
            var nextPoint = _path.Peek();
            var direction = nextPoint.WorldPosition - _myTransform.position;
            var distance = direction.magnitude;
            if (distance < .1f)
            {
                _path.Dequeue();
                return;
            }
            
            var lookRotation = Quaternion.LookRotation(direction);
            _myTransform.rotation = Quaternion.Slerp(_myTransform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
            _myTransform.Translate(_myTransform.forward * (movementSpeed * Time.deltaTime));
        }
    }
}