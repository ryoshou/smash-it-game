using System.Collections.Generic;

namespace Game.Levels
{
    public readonly struct Reveal
    {
        public readonly bool HasNext;
        public readonly PropKind Next;
        public readonly float Progress01;

        public Reveal(bool hasNext, PropKind next, float progress01)
        {
            HasNext = hasNext;
            Next = next;
            Progress01 = progress01;
        }
    }

    /// <summary>
    /// Spec §4.5's "new thing to knock over" bar. A level introduces a kind if no level before it in the list has it.
    /// After winning level w, the bar spans from the last introducing level p ≤ w to the next one n > w:
    /// progress = (w − p + 1) / (n − p), so it is full on the level just before n.
    /// Every keycap size counts as one thing ("Keycap"): a wider key is not news to the player.
    /// </summary>
    public static class UnlockReveal
    {
        public static Reveal After(LevelList list, int wonIndex)
        {
            int count = list.Count;
            int w = ((wonIndex % count) + count) % count;
            var seen = new HashSet<PropKind>();
            int previous = 0;
            for (int i = 0; i < count; i++)
            {
                bool introduces = TryFirstNewKind(list.levels[i], seen, out PropKind first);
                if (!introduces) continue;
                if (i <= w)
                {
                    previous = i;
                    continue;
                }
                return new Reveal(true, first, (float)(w - previous + 1) / (i - previous));
            }
            return new Reveal(false, default, 1f);
        }

        /// <summary>The thing a kind is shown as: all keycap sizes are Keycap1u.</summary>
        public static PropKind Family(PropKind kind) => kind <= PropKind.KeycapEnter ? PropKind.Keycap1u : kind;

        /// <summary>Adds the level's kinds to seen; outputs the first new one in PropKind order.</summary>
        private static bool TryFirstNewKind(LevelDef level, HashSet<PropKind> seen, out PropKind first)
        {
            first = default;
            bool found = false;
            foreach (var prop in level.props)
            {
                var kind = Family(prop.kind);
                if (seen.Contains(kind)) continue;
                if (!found || kind < first) first = kind;
                found = true;
            }
            foreach (var prop in level.props) seen.Add(Family(prop.kind));
            return found;
        }
    }
}
