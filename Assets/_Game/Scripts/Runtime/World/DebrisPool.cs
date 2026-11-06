using System.Collections.Generic;
using Game.Core;
using Game.Levels;
using UnityEngine;

namespace Game.World
{
    /// <summary>
    /// Pre-fractured sets, instantiated only in <see cref="Prewarm"/>. At most ShotTuning.maxDebrisSets are active
    /// (feastDebrisSets on a Feast level); beyond that the oldest set is recycled. Each set fades away and returns itself.
    /// </summary>
    public sealed class DebrisPool : MonoBehaviour
    {
        private sealed class Set
        {
            public int key;
            public GameObject root;
            public FadeOut fade;
            public Rigidbody[] pieces;
            public Vector3[] localPositions;
            public Quaternion[] localRotations;
            public int slot = DebrisBudget.None;
            public long activatedAt;
        }

        private const int VariantsPerKind = 16;

        private readonly Dictionary<int, List<Set>> sets = new Dictionary<int, List<Set>>();
        private DebrisBudget budget = new DebrisBudget(1);
        private Set[] slots = new Set[1];
        private ShotTuning tuning;
        private long clock;

        public int ActiveSets => budget.Active;

        public int Capacity => budget.Capacity;

        /// <summary>Makes sure there are enough sets for every shattering prop in the level (capped by the budget).</summary>
        public void Prewarm(PropCatalog catalog, LevelDef level, ShotTuning t)
        {
            DespawnAll();
            tuning = t;
            int capacity = level.difficulty == Difficulty.Feast ? t.feastDebrisSets : t.maxDebrisSets;
            if (budget.Capacity != Mathf.Max(1, capacity))
            {
                budget = new DebrisBudget(capacity);
                slots = new Set[budget.Capacity];
            }
            var needed = new Dictionary<int, int>();
            foreach (var placement in level.props)
            {
                var spec = catalog.Get(placement.kind);
                if (spec.breakMode != BreakMode.Shatter) continue;
                int key = Key(placement.kind, spec.VariantIndex(placement.variant));
                needed[key] = needed.TryGetValue(key, out int n) ? n + 1 : 1;
                if (!sets.ContainsKey(key)) sets[key] = new List<Set>();
            }
            foreach (var pair in needed)
            {
                var list = sets[pair.Key];
                var prefab = catalog.Get((PropKind)(pair.Key / VariantsPerKind)).Variant(pair.Key % VariantsPerKind).brokenPrefab;
                for (int i = list.Count; i < Mathf.Min(pair.Value, budget.Capacity); i++) list.Add(Create(pair.Key, prefab));
            }
        }

        /// <summary>Swaps in a broken set at the prop's pose, moving like the prop was, pieces pushed outward.</summary>
        public void Spawn(PropKind kind, int variant, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity)
        {
            if (!sets.TryGetValue(Key(kind, variant), out var list) || list.Count == 0) return;
            var set = FreeSet(list) ?? OldestActive(list);
            if (set.slot != DebrisBudget.None) Recycle(set);
            int slot = budget.Acquire(out int evicted);
            if (evicted != DebrisBudget.None) Recycle(slots[evicted], release: false);
            slots[slot] = set;
            set.slot = slot;
            Activate(set, position, rotation, velocity, angularVelocity);
        }

        public void DespawnAll()
        {
            for (int i = 0; i < slots.Length; i++)
                if (slots[i] != null) Recycle(slots[i], release: false);
            budget.ReleaseAll();
        }

        private static int Key(PropKind kind, int variant) => (int)kind * VariantsPerKind + variant;

        private Set Create(int key, GameObject prefab)
        {
            var root = Instantiate(prefab, transform);
            root.SetActive(false);
            var set = new Set { key = key, root = root, fade = root.GetComponent<FadeOut>() };
            set.pieces = root.GetComponentsInChildren<Rigidbody>(true);
            set.localPositions = new Vector3[set.pieces.Length];
            set.localRotations = new Quaternion[set.pieces.Length];
            for (int i = 0; i < set.pieces.Length; i++)
            {
                set.localPositions[i] = set.pieces[i].transform.localPosition;
                set.localRotations[i] = set.pieces[i].transform.localRotation;
            }
            set.fade.Finished += _ => Recycle(set);
            return set;
        }

        private void Activate(Set set, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity)
        {
            set.activatedAt = clock++;
            set.root.transform.SetPositionAndRotation(position, rotation);
            for (int i = 0; i < set.pieces.Length; i++)
            {
                var piece = set.pieces[i].transform;
                piece.localPosition = set.localPositions[i];
                piece.localRotation = set.localRotations[i];
            }
            set.root.SetActive(true);
            for (int i = 0; i < set.pieces.Length; i++)
            {
                var body = set.pieces[i];
                body.linearVelocity = velocity;
                body.angularVelocity = angularVelocity;
                var outward = body.worldCenterOfMass - position;
                if (outward.sqrMagnitude > 1e-6f)
                    body.AddForce(outward.normalized * tuning.breakScatterImpulse, ForceMode.Impulse);
            }
            set.fade.Begin(tuning.debrisHoldSeconds, tuning.debrisFadeSeconds);
        }

        private void Recycle(Set set, bool release = true)
        {
            if (set.slot == DebrisBudget.None) return;
            if (release) budget.Release(set.slot);
            slots[set.slot] = null;
            set.slot = DebrisBudget.None;
            set.fade.ResetScale();
            set.root.SetActive(false);
        }

        private static Set FreeSet(List<Set> list)
        {
            for (int i = 0; i < list.Count; i++)
                if (list[i].slot == DebrisBudget.None) return list[i];
            return null;
        }

        /// <summary>Every set of this kind is busy: the one activated first gets recycled.</summary>
        private static Set OldestActive(List<Set> list)
        {
            Set oldest = list[0];
            for (int i = 1; i < list.Count; i++)
                if (list[i].activatedAt < oldest.activatedAt) oldest = list[i];
            return oldest;
        }
    }
}
