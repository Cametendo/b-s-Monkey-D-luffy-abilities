using System;

namespace LuffyAbilities.Core
{
    internal enum Gear
    {
        Second,
        Third
    }

    /// <summary>
    /// Single source of truth for which gears are active and what that means for the
    /// other abilities (fling speed, impact, fist size). Abilities subscribe to
    /// <see cref="Changed"/> instead of polling each other.
    /// </summary>
    internal sealed class GearState
    {
        public bool Gear2 { get; private set; }
        public bool Gear3 { get; private set; }

        public int ActiveCount => (Gear2 ? 1 : 0) + (Gear3 ? 1 : 0);

        /// <summary>(gear, nowActive)</summary>
        public event Action<Gear, bool> Changed;

        public bool IsActive(Gear gear) => gear == Gear.Second ? Gear2 : Gear3;

        public void Set(Gear gear, bool active)
        {
            if (IsActive(gear) == active) return;
            if (gear == Gear.Second) Gear2 = active; else Gear3 = active;
            Changed?.Invoke(gear, active);
        }

        public void Toggle(Gear gear) => Set(gear, !IsActive(gear));

        public void DeactivateAll()
        {
            Set(Gear.Second, false);
            Set(Gear.Third, false);
        }

        // ------------------------------------------------------------- derived stats

        public float FlingSpeedMultiplier => Gear2 ? Options.Gear2FlingMultiplier : 1f;

        /// <summary>Gear 2 recovers faster between punches too.</summary>
        public float FlingCooldown => Gear2 ? 0.12f : 0.3f;

        public float ImpactMultiplier => Gear3 ? Options.Gear3ImpactMultiplier : 1f;

        public float FistScale => Gear3 ? Options.Gear3FistScale : 1f;
    }
}
