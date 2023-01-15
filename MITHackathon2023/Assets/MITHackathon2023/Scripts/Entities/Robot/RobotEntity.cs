using System;
using MITHack.Robot.Game;
using MITHack.Robot.Spawner;
using MITHack.Robot.Utils;
using MITHack.Robot.Utils.Components;
using UnityEngine;

namespace MITHack.Robot.Entities
{
    public class RobotEntity : GenericSingleton<RobotEntity>
    {
        #region defines
        
        public enum RobotEntityState
        {
            StateAlive,
            StateDead
        }

        public struct RobotEntityStateChangeContext
        {
            public RobotEntityState prev;
            public RobotEntityState next;
        }
        
        public delegate void RobotEntityGenericDelegate<in TContext>(TContext context);
        
        #endregion

        public RobotEntityGenericDelegate<RobotEntityStateChangeContext> StateChangedEvent;

        [Header("Projectile")] 
        [SerializeField, Min(0.0f)]
        private float projectileTimeBetweenEachFire = 0.2f;
        [Space]
        [SerializeField]
        private Prefab<PooledObjectComponent> projectilePrefab;
        [SerializeField]
        private Transform shootLocation;

        private ObjectPoolInstance<Prefab<PooledObjectComponent>, PooledObjectComponent, PooledObjectComponent.PooledObjectSpawnContext> _objectPool;
        private RobotEntityState _robotEntityState = RobotEntityState.StateAlive;

        private float _currentProjectileTime = 0.0f;
        
        public RobotEntityState EntityState => _robotEntityState;

        protected override void Awake()
        {
            base.Awake();

            _objectPool ??=
                new ObjectPoolInstance<Prefab<PooledObjectComponent>, PooledObjectComponent,
                    PooledObjectComponent.PooledObjectSpawnContext>(64, projectilePrefab);
        }

        private void Start()
        {
            _objectPool?.Initialize();
            _currentProjectileTime = projectileTimeBetweenEachFire;
        }

        private void OnDestroy()
        {
            _objectPool.DeInitialize();
        }

        private void Update()
        {
            switch (EntityState)
            {
                case RobotEntityState.StateAlive:
                {
                    if (_currentProjectileTime > 0.0f)
                    {
                        _currentProjectileTime -= Time.deltaTime;
                
                        if (_currentProjectileTime <= 0.0f)
                        {
                            _currentProjectileTime = projectileTimeBetweenEachFire;
                            Fire();
                        }
                    }
                    break;
                }
            }
        }

        private void SetState(RobotEntityState entityState)
        {
            if (_robotEntityState != entityState)
            {
                StateChangedEvent?.Invoke(new RobotEntityStateChangeContext
                {
                    prev = _robotEntityState,
                    next = entityState
                });
                _robotEntityState = entityState;
            }
        }
        
        public void Kill()
        {
            if (EntityState != RobotEntityState.StateAlive)
            {
                return;
            }
            SetState(RobotEntityState.StateDead);
            // TODO: Animation
            Revive();
        }

        public void Revive()
        {
            SetState(RobotEntityState.StateAlive);
        }

        public void Fire()
        {
            var location = shootLocation ? shootLocation : transform;
            PooledObjectComponent objectPooledReference = null;
            _objectPool?.Allocate(ref objectPooledReference, new PooledObjectComponent.PooledObjectSpawnContext
                {
                    position = location.position,
                    rotation = location.rotation
                });
        }
        
        private void OnDrawGizmos()
        {
            var gameManager = GameManager.Get();
            if (gameManager)
            {
                var currentLives = gameManager.CurrentLives;
                switch (currentLives)
                {
                    case 0:
                    {
                        Gizmos.color = Color.red;
                        break;
                    }
                    case 1:
                    {
                        Gizmos.color = new Color(1.0f,115.0f / 255.0f,0);
                        break;
                    }
                    case 2:
                    {
                        Gizmos.color = new Color(1.0f, 213.0f / 255.0f, 0.0f);
                        break;
                    }
                    case 3:
                    {
                        Gizmos.color = new Color(0.0f, 1.0f, 38.0f / 255.0f);
                        break;
                    }
                }
                Gizmos.DrawSphere(transform.position, 0.2f);
            }
        }
    }
}