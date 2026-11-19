using LuffyAbilities.Core;
using LuffyAbilities.Visuals;
using UnityEngine;

namespace LuffyAbilities.Abilities
{
    /// <summary>
    /// Gear Third: hold an empty hand against your mouth (like blowing into your thumb) and an
    /// interaction circle appears. Press that hand's trigger to inflate both fists
    /// (more force / impact for punches and flings). Repeat to deflate.
    /// </summary>
    internal sealed class GearThird : Ability
    {
        private const float MouthRadius = 0.15f;
        private const float MouthHoldTime = 0.35f;
        private const float RingGraceTime = 0.75f;   // ring stays a moment after the hand leaves
        private const float RingDistance = 0.16f;

        private readonly HoldTimer leftHold = new HoldTimer();
        private readonly HoldTimer rightHold = new HoldTimer();
        private readonly MouthRing ring = new MouthRing();

        private HandContext ringHand;
        private float ringVisibleUntil;

        public GearThird(ModServices services) : base(services) { }

        public override void Update(float deltaTime)
        {
            // Fist size follows the gear (and live option changes).
            Services.LeftScaler.TargetInflation = Gears.FistScale;
            Services.RightScaler.TargetInflation = Gears.FistScale;

            if (!Options.Gear3Enabled)
            {
                if (Gears.Gear3) Gears.Set(Gear.Third, false);
                HideRing();
                return;
            }

            TrackMouthHold(Player.Left, leftHold, deltaTime);
            TrackMouthHold(Player.Right, rightHold, deltaTime);

            if (ringHand == null) return;

            if (IsAtMouth(ringHand)) ringVisibleUntil = Time.time + RingGraceTime;

            if (ringHand.TriggerDown)
            {
                Toggle(ringHand);
                HideRing();
            }
            else if (Time.time > ringVisibleUntil)
            {
                HideRing();
            }
        }

        private void TrackMouthHold(HandContext hand, HoldTimer hold, float deltaTime)
        {
            if (!hold.Tick(IsAtMouth(hand), MouthHoldTime, deltaTime)) return;
            ringHand = hand;
            ringVisibleUntil = Time.time + RingGraceTime;
            hand.Haptic(0.4f);
        }

        private bool IsAtMouth(HandContext hand) =>
            hand.IsFree && (hand.Position - Player.MouthPosition).sqrMagnitude < MouthRadius * MouthRadius;

        private void Toggle(HandContext hand)
        {
            bool activate = !Gears.Gear3;
            if (activate && !Services.Energy.HasEnergyFor(Player.Creature))
            {
                hand.Haptic(0.2f);
                return;
            }
            Gears.Set(Gear.Third, activate);
            hand.Haptic(1f);
        }

        public override void LateUpdate(float deltaTime)
        {
            if (ringHand == null) return;
            Transform head = Player.Head;
            Vector3 center = Player.MouthPosition + head.forward * RingDistance;
            Quaternion facing = Quaternion.LookRotation(head.forward, head.up);
            ring.Show(center, facing, Gears.Gear3 ? VisualFactory.RingOff : VisualFactory.RingOn);
        }

        private void HideRing()
        {
            ringHand = null;
            ring.Hide();
        }

        public override void Reset()
        {
            leftHold.Reset();
            rightHold.Reset();
            HideRing();
        }

        public override void Dispose()
        {
            Reset();
            ring.Destroy();
        }
    }
}
