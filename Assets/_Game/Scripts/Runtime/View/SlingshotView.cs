using Game.Core;
using Game.Juice;
using UnityEngine;

namespace Game.View
{
    /// <summary>Draws the bands, moves the pouch with the pull and places the aim-preview dots.</summary>
    public sealed class SlingshotView : MonoBehaviour
    {
        [SerializeField] private JuiceConfig juice;
        [SerializeField] private Transform pouchRest;
        [SerializeField] private Transform pouchVisual;
        [SerializeField] private Transform forkLeftTip;
        [SerializeField] private Transform forkRightTip;
        [SerializeField] private LineRenderer bandLeft;
        [SerializeField] private LineRenderer bandRight;
        [SerializeField] private Transform[] dots;
        [Tooltip("The bands meet the loaded ball this far either side of its centre, so they hold it rather than pierce it.")]
        [SerializeField] private float bandGrip;

        private SlingshotGesture gesture;
        private ShotTuning tuning;
        private Vector3[] dotPositions;
        private bool dotsVisible = true;

        public void Bind(SlingshotGesture gesture, ShotTuning tuning)
        {
            this.gesture = gesture;
            this.tuning = tuning;
        }

        private void Awake()
        {
            dotPositions = new Vector3[dots.Length];
            SetDotsVisible(false);
        }

        private void LateUpdate()
        {
            Vector2 pull = gesture != null && gesture.IsHolding ? gesture.Pull : Vector2.zero;
            pouchVisual.position = pouchRest.position + new Vector3(pull.x, pull.y * 0.5f, pull.y) * juice.pouchTravel;
            bandLeft.SetPosition(0, forkLeftTip.position);
            var side = transform.right * bandGrip;
            bandLeft.SetPosition(1, pouchVisual.position - side);
            bandRight.SetPosition(0, forkRightTip.position);
            bandRight.SetPosition(1, pouchVisual.position + side);
            UpdateDots(pull);
        }

        private void UpdateDots(Vector2 pull)
        {
            var launch = gesture != null && gesture.IsHolding ? SlingshotMath.Compute(pull, tuning) : LaunchResult.None;
            SetDotsVisible(launch.Fired);
            if (!launch.Fired) return;

            int steps = TrajectoryMath.StepsPerDot(tuning.previewSeconds, dots.Length, Time.fixedDeltaTime);
            TrajectoryMath.Sample(pouchRest.position, launch.Velocity, Physics.gravity, Time.fixedDeltaTime, steps, dotPositions);
            for (int i = 0; i < dots.Length; i++) dots[i].position = dotPositions[i];
        }

        private void SetDotsVisible(bool visible)
        {
            if (visible == dotsVisible) return;
            dotsVisible = visible;
            foreach (var dot in dots) dot.gameObject.SetActive(visible);
        }
    }
}
