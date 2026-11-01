using System.Collections.Generic;
using UnityEngine;

namespace Game.Juice
{
    /// <summary>Pre-instantiated particle effects. Play reuses an idle copy, or the next one in turn when all are busy.</summary>
    public sealed class VfxPool : MonoBehaviour
    {
        private sealed class Copies
        {
            public readonly List<ParticleSystem> items = new List<ParticleSystem>();
            public int next;
        }

        private readonly Dictionary<ParticleSystem, Copies> pools = new Dictionary<ParticleSystem, Copies>();

        public void Prewarm(ParticleSystem prefab, int count)
        {
            if (prefab == null) return;
            if (!pools.TryGetValue(prefab, out var copies))
            {
                copies = new Copies();
                pools[prefab] = copies;
            }
            for (int i = copies.items.Count; i < count; i++)
            {
                var item = Instantiate(prefab, transform);
                item.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                copies.items.Add(item);
            }
        }

        public void Play(ParticleSystem prefab, Vector3 position)
        {
            if (prefab == null || !pools.TryGetValue(prefab, out var copies) || copies.items.Count == 0) return;
            var item = Idle(copies);
            item.transform.position = position;
            item.Clear(true);
            item.Play(true);
        }

        public void StopAll()
        {
            foreach (var copies in pools.Values)
                foreach (var item in copies.items) item.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private static ParticleSystem Idle(Copies copies)
        {
            for (int i = 0; i < copies.items.Count; i++)
                if (!copies.items[i].IsAlive(true)) return copies.items[i];
            var item = copies.items[copies.next];
            copies.next = (copies.next + 1) % copies.items.Count;
            return item;
        }
    }
}
