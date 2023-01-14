using System;
using MITHack.Robot.Spawner;
using MITHack.Robot.Utils.Components;
using UnityEngine;

namespace MITHack.Robot.Entities
{
    [RequireComponent(typeof(PooledObjectComponent))]
    public class ChickenEntity : MonoBehaviour
    {
        [Header("Life Length")]
        [SerializeField, Min(1.0f)]
        private float maxLifeLength = 5.0f;
        
        private IPooledObject.PooledObjectDelegate<PooledObjectComponent> _allocatedDelegate;
        private IPooledObject.PooledObjectDelegate<PooledObjectComponent> _deallocatedDelegate;
        
        private PooledObjectComponent _pooledObject;

        private float _currentLifeLength = 0.0f;

        private void Awake()
        {
            _pooledObject = GetComponent<PooledObjectComponent>();
            _allocatedDelegate = OnAllocated;
            _deallocatedDelegate = OnDeAllocated;
            _currentLifeLength = maxLifeLength;
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

        private void Update()
        {
            UpdateLifeLength(Time.deltaTime);
        }

        /// <summary>
        /// Updates the life length of the chicken entity.
        /// </summary>
        /// <param name="deltaTime">The delta time of the chicken entity.</param>
        private void UpdateLifeLength(float deltaTime)
        {
            if (_currentLifeLength > 0.0f)
            {
                _currentLifeLength -= deltaTime;
                if (_currentLifeLength <= 0.0f)
                {
                    _currentLifeLength = 0.0f;
                    // The Pooled Object.
                    if (!_pooledObject) return;
                    _pooledObject.DeAllocate();
                }
            } 
        }

        private void OnAllocated(IObjectPool<PooledObjectComponent> pool)
        {
            _currentLifeLength = maxLifeLength;
        }

        private void OnDeAllocated(IObjectPool<PooledObjectComponent> pool)
        {
        }
    }
}