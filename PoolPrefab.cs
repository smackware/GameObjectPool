using UnityEngine;
using System.Collections;

namespace Smackware.ObjectPool
{
    /* This class is auto-attached to borrowed instances, it contains data relevant to the origin of this borrowed instance
     */
    public class PoolPrefab : MonoBehaviour
    {
        public GameObject SourcePrefab;
    }

}