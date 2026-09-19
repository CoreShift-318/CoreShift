#if UNITY_2017_1_OR_NEWER
using CoreShift.Core;
using CoreShift.Core.Ecs;
using UnityEngine;
using CoreTransform = CoreShift.Core.Ecs.Transform2;

namespace CoreShift.Unity
{
    /// <summary>
    /// View for a single enemy entity. Instances are pooled by <see cref="EnemyViewPool"/>.
    /// </summary>
    public sealed class EnemyView : MonoBehaviour
    {
        private World _world;
        private CoreShiftRunner _runner;
        private Entity _entity;

        public Entity Entity => _entity;

        public void Bind(World world, CoreShiftRunner runner, Entity entity)
        {
            _world = world;
            _runner = runner;
            _entity = entity;
        }

        public bool IsAlive => _world != null && _world.Entities.IsAlive(_entity);

        public void SyncFromSimulation()
        {
            if (!IsAlive) return;
            if (!_world!.Entities.Has<CoreTransform>(_entity)) return;

            CoreTransform transform2 = _world.Entities.Get<CoreTransform>(_entity);
            Vector3 position = _runner is null
                ? new Vector3(transform2.X, transform2.Y, 0f)
                : _runner.ToWorldPosition(transform2.X, transform2.Y);
            transform.position = position;
        }
    }
}
#endif
