using MITHack.Robot.Spawner;
using UnityEngine;

namespace MITHack.Robot.Utils.Components
{
    public class PooledObjectComponent : MonoBehaviour, IPooledObject
    {
        public IPooledObject.PooledObjectDelegate<PooledObjectComponent> initializedEvent;
        public IPooledObject.PooledObjectDelegate<PooledObjectComponent> allocatedEvent;
        public IPooledObject.PooledObjectDelegate<PooledObjectComponent> deallocatedEvent;

        public void OnInitialized<TSelf>(IObjectPool<TSelf> pool)
        {
            if (pool is IObjectPool<PooledObjectComponent> pooledObjects)
            {
                gameObject.SetActive(false);
                initializedEvent?.Invoke(pooledObjects);
            }
        }

        public void OnAllocated<TSelf>(IObjectPool<TSelf> pool)
        {
            if (pool is IObjectPool<PooledObjectComponent> pooledObjects)
            {
                gameObject.SetActive(true);
                allocatedEvent?.Invoke(pooledObjects);
            }
        }

        public void OnDeAllocated<TSelf>(IObjectPool<TSelf> pool)
        {
            if (pool is IObjectPool<PooledObjectComponent> pooledObjects)
            {
                gameObject.SetActive(false);
                deallocatedEvent?.Invoke(pooledObjects);
            }
        }
    }
}