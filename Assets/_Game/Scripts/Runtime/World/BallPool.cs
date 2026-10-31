using UnityEngine;

namespace Game.World
{
    /// <summary>A few reusable balls. When all are in flight, the oldest is recycled.</summary>
    public sealed class BallPool : MonoBehaviour
    {
        private const int Capacity = 4;

        [SerializeField] private YarnBall ballPrefab;

        private YarnBall[] balls;

        public bool AnyActive
        {
            get
            {
                EnsureBalls();
                foreach (var ball in balls)
                    if (ball.IsActive) return true;
                return false;
            }
        }

        public YarnBall Get()
        {
            EnsureBalls();
            YarnBall oldest = balls[0];
            foreach (var ball in balls)
            {
                if (!ball.IsActive) return ball;
                if (ball.Age > oldest.Age) oldest = ball;
            }
            oldest.Despawn();
            return oldest;
        }

        private void Awake() => EnsureBalls();

        private void EnsureBalls()
        {
            if (balls != null) return;
            balls = new YarnBall[Capacity];
            for (int i = 0; i < Capacity; i++)
            {
                balls[i] = Instantiate(ballPrefab, transform);
                balls[i].name = $"Ball {i}";
                balls[i].gameObject.SetActive(false);
            }
        }
    }
}
