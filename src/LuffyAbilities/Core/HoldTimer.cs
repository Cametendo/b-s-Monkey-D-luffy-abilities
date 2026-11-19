namespace LuffyAbilities.Core
{
    /// <summary>
    /// "Hold a pose for N seconds" helper. Fires once per hold; the pose must be released
    /// before it can fire again, so a held pose never toggles something on and off repeatedly.
    /// </summary>
    internal sealed class HoldTimer
    {
        private float elapsed;
        private bool fired;

        /// <summary>0..1 progress of the current hold (for haptics / visuals).</summary>
        public float Progress { get; private set; }

        /// <summary>The pose is being held and has not fired yet.</summary>
        public bool IsCharging => Progress > 0f && !fired;

        /// <summary>Returns true exactly once when the condition has been held for <paramref name="duration"/> seconds.</summary>
        public bool Tick(bool condition, float duration, float deltaTime)
        {
            if (!condition)
            {
                Reset();
                return false;
            }

            if (fired) return false;

            elapsed += deltaTime;
            Progress = duration <= 0f ? 1f : UnityEngine.Mathf.Clamp01(elapsed / duration);
            if (elapsed < duration) return false;

            fired = true;
            return true;
        }

        public void Reset()
        {
            elapsed = 0f;
            Progress = 0f;
            fired = false;
        }
    }
}
