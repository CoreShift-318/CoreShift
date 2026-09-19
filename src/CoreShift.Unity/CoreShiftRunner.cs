#if UNITY_2017_1_OR_NEWER
using CoreShift.App;
using CoreShift.Core;
using CoreShift.Core.Systems;
using CoreShift.Core.Upgrades;
using CoreShift.Data.Content;
using UnityEngine;

namespace CoreShift.Unity
{
    /// <summary>
    /// Owns the engine-agnostic simulation and pumps it at a fixed timestep.
    /// Attach to a single empty GameObject in the scene.
    /// </summary>
    public sealed class CoreShiftRunner : MonoBehaviour
    {
        [Header("Simulation")]
        [SerializeField] private uint seed = 12345;
        [SerializeField] private float arenaWidth = 64f;
        [SerializeField] private float arenaHeight = 36f;
        [SerializeField] private float worldUnitsPerPixel = 1f / 32f;

        [Header("Views")]
        [SerializeField] private EnemyView enemyViewPrefab;
        [SerializeField] private PlayerView playerViewPrefab;

        private World _world;
        private UpgradeSystem _upgradeSystem;
        private float _accumulator;

        public World World => _world;

        private void Awake()
        {
            var content = GameContent.Default();
            _world = GameFactory.Build(seed, content);
            _upgradeSystem = null;
            foreach (var system in _world.Systems)
            {
                if (system is UpgradeSystem upgrades) _upgradeSystem = upgrades;
            }

            if (playerViewPrefab != null)
            {
                var player = Instantiate(playerViewPrefab);
                player.Bind(_world, this);
            }
        }

        private void Update()
        {
            ReadInput();

            _accumulator += Time.deltaTime;
            while (_accumulator >= World.FixedDeltaSeconds)
            {
                _world.Tick();
                _accumulator -= World.FixedDeltaSeconds;
            }

            if (_upgradeSystem != null && _upgradeSystem.HasOffers)
            {
                // UI hook: present offers, then call _upgradeSystem.Choose(_world, index).
                _upgradeSystem.Choose(_world, 0);
            }
        }

        private void ReadInput()
        {
            float x = 0f;
            float y = 0f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) y -= 1f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) y += 1f;
            _world.Input = InputState.Move(x, y);
        }

        public Vector3 ToWorldPosition(float x, float y) =>
            new Vector3(x / worldUnitsPerPixel, y / worldUnitsPerPixel, 0f);
    }
}
#endif
