using Game.Core;
using UnityEngine;

namespace Game.Juice
{
    /// <summary>A confetti burst over the desk when it is cleared.</summary>
    public sealed class WinFeedback : MonoBehaviour
    {
        private GameEvents events;
        private VfxPool vfx;
        private ParticleSystem confetti;
        private Transform anchor;

        public void Bind(GameEvents events, VfxPool vfx, ParticleSystem confetti, Transform anchor)
        {
            Unbind();
            this.events = events;
            this.vfx = vfx;
            this.confetti = confetti;
            this.anchor = anchor;
            vfx.Prewarm(confetti, 1);
            events.RoundEnded += OnRoundEnded;
        }

        private void OnDestroy() => Unbind();

        private void Unbind()
        {
            if (events == null) return;
            events.RoundEnded -= OnRoundEnded;
            events = null;
        }

        private void OnRoundEnded(RoundState state)
        {
            if (state == RoundState.Won) vfx.Play(confetti, anchor.position);
        }
    }
}
