using System.Collections.Generic;
using MITHack.Robot.Utils;
using UnityEngine;

namespace MITHack.Robot.Game
{
    public class GameManager : GenericSingleton<GameManager>
    {
        #region defines
        
        public struct StatisticChangeEventContext<TStatistic>
            where TStatistic : unmanaged
        {
            public TStatistic prev;
            public TStatistic next;
        }
        
        public delegate void GameManagerGenericDelegate<in TContext>(TContext context);
        
        #endregion

        public GameManagerGenericDelegate<StatisticChangeEventContext<int>> LivesChangedEvent;

        [Header("Statistics")]
        [SerializeField, Min(1)]
        private int totalLives = 3;

        [Header("References")] 
        [SerializeField]
        private List<Spawner.Spawner> spawners;

        private int _chickensKilled = 0;
        private int _currentLives = 0;

        private bool _spawnersEnabled = true;

        public bool SpawnersEnabled => _spawnersEnabled;
        
        /// <summary>
        /// The current amount of lives.
        /// </summary>
        public int ChickensKilled => _chickensKilled;

        /// <summary>
        /// The total chickens killed.
        /// </summary>
        public int CurrentLives => _currentLives;

        public int TotalLives => totalLives;

        protected override void Awake()
        {
            base.Awake();
            
            _chickensKilled = 0;
            _currentLives = totalLives;
            SetSpawnersEnabled(true);
        }
        
        public void AddLives(int lives)
        {
            SetCurrentLives(_currentLives + lives);
        }

        public void RemoveLives(int lives)
        {
            SetCurrentLives(_currentLives - lives);
        }
        
        public void SetCurrentLives(int currentLives)
        {
            if (_currentLives != currentLives)
            {
                LivesChangedEvent?.Invoke(new StatisticChangeEventContext<int>()
                {
                    prev = _currentLives,
                    next = currentLives
                });
            }
            _currentLives = currentLives;
        }

        public void SetSpawnersEnabled(bool spawnersEnabled)
        {
            SetSpawnersEnabled(spawnersEnabled,  false);
        }

        private void SetSpawnersEnabled(bool spawnersEnabled, bool force)
        {
            if (!force && spawnersEnabled == _spawnersEnabled) return;
            foreach (var spawner in spawners)
            {
                if (spawner) spawner.enabled = spawnersEnabled;
            }
            _spawnersEnabled = spawnersEnabled;
        }

        public void InvokeOnRobotKilled()
        {
            RemoveLives(1);
            SetSpawnersEnabled(false);
            
            // TODO: Should probably 
        }
    }
}