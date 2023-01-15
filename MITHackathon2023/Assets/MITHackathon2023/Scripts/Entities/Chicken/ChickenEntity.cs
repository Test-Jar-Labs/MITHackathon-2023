using DG.Tweening;
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
        [SerializeField, Min(0.0f)] 
        private float minTravelTime;
        [SerializeField]
        private float maxTravelTime;
        
        [Header("Misc")] 
        [SerializeField, Min(0.0f)]
        private float targetEntityNormalOffset = 0.05f;
        [SerializeField]
        private Prefab<ChickenTarget> targetEntity;
        [Space] 
        [SerializeField] 
        private LayerMask layerMask;

        [Header("References")] 
        [SerializeField]
        private new Rigidbody rigidbody;
        [SerializeField]
        private CollisionEventsListener3D collisionEventsListener;
        
        private IPooledObject.PooledObjectDelegate<PooledObjectComponent, PooledObjectComponent.PooledObjectSpawnContext> _allocatedDelegate;
        private IPooledObject.PooledObjectDelegate<PooledObjectComponent, PooledObjectComponent.PooledObjectSpawnContext> _deallocatedDelegate;

        private CollisionEventsListener3D.CollisionEventsListenerGenericDelegate<
            CollisionEventsListener3D.CollisionEventContext> _collisionEnterEvent;
        private CollisionEventsListener3D.CollisionEventsListenerGenericDelegate<
            CollisionEventsListener3D.TriggerEventContext> _triggerEnterEvent;

        private PooledObjectComponent _pooledObject;
        private float _currentLifeLength = 0.0f;

        private bool _killedRobot = false;

        private Rigidbody Rigidbody => rigidbody ??= GetComponent<Rigidbody>();
        
        private void Awake()
        {
            _pooledObject = GetComponent<PooledObjectComponent>();
            _allocatedDelegate = OnAllocated;
            _deallocatedDelegate = OnDeAllocated;
            _collisionEnterEvent = OnCollisionEnter3D;
            _triggerEnterEvent = OnTriggerEnter3D;
            _currentLifeLength = maxLifeLength;
        }

        private void OnEnable()
        {
            _pooledObject.allocatedEvent
                += _allocatedDelegate;
            _pooledObject.deallocatedEvent
                += _deallocatedDelegate;

            if (collisionEventsListener)
            {
                collisionEventsListener.CollisionEnterEvent
                    += _collisionEnterEvent;
                collisionEventsListener.TriggerEnterEvent
                    += _triggerEnterEvent;
            }
        }

        private void OnDisable()
        {
            _pooledObject.allocatedEvent
                -= _allocatedDelegate;
            _pooledObject.deallocatedEvent
                -= _deallocatedDelegate;

            if (collisionEventsListener)
            {
                collisionEventsListener.CollisionEnterEvent
                    -= _collisionEnterEvent;
                collisionEventsListener.TriggerExitEvent
                    -= _triggerEnterEvent;
            }
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

            var cachedTransform = transform;
            var position = cachedTransform.position;
            cachedTransform.rotation = Quaternion.identity;
            
            var targetDirection = CalculateRandomDirection(position);
            var randomTravelTime = Random.Range(minTravelTime, maxTravelTime); 
            
            // Initializes the Chicken Target Pool.
            ChickenTarget.Initialize(32, targetEntity);

            if (Physics.Raycast(transform.position,
                    targetDirection, out var raycastHit, layerMask))
            {
                var point = raycastHit.point;
                var rotation = Quaternion.LookRotation(raycastHit.normal);
                ChickenTarget.Allocate(new ChickenTarget.ChickenTargetAllocContext
                {
                    position = point + raycastHit.normal * targetEntityNormalOffset,
                    rotation = rotation
                });
                cachedTransform.DOMove(point, randomTravelTime);
            }
        }
        
        private void OnDeAllocated(IObjectPool<PooledObjectComponent, PooledObjectComponent.PooledObjectSpawnContext> pool)
        {
            // Resets the rigidbody.
            if (Rigidbody)
            {
                Rigidbody.velocity = Vector3.zero;
                Rigidbody.angularVelocity = Vector3.zero;
                Rigidbody.Sleep();
            }
        }

        private Vector3 CalculateRandomDirection(Vector3 position)
        {
            var referenceDirection = Vector3.down;
            var droneEntity = RobotEntity.Get();
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

        private void OnTriggerEnter3D(CollisionEventsListener3D.TriggerEventContext context)
        {
            TryCollide(context.otherCollider.gameObject);
        }
        
        private void OnCollisionEnter3D(CollisionEventsListener3D.CollisionEventContext context)
        {
            TryCollide(context.collision.gameObject);
        }

        private void TryCollide(GameObject collidedGameObject)
        {
            if (!collidedGameObject)
            {
                return;
            }
            var robotEntity = collidedGameObject.GetComponent<RobotEntity>()
                              ?? collidedGameObject.GetComponentInParent<RobotEntity>();
            if (robotEntity
                && !_killedRobot)
            {
                robotEntity.Kill();
                _killedRobot = true;
            }
            // TODO: Explode
        }

        /// <summary>
        /// Deallocates the chicken. Equivalent to destroy/dispose.
        /// </summary>
        public void DeAllocate()
        {
            if (_pooledObject)
            {
                _pooledObject.DeAllocate();
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            var position = transform.position;
            Gizmos.DrawRay(position, CalculateRandomDirection(position));
        }

        private void OnValidate()
        {
            maxTravelTime = Mathf.Max(maxTravelTime, minTravelTime);
        }
    }
}