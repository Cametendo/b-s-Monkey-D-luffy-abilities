using System.Collections.Generic;
using LuffyAbilities.Core;
using LuffyAbilities.Visuals;
using ThunderRoad;
using UnityEngine;

namespace LuffyAbilities.Abilities
{
    /// <summary>
    /// Gear Second: crouch and hold an empty fist down at your knee for ~1 s to toggle.
    /// While active: much faster movement + jumps, faster stretched punches, steam from the arms.
    /// </summary>
    internal sealed class GearSecond : Ability
    {
        private const float CrouchThreshold = 0.78f;   // head below 78% of standing height
        private const float KneeHeight = 0.42f;        // fist below 42% of standing height
        private const float JumpMultiplier = 1.6f;

        private readonly HoldTimer hold = new HoldTimer();
        private readonly List<ParticleSystem> steam = new List<ParticleSystem>();
        private Creature steamOwner;

        public GearSecond(ModServices services) : base(services)
        {
            Gears.Changed += OnGearChanged;
        }

        public override void Update(float deltaTime)
        {
            if (!Options.Gear2Enabled)
            {
                if (Gears.Gear2) Gears.Set(Gear.Second, false);
                hold.Reset();
                return;
            }

            HandContext hand = FindPosingHand();
            if (hold.Tick(hand != null, Options.Gear2HoldTime, deltaTime))
            {
                Toggle(hand);
                return;
            }

            // Ramp up vibration while holding the pose so the player feels it charging.
            if (hand != null && hold.IsCharging && Time.frameCount % 6 == 0) hand.Haptic(0.1f + hold.Progress * 0.4f);
        }

        private HandContext FindPosingHand()
        {
            if (!Player.IsCrouching(CrouchThreshold)) return null;
            if (IsKneeFist(Player.Left)) return Player.Left;
            if (IsKneeFist(Player.Right)) return Player.Right;
            return null;
        }

        private bool IsKneeFist(HandContext hand) =>
            hand.IsFist && hand.Speed < 1f && Player.NormalizedHeight(hand.Position) < KneeHeight;

        private void Toggle(HandContext hand)
        {
            bool activate = !Gears.Gear2;
            if (activate && !Services.Energy.HasEnergyFor(Player.Creature))
            {
                hand.Haptic(0.2f);
                return;
            }
            Gears.Set(Gear.Second, activate);
            hand.Haptic(1f);
        }

        private void OnGearChanged(Gear gear, bool active)
        {
            if (gear != Gear.Second) return;
            ApplySpeed(active);
            SetSteam(active);
        }

        private void ApplySpeed(bool active)
        {
            Locomotion locomotion = Player.Player != null ? Player.Player.locomotion : null;
            if (locomotion == null) return;

            if (active)
            {
                float m = Options.Gear2MoveMultiplier;
                locomotion.SetSpeedModifier(this, m, m, m, m, JumpMultiplier, m);
            }
            else
            {
                locomotion.RemoveSpeedModifier(this);
            }
        }

        private void SetSteam(bool active)
        {
            if (active && Options.Gear2Steam) EnsureSteam();
            foreach (ParticleSystem ps in steam)
            {
                if (ps == null) continue;
                if (active && Options.Gear2Steam) ps.Play(true);
                else ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        private void EnsureSteam()
        {
            Creature creature = Player.Creature;
            if (creature == null || (steamOwner == creature && steam.Count > 0)) return;

            DestroySteam();
            steamOwner = creature;
            foreach (RagdollHand hand in new[] { creature.handLeft, creature.handRight })
            {
                if (hand == null) continue;
                if (hand.lowerArmPart != null) steam.Add(VisualFactory.CreateSteam("LuffyAbilities_Steam", hand.lowerArmPart.transform));
                if (hand.upperArmPart != null) steam.Add(VisualFactory.CreateSteam("LuffyAbilities_Steam", hand.upperArmPart.transform));
            }
        }

        private void DestroySteam()
        {
            foreach (ParticleSystem ps in steam)
                if (ps != null) Object.Destroy(ps.gameObject);
            steam.Clear();
            steamOwner = null;
        }

        public override void Reset()
        {
            hold.Reset();
            DestroySteam();
        }

        public override void Dispose()
        {
            Reset();
            Gears.Changed -= OnGearChanged;
        }
    }
}
