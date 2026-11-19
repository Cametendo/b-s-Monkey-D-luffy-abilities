using System.Collections.Generic;
using LuffyAbilities.Core;
using ThunderRoad;
using UnityEngine;

namespace LuffyAbilities.Abilities
{
    /// <summary>
    /// Gear 3 close-range punches: the inflated fist hits a much bigger area with extra
    /// force on top of the game's normal hand collision (Gum-Gum Giant Pistol up close).
    /// </summary>
    internal sealed class GiantFistPunch : Ability
    {
        private const float MinPunchSpeed = 2.5f;
        private const float BaseRadius = 0.08f;
        private const float StrengthPerSpeed = 0.9f;
        private const float RehitDelay = 0.4f;

        private readonly Side side;
        private readonly GumGumFling fling;
        private readonly List<HitTarget> hits = new List<HitTarget>();
        private readonly Dictionary<Object, float> lastHitTime = new Dictionary<Object, float>();

        public GiantFistPunch(ModServices services, Side side, GumGumFling fling) : base(services)
        {
            this.side = side;
            this.fling = fling;
        }

        public override void FixedUpdate(float deltaTime)
        {
            if (!Gears.Gear3 || fling.IsBusy) return;

            HandContext hand = Player.GetHand(side);
            if (!hand.IsFree || hand.Speed < MinPunchSpeed) return;

            float radius = BaseRadius * Services.GetScaler(side).Inflation;
            if (Impact.Overlap(hand.Position, radius, Player.Creature, hits) == 0) return;

            float strength = hand.Speed * StrengthPerSpeed * Gears.ImpactMultiplier * Options.FlingForceScale;
            bool hitSomething = false;
            foreach (HitTarget target in hits)
            {
                Object key = target.Creature != null ? target.Creature : (Object)target.Body;
                if (key == null) continue;
                if (lastHitTime.TryGetValue(key, out float last) && Time.time - last < RehitDelay) continue;

                lastHitTime[key] = Time.time;
                Impact.Apply(target, hand.Velocity, strength);
                hitSomething = true;
            }

            if (hitSomething) hand.Haptic(1f);
            if (lastHitTime.Count > 64) PruneOldHits();
        }

        private readonly List<Object> expired = new List<Object>();

        private void PruneOldHits()
        {
            expired.Clear();
            foreach (KeyValuePair<Object, float> entry in lastHitTime)
                if (entry.Key == null || Time.time - entry.Value > RehitDelay) expired.Add(entry.Key);
            foreach (Object key in expired) lastHitTime.Remove(key);
        }

        public override void Reset()
        {
            hits.Clear();
            lastHitTime.Clear();
        }
    }
}
