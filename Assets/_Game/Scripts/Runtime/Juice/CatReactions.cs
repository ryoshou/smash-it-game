using Game.Core;
using UnityEngine;

namespace Game.Juice
{
    /// <summary>Drives the cat's Animator from gameplay events: lean back while pulling, release, smug or annoyed.</summary>
    public sealed class CatReactions : MonoBehaviour
    {
        private static readonly int Aiming = Animator.StringToHash("Aiming");
        private static readonly int Pull = Animator.StringToHash("Pull");
        private static readonly int Release = Animator.StringToHash("Release");
        private static readonly int Celebrate = Animator.StringToHash("Celebrate");
        private static readonly int Annoyed = Animator.StringToHash("Annoyed");

        private GameEvents events;
        private Animator animator;

        public void Bind(GameEvents gameEvents, Animator catAnimator)
        {
            Unbind();
            events = gameEvents;
            animator = catAnimator;
            events.PullChanged += OnPullChanged;
            events.Fired += OnFired;
            events.ShotResolved += OnShotResolved;
            events.RoundEnded += OnRoundEnded;
        }

        private void OnDestroy() => Unbind();

        private void Unbind()
        {
            if (events == null) return;
            events.PullChanged -= OnPullChanged;
            events.Fired -= OnFired;
            events.ShotResolved -= OnShotResolved;
            events.RoundEnded -= OnRoundEnded;
            events = null;
        }

        private void OnPullChanged(float power01)
        {
            animator.SetBool(Aiming, power01 > 0f);
            animator.SetFloat(Pull, power01);
        }

        private void OnFired(Vector3 velocity)
        {
            animator.SetBool(Aiming, false);
            animator.SetTrigger(Release);
        }

        private void OnShotResolved(int cleared) => React(CatReactionRule.For(cleared, RoundState.Playing));

        private void OnRoundEnded(RoundState state) => React(CatReactionRule.For(0, state));

        private void React(CatReaction reaction)
        {
            if (reaction == CatReaction.Celebrate) animator.SetTrigger(Celebrate);
            else if (reaction == CatReaction.Annoyed) animator.SetTrigger(Annoyed);
        }
    }
}
