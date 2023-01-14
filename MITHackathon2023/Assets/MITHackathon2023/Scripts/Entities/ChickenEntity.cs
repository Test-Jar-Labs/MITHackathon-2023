using System;
using MITHack.Robot.Spawner;
using MITHack.Robot.Utils;
using MITHack.Robot.Utils.Components;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MITHack.Robot.Entities
{
    [RequireComponent(typeof(PooledObjectComponent))]
    public class ChickenEntity : MonoBehaviour
    {
        [Header("Variables")]
        [SerializeField, Min(1.0f)]
        private float maxLifeLength = 5.0f;
        [Space] 
        [SerializeField, Range(0.0f, 180.0f)] 
        private float leftAngle = 20.0f;
        [SerializeField, Range(0.0f, 180.0f)]
        private float rightAngle = 20.0f;
        [Space] 
        [SerializeField, Min(0.0f)] private float minForce;
        [SerializeField, Min(0.0f)] private float maxForce;
        
        [Header("References")] 
        [SerializeField]
        private new Rigidbody rigidbody;
        
        private IPooledObject.PooledObjectDelegate<PooledObjectComponent, PooledObjectComponent.PooledObjectSpawnContext> _allocatedDelegate;
        private IPooledObject.PooledObjectDelegate<PooledObjectComponent, PooledObjectComponent.PooledObjectSpawnContext> _deallocatedDelegate;
        
        private PooledObjectComponent _pooledObject;
        private float _currentLifeLength = 0.0f;

        private Rigidbody Rigidbody => rigidbody ??= GetComponent<Rigidbody>();
        
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

        private void OnAllocated(IObjectPool<PooledObjectComponent, PooledObjectComponent.PooledObjectSpawnContext> pool)
        {
            _currentLifeLength = maxLifeLength;
            ApplyForces(transform.position);
        }
        
        private void OnDeAllocated(IObjectPool<PooledObjectComponent, PooledObjectComponent.PooledObjectSpawnContext> pool)
        {
            // Resets the rigidbody.
            if (Rigidbody)
            {
                Rigidbody.velocity = Vector3.zero;
                Rigidbody.angularVelocity = Vector3.zero;
            }
        }

        private void ApplyForces(Vector3 position)
        {
            if (!Rigidbody) return;

            var cachedTransform = transform;
            cachedTransform.position = position;
            cachedTransform.rotation = Quaternion.identity;;
            
            var direction = CalculateRandomDirection(position);
            var randomForce = Random.Range(minForce, maxForce);
            Rigidbody.AddRelativeForce(direction * randomForce, ForceMode.Impulse);
        }

        private Vector3 CalculateRandomDirection(Vector3 position)
        {
            var referenceDirection = Vector3.down;
            var droneEntity = DroneEntity.Get();
            if (droneEntity)
            {
                referenceDirection = (droneEntity.transform.position - position);
                referenceDirection.Normalize();
            }
            VectorUtility.CalculateDirections(
                referenceDirection, out var up, out var right);
            var randomAngleX = Random.Range(-leftAngle, rightAngle);
            var randomAngleY = Random.Range(-leftAngle, rightAngle);
            var randomRotation = Quaternion.LookRotation(referenceDirection)
                * Quaternion.AngleAxis(randomAngleX, up)
                * Quaternion.AngleAxis(randomAngleY, right);
            return randomRotation * Vector3.forward;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            var position = transform.position;
            Gizmos.DrawRay(position, CalculateRandomDirection(position));
        }

        private void OnValidate()
        {
            maxForce = Mathf.Max(maxForce, minForce);
        }
    }
}