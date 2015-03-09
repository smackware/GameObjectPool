using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class MaxPoolSizeException : Exception {}


namespace Smackware.ObjectPool
{    

    public class GameObjectPoolManager : MonoBehaviour
    {

        internal class DynamicGameObjectPool : DynamicObjectPool<GameObject>
        {
            private GameObject _prefab;
            public DynamicGameObjectPool(GameObject prefab, int maxSize)
                : base(prefab, maxSize)
            {
                _prefab = prefab;
            }

            public override GameObject CreateNew()
            {
                GameObject obj = (GameObject)GameObject.Instantiate(_prefab);
                PoolPrefab poolPrefab = obj.AddComponent<PoolPrefab>();
                poolPrefab.SourcePrefab = _prefab;
                return obj;
            }

        }

        private static GameObjectPoolManager _singleton;
        private Transform _transform;

        static GameObjectPoolManager()
        {
            GameObject go = new GameObject("ObjectPoolManager");
            _singleton = go.AddComponent<GameObjectPoolManager>();
        }

        void Awake()
        {
            _transform = transform;
        }

        public static GameObject Borrow(GameObject prefab)
        {
            return _singleton._borrow(prefab);
        }


        public static T Borrow<T>(T prefab, Transform newParent) where T : MonoBehaviour
        {
            GameObject instance = _singleton._borrow(prefab.gameObject);
            T instanceComponent = instance.GetComponent<T>();
            if (instanceComponent == null)
            {
                Revert(instance);
            }
            else
            {
                instance.transform.SetParent(newParent);
            }
            return instanceComponent;
        }


        public static T Borrow<T>(T prefab) where T : MonoBehaviour
        {
            return Borrow<T>(prefab, null);
        }

        public static void Revert(GameObject prefab, GameObject instance)
        {
            _singleton._revert(prefab, instance);
        }

        public static void Revert(MonoBehaviour instance)
        {
            Revert(instance.gameObject);
        }

        public static void Revert(GameObject instance)
        {
            PoolPrefab poolPrefab = instance.GetComponent<PoolPrefab>();
            Revert(poolPrefab.SourcePrefab, instance);
        }



        public static void SetMaxPoolSize(GameObject prefab, int maxSize)
        {
            _singleton._setMaxPoolSize(prefab, maxSize);
        }

        public int defaultMaxPoolSize = 1000;
        private Dictionary<UnityEngine.Object, DynamicGameObjectPool> _pools = new Dictionary<UnityEngine.Object, DynamicGameObjectPool>();



        private GameObject _borrow(GameObject prefab)
        {
            GameObject item = GetPoolFor(prefab).Borrow();
            item.SetActive(true);
            return item;
        }

        private void _revert(GameObject prefab, GameObject instance)
        {
            instance.transform.SetParent(_transform);
            instance.SetActive(false);
            GetPoolFor(prefab).Revert(instance);
        }

        private void _setMaxPoolSize(GameObject prefab, int maxSize)
        {
            GetPoolFor(prefab).MaxSize = maxSize;
        }

        private DynamicGameObjectPool GetPoolFor(GameObject prefab)
        {
            if (!_pools.ContainsKey(prefab))
            {
                _pools.Add(prefab, new DynamicGameObjectPool(prefab, defaultMaxPoolSize));
            }
            return _pools[prefab];
        }

    }
}