using System;
using System.Collections.Generic;
using Game.Core;
using Game.Levels;
using UnityEngine;

namespace Game.World
{
    /// <summary>Spawns a level's props on the desk and counts targets as they are cleared.</summary>
    public sealed class LevelSpawner : MonoBehaviour
    {
        public const float DeskTopY = 1.0f;
        public static readonly Bounds PlayVolume = new Bounds(new Vector3(0f, 2f, 0f), new Vector3(8f, 6f, 8f));

        private readonly List<DeskObject> spawned = new List<DeskObject>();

        /// <summary>Targets remaining.</summary>
        public event Action<int> TargetCleared;

        public IReadOnlyList<DeskObject> Spawned => spawned;
        public int TargetsRemaining { get; private set; }

        /// <summary>True while any prop still on the desk is moving. Cleared props no longer count.</summary>
        public bool AnyMoving()
        {
            for (int i = 0; i < spawned.Count; i++)
                if (!spawned[i].IsCleared && spawned[i].IsMoving()) return true;
            return false;
        }

        public void Load(LevelDef level, PropCatalog catalog, ShotTuning t)
        {
            Clear();
            var deskTop = new Vector3(0f, DeskTopY, 0f);
            foreach (var placement in level.props)
            {
                var spec = catalog.Get(placement.kind);
                var position = deskTop + placement.basePosition + Vector3.up * (spec.size.y / 2f);
                var rotation = Quaternion.Euler(0f, placement.yawDegrees, 0f);
                var instance = Instantiate(spec.prefab, position, rotation, transform);
                spawned.Add(Configure(instance, spec, placement.isTarget, t));
                if (placement.isTarget) TargetsRemaining++;
            }
        }

        public void Clear()
        {
            foreach (var prop in spawned)
            {
                prop.Cleared -= OnCleared;
                Destroy(prop.gameObject);
            }
            spawned.Clear();
            TargetsRemaining = 0;
        }

        private DeskObject Configure(GameObject instance, PropSpec spec, bool isTarget, ShotTuning t)
        {
            var prop = instance.GetComponent<DeskObject>();
            prop.Body.mass = spec.mass;
            if (spec.material != null && instance.TryGetComponent(out Collider collider)) collider.sharedMaterial = spec.material;
            prop.Init(isTarget, DeskTopY, PlayVolume, t);
            prop.Cleared += OnCleared;
            return prop;
        }

        private void OnCleared(DeskObject prop)
        {
            if (!prop.IsTarget) return;
            TargetsRemaining--;
            TargetCleared?.Invoke(TargetsRemaining);
        }
    }
}
