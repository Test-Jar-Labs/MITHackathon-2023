using MITHack.Robot.Spawner;
using MITHack.Robot.Utils.Components;
using UnityEngine;

namespace MITHack.Robot.Entities
{
    [RequireComponent(typeof(PooledObjectComponent))]
    public class ChickenEntity : MonoBehaviour
    {
        private IPooledObject.PooledObjectDelegate<PooledObjectComponent> _allocatedDelegate;
        private IPooledObject.PooledObjectDelegate<PooledObjectComponent> _deallocatedDelegate;
        
        private PooledObjectComponent _pooledObject;

        private void Awake()
        {
            _pooledObject = GetComponent<PooledObjectComponent>();
            _allocatedDelegate = OnAllocated;
            _deallocatedDelegate = OnDeAllocated;
        }

        private void OnEnable()
        {
            _pooledObject.allocatedEvent
                += _allocatedDelegate;
            _pooledObject.deallocatedEvent
                += _deallocatedDelegate;
        }

        private void OnDisable()
        {
            _pooledObject.allocatedEvent
                -= _allocatedDelegate;
            _pooledObject.deallocatedEvent
                -= _deallocatedDelegate;
        }
        
        private void OnAllocated(IObjectPool<PooledObjectComponent> pool)
        {
            // TODO: Implementation
        }

        private void OnDeAllocated(IObjectPool<PooledObjectComponent> pool)
        {
            // TODO: Implementation
        }
    }
}