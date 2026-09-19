#if UNITY_2017_1_OR_NEWER
using CoreShift.Core;
using CoreShift.Core.Ecs;
using UnityEngine;
using CoreTransform = CoreShift.Core.Ecs.Transform2;

namespace CoreShift.Unity
{
    /// <summary>
    /// Renders the player entity from the simulation. Purely presentational: it reads
    /// component data and moves the GameObject; it never changes game state.
    /// </summary>
    public sealed class PlayerView : MonoBehaviour
    {
        private World _world;
        private CoreShiftRunner _runner;

        public void Bind(World world, CoreShiftRunner runner)
        {
            _world = world;
            _runner = runner;
        }

        private void LateUpdate()
        {
            if (_world is null || !_world.HasPlayer) return;

            Entity player = _world.Player;
            if (!_world.Entities.Has<CoreTransform>(player)) return;

            CoreTransform transform2 = _world.Entities.Get<CoreTransform>(player);
            Vector3 position = _runner is null
                ? new Vector3(transform2.X, transform2.Y, 0f)
                : _runner.ToWorldPosition(transform2.X, transform2.Y);
            transform.position = position;
        }
    }
}
#endif
