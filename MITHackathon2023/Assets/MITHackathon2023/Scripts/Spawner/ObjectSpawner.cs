using System.Collections.Generic;

namespace MITHack.Robot.Spawner
{
    public interface IPooledObject
    {
        public void OnSpawnedByPool();
        public void OnDeSpawnedByPool();
    }

    /// <summary>
    /// Interface for spawning in the object.
    /// </summary>
    /// <typeparam name="TObject">The associated object.</typeparam>
    public interface IObjectSpawnerInstance<out TObject>
        where TObject : class
    {
        public TObject Create();
    }
    
    public class ObjectSpawner<TObjectSpawnerInstance, TObject>
        where TObject : class
        where TObjectSpawnerInstance : struct, IObjectSpawnerInstance<TObject>
    {
        private struct SpawnedObject
        {
            public TObject @object;
            public bool enabled;
        }

        private readonly Dictionary<TObject, SpawnedObject> _spawnedObjects = new();
        
        private TObjectSpawnerInstance _spawner;
        private readonly int _capacity;

        private bool _initialized = false;
        
        public ObjectSpawner(int capacity, in TObjectSpawnerInstance spawner)
        {
            _spawner = spawner;
            _capacity = capacity;
        }

        public void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            for (var index = 0; index < _capacity; ++index)
            {
                var instantiated = _spawner.Create();
                if (instantiated == null) continue;
                
                _spawnedObjects.Add(instantiated, new SpawnedObject
                {
                    enabled = false,
                    @object = instantiated
                });
            }
            _initialized = true;
        }
        
        public bool Alloc(ref TObject obj)
        {
            if (!_initialized)
            {
                return false;
            }

            return true;
        }

        public void DeAlloc(TObject obj)
        {
            
        }
    }
}