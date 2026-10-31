namespace Game.Core
{
    /// <summary>When a fired ball goes back to the pool: too old, fell off the world, or came to rest.</summary>
    public static class BallLife
    {
        public static bool ShouldDespawn(float age, float speed, float y, ShotTuning t) =>
            age >= t.ballLifetimeSeconds || y < t.ballKillY || (age >= t.ballRestMinAge && speed < t.ballRestSpeed);
    }
}
