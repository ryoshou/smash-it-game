using Game.Controls;
using Game.Core;
using Game.Juice;
using Game.Levels;
using Game.UI;
using Game.View;
using Game.World;
using UnityEngine;

namespace Game
{
    /// <summary>Owns the round: shot budget, judge, gesture and events. Wires input, world, view and UI together.</summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        private const int TargetFrameRate = 60;

        [SerializeField] private ShotTuning tuningAsset;
        [SerializeField] private LevelList levels;
        [SerializeField] private PropCatalog catalog;
        [SerializeField] private LevelSpawner spawner;
        [SerializeField] private BallPool pool;
        [SerializeField] private Transform pouch;
        [SerializeField] private Camera cam;
        [SerializeField] private SlingshotInput input;
        [SerializeField] private SlingshotView view;
        [SerializeField] private Hud hud;
        [SerializeField] private ResultPanel resultPanel;
        [SerializeField] private JuiceConfig juice;
        [SerializeField] private SfxPlayer sfx;
        [SerializeField] private CameraShake shake;
        [SerializeField] private ShotFeedback shotFeedback;
        [SerializeField] private ImpactFeedback impactFeedback;
        [SerializeField] private DebrisPool debris;
        [SerializeField] private PropFeelLibrary propFeel;
        [SerializeField] private VfxPool vfx;
        [SerializeField] private PropFeedback propFeedback;
        [SerializeField] private WinFeedback winFeedback;
        [SerializeField] private ParticleSystem confetti;
        [SerializeField] private Transform confettiAnchor;

        private RoundJudge judge;
        private SlingshotGesture gesture;
        private int levelIndex;
        private float lastFireTime = float.NegativeInfinity;
        private float lastPower = -1f;

        /// <summary>The runtime clone of the tuning asset; the debug panel edits this, never the asset.</summary>
        public ShotTuning Tuning { get; private set; }
        public GameEvents Events { get; private set; }
        public ShotBudget Budget { get; private set; }
        public RoundState State { get; private set; }

        public void LoadLevel(int index)
        {
            levelIndex = index;
            var level = levels.At(index);
            pool.DespawnAll();
            vfx.StopAll();
            gesture.Cancel();
            debris.Prewarm(catalog, level, Tuning);
            spawner.Load(level, catalog, Tuning, debris, Events);
            Budget.ResetAttempt(level.shots);
            judge.Reset();
            State = RoundState.Playing;
            lastFireTime = float.NegativeInfinity;
            resultPanel.Hide();
            hud.SetLevel(index + 1);
            hud.SetShots(Budget.Remaining);
        }

        /// <summary>Same path as a real release.</summary>
        public void FireForTest(Vector2 pull) => OnReleased(SlingshotMath.Compute(pull, Tuning));

        public void Next() => LoadLevel(levelIndex + 1);

        public void Retry() => LoadLevel(levelIndex);

        public void Continue()
        {
            if (State != RoundState.Lost || !Budget.TryContinue(Tuning.continueShots)) return;
            judge.Reset();
            State = RoundState.Playing;
            resultPanel.Hide();
            hud.SetShots(Budget.Remaining);
        }

        private void Awake()
        {
            Tuning = tuningAsset.Clone();
            Physics.gravity = new Vector3(0f, Tuning.gravityY, 0f);
            Application.targetFrameRate = TargetFrameRate;

            Events = new GameEvents();
            Budget = new ShotBudget(0);
            judge = new RoundJudge(Tuning.settleHoldSeconds, Tuning.settleMaxWaitSeconds);
            gesture = new SlingshotGesture(Tuning);

            input.Bind(gesture, CanGrab, pouch, cam);
            input.Released += OnReleased;
            view.Bind(gesture, Tuning);
            spawner.TargetCleared += Events.RaiseTargetCleared;
            shotFeedback.Bind(Events, juice, sfx);
            impactFeedback.Bind(Events, juice, sfx, shake);
            propFeedback.Bind(Events, juice, propFeel, sfx, vfx);
            winFeedback.Bind(Events, vfx, confetti, confettiAnchor);
        }

        private void Start() => LoadLevel(0);

        private void OnDestroy()
        {
            input.Released -= OnReleased;
            spawner.TargetCleared -= Events.RaiseTargetCleared;
            if (Tuning != null) Destroy(Tuning);
        }

        private void Update()
        {
            RaisePullChanges();
            if (State != RoundState.Playing) return;

            State = judge.Evaluate(Time.time, spawner.TargetsRemaining, Budget.Remaining, pool.AnyActive, spawner.AnyMoving());
            if (State == RoundState.Playing) return;

            gesture.Cancel();
            Events.RaiseRoundEnded(State);
            if (State == RoundState.Won) resultPanel.ShowWin(Next);
            else resultPanel.ShowLose(Budget.CanContinue, Continue, Retry);
        }

        private bool CanGrab() =>
            State == RoundState.Playing && Budget.Remaining > 0 && Time.time - lastFireTime >= Tuning.reloadSeconds;

        private void RaisePullChanges()
        {
            float power = gesture.Power01;
            if (Mathf.Approximately(power, lastPower)) return;
            lastPower = power;
            Events.RaisePullChanged(power);
        }

        private void OnReleased(LaunchResult result)
        {
            if (!result.Fired || State != RoundState.Playing || !Budget.TryConsume()) return;
            lastFireTime = Time.time;
            pool.Get().Launch(pouch.position, result.Velocity, Tuning, Events);
            Events.RaiseFired(result.Velocity);
            hud.SetShots(Budget.Remaining);
        }
    }
}
