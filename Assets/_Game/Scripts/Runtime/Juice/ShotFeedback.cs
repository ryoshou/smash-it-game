using Game.Audio;
using Game.Core;
using UnityEngine;

namespace Game.Juice
{
    /// <summary>Pull ticks, the release twang, target pops and the win/lose stings.</summary>
    public sealed class ShotFeedback : MonoBehaviour
    {
        private GameEvents events;
        private JuiceConfig config;
        private SfxPlayer sfx;
        private int lastPullStep;

        public void Bind(GameEvents events, JuiceConfig config, SfxPlayer sfx)
        {
            Unbind();
            this.events = events;
            this.config = config;
            this.sfx = sfx;
            events.PullChanged += OnPullChanged;
            events.Fired += OnFired;
            events.TargetCleared += OnTargetCleared;
            events.RoundEnded += OnRoundEnded;
        }

        private void OnDestroy() => Unbind();

        private void Unbind()
        {
            if (events == null) return;
            events.PullChanged -= OnPullChanged;
            events.Fired -= OnFired;
            events.TargetCleared -= OnTargetCleared;
            events.RoundEnded -= OnRoundEnded;
            events = null;
        }

        private void OnPullChanged(float power01)
        {
            int step = Mathf.FloorToInt(power01 / config.pullTickStep);
            if (step == lastPullStep) return;
            lastPullStep = step;
            if (power01 <= 0f) return;
            sfx.Play(SfxId.Pull, config.maxOtherCopies, Mathf.Lerp(config.pullPitchMin, config.pullPitchMax, power01));
        }

        private void OnFired(Vector3 velocity)
        {
            lastPullStep = 0;
            sfx.Play(SfxId.Twang, config.maxOtherCopies);
            HapticsBridge.Play(HapticKind.Light, config.FiredHaptic);
        }

        private void OnTargetCleared(int remaining) => sfx.Play(SfxId.Cleared, config.maxHitCopies);

        private void OnRoundEnded(RoundState state)
        {
            if (state == RoundState.Won)
            {
                sfx.Play(SfxId.Win, 1);
                HapticsBridge.Play(HapticKind.Heavy, config.WinHaptic);
            }
            else if (state == RoundState.Lost)
            {
                sfx.Play(SfxId.Lose, 1);
            }
        }
    }
}
