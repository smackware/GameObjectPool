using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class MaxPoolSizeException : Exception {}


namespace Smackware.ObjectPool
{
    /*
     * A singleton instance of this will be auto-created in the static constructor
     * 
     * Immediate Usage:
     *      
     * 
     * GameObject newInstance = GameObjectPoolManager.Borrow(GameObjectPrefab);
     * MyScript newInstance2 = GameObjectPoolManager.Borrow<MyScript>(myScriptPrefab, newParent: Transform);
     * 
     * 
     * // When we're done, we give the objects back to the pool. The pool manager will auto-deactivate the gameObject of each.
     * GameObjectPoolManager.Revert(newInstance);
     * GameObjectPoolManager.Revert(newInstance2);
     * 
     * 
     */
    public class GameObjectPoolManager : MonoBehaviour
    {
        /* This class describes a Pool of a single prefab type */
        private class DynamicGameObjectPool : DynamicObjectPool<GameObject>
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
                var poolPrefab = GetOrAddPoolPrefabComponent(obj);
                poolPrefab.SourcePrefab = _prefab;
                return obj;
            }

            public static PoolPrefab GetOrAddPoolPrefabComponent(GameObject obj)
            {
                PoolPrefab o = obj.GetComponent<PoolPrefab>();
                if (o == null)
                {
                    o = obj.AddComponent<PoolPrefab>();
                }
                return o;
            }
        }


        private static GameObjectPoolManager _singleton;
        private Transform _transform;

        #region Static Constructor
        static GameObjectPoolManager()
        {
            GameObject go = new GameObject("ObjectPoolManager");
            _singleton = go.AddComponent<GameObjectPoolManager>();
        }
        #endregion

        void Awake()
        {
            _transform = transform;
            _transform.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;	
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
            PoolPrefab poolPrefab = DynamicGameObjectPool.GetOrAddPoolPrefabComponent(item);
            item.SetActive(true);
            poolPrefab.OnBorrow();
            return item;
        }

        private void _revert(GameObject prefab, GameObject instance)
        {            
            instance.transform.SetParent(_transform);
            PoolPrefab poolPrefab = DynamicGameObjectPool.GetOrAddPoolPrefabComponent(prefab);
            poolPrefab.OnRevert();
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