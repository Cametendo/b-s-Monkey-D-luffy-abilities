using LuffyAbilities.Compat;
using ThunderRoad;
using UnityEngine;

namespace LuffyAbilities.Core
{
    /// <summary>
    /// Drains the player's casting resource (Mana in U12, Focus in 1.0) while gears are active.
    ///
    /// Each active gear drains <c>max / GearDurationSeconds</c> per second, so from 100%:
    ///   Gear 2 alone  -> 60 s,  Gear 3 alone -> 60 s,  Gear 2 + Gear 3 -> 30 s.
    /// Natural regeneration is suppressed while a gear runs, so those numbers are real maximums.
    /// With infinite Mana/Focus nothing is drained.
    ///
    /// The Gum-Gum fling costs <see cref="FlingCost"/> - effectively free, but it still
    /// goes through the same resource path.
    /// </summary>
    internal sealed class GearEnergy
    {
        /// <summary>Practically nothing (0.00000000000000000000001 % of a full bar).</summary>
        public const float FlingCostFraction = 1e-25f;

        private readonly GearState gears;
        private float lastValue = -1f;

        public GearEnergy(GearState gears) { this.gears = gears; }

        public bool HasEnergyFor(Creature creature)
        {
            if (GameCompat.ResourceIsInfinite) return true;
            Mana mana = creature != null ? creature.mana : null;
            if (mana == null) return true; // no resource system -> unlimited
            return GameCompat.GetResource(mana) > 0.01f * GameCompat.GetMaxResource(mana);
        }

        /// <summary>Charge the tiny fling cost. Never blocks the fling.</summary>
        public void PayFling(Creature creature)
        {
            Mana mana = creature != null ? creature.mana : null;
            if (mana == null || GameCompat.ResourceIsInfinite) return;
            float cost = GameCompat.GetMaxResource(mana) * FlingCostFraction;
            GameCompat.SetResource(mana, GameCompat.GetResource(mana) - cost);
        }

        /// <summary>Call every frame. Returns false when the resource ran out (gears were switched off).</summary>
        public bool Tick(Creature creature, float deltaTime)
        {
            int active = gears.ActiveCount;
            Mana mana = creature != null ? creature.mana : null;

            if (active == 0 || mana == null || GameCompat.ResourceIsInfinite)
            {
                lastValue = -1f;
                return true;
            }

            float max = GameCompat.GetMaxResource(mana);
            float current = GameCompat.GetResource(mana);

            // Ignore regeneration that happened since our last write, but respect other spending.
            if (lastValue >= 0f) current = Mathf.Min(current, lastValue);

            float duration = Mathf.Max(1f, Options.GearDurationSeconds);
            float next = Mathf.Max(0f, current - max * active / duration * deltaTime);
            GameCompat.SetResource(mana, next);
            lastValue = next;

            if (next > 0f) return true;

            gears.DeactivateAll();
            lastValue = -1f;
            return false;
        }

        public void Reset() => lastValue = -1f;
    }
}
