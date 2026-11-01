using Game.Levels;
using UnityEngine;

namespace Game.Juice
{
    /// <summary>Per-kind feel. Lives on the Juice side so gameplay data never references sounds or effects.</summary>
    [CreateAssetMenu(menuName = "Smash/Prop Feel Library")]
    public sealed class PropFeelLibrary : ScriptableObject
    {
        public PropFeel[] feels = new PropFeel[0];

        public bool TryGet(PropKind kind, out PropFeel feel)
        {
            if (feels != null)
                for (int i = 0; i < feels.Length; i++)
                    if (feels[i].kind == kind)
                    {
                        feel = feels[i];
                        return true;
                    }
            feel = default;
            return false;
        }
    }
}
