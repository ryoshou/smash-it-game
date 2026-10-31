using Game.Audio;
using Game.Core;
using UnityEngine;

namespace Game.Juice
{
    /// <summary>Hit sounds by impact speed, a camera kick on big hits, and a haptic on the ball's first contact.</summary>
    public sealed class ImpactFeedback : MonoBehaviour
    {
        private readonly RateGate soundGate = new RateGate();
        private GameEvents events;
        private JuiceConfig config;
        private SfxPlayer sfx;
        private CameraShake shake;

        public void Bind(GameEvents events, JuiceConfig config, SfxPlayer sfx, CameraShake shake)
        {
            Unbind();
            this.events = events;
            this.config = config;
            this.sfx = sfx;
            this.shake = shake;
            events.Impact += OnImpact;
        }

        private void OnDestroy() => Unbind();

        private void Unbind()
        {
            if (events == null) return;
            events.Impact -= OnImpact;
            events = null;
        }

        private void OnImpact(Vector3 point, float speed, bool isFirstContact)
        {
            if (soundGate.TryPass(Time.unscaledTime, config.impactMinInterval))
                sfx.Play(speed >= config.hardHitSpeed ? SfxId.HitHard : SfxId.HitSoft, config.maxHitCopies);
            if (speed > config.shakeSpeed)
                shake.Kick(Mathf.Min(config.shakeMax, (speed - config.shakeSpeed) * config.shakePerSpeed));
            if (isFirstContact) HapticsBridge.Play(HapticKind.Medium, config.FirstContactHaptic);
        }
    }
}
