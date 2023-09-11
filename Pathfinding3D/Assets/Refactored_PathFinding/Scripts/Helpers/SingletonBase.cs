// ISingleton.cs

using UnityEngine;

namespace Refactored_PathFinding.Scripts.Helpers
{
    public class SingletonBase<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance != null) return _instance;
                // if there is no instance, try to find it
                _instance = FindObjectOfType<T>();
                if (_instance != null) return _instance;
                // if there is no instance in the scene, create one
                var singleton = new GameObject(typeof(T).Name);
                _instance = singleton.AddComponent<T>();
                return _instance;
            }
        }
    }
}