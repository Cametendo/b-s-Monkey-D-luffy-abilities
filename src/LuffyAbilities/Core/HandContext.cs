using LuffyAbilities.Compat;
using ThunderRoad;
using UnityEngine;

namespace LuffyAbilities.Core
{
    /// <summary>
    /// Input + motion state of one player hand.
    /// Velocity is measured in the player's local space so walking, turning and
    /// Gear 2 speed never count as a "punch".
    /// </summary>
    internal sealed class HandContext
    {
        private Vector3 lastLocalPosition;
        private bool hasLastPosition;
        private bool lastGrip;
        private bool lastTrigger;

        public HandContext(Side side) { Side = side; }

        public Side Side { get; }
        public RagdollHand Hand { get; private set; }
        public PlayerControl.Hand Control { get; private set; }

        public Vector3 Position { get; private set; }
        /// <summary>World-space hand velocity relative to the player rig (m/s).</summary>
        public Vector3 Velocity { get; private set; }
        public float Speed => Velocity.magnitude;

        public bool Grip { get; private set; }
        public bool Trigger { get; private set; }
        public bool GripDown => Grip && !lastGrip;
        public bool TriggerDown => Trigger && !lastTrigger;

        /// <summary>Nothing in hand, not climbing, not holding something with telekinesis.</summary>
        public bool IsFree =>
            Hand != null && Hand.grabbedHandle == null
            && (Hand.climb == null || !Hand.climb.isGripping)
            && !GameCompat.IsUsingTelekinesis(Hand);

        /// <summary>A closed, empty fist.</summary>
        public bool IsFist => Grip && IsFree;

        public Transform Shoulder => Hand.upperArmPart != null ? Hand.upperArmPart.transform : Hand.transform;
        public Transform Elbow => Hand.lowerArmPart != null ? Hand.lowerArmPart.transform : Hand.transform;

        public void Reset()
        {
            hasLastPosition = false;
            Velocity = Vector3.zero;
            lastGrip = lastTrigger = Grip = Trigger = false;
        }

        public void Refresh(RagdollHand hand, Player player, float deltaTime)
        {
            Hand = hand;
            Control = hand.playerHand != null ? hand.playerHand.controlHand : null;

            lastGrip = Grip;
            lastTrigger = Trigger;
            Grip = Control != null && Control.gripPressed;
            Trigger = Control != null && Control.usePressed;

            Position = hand.transform.position;

            // Relative velocity: position in the player's local space, differentiated, back to world.
            Transform root = player.transform;
            Vector3 local = root.InverseTransformPoint(Position);
            if (hasLastPosition && deltaTime > 1e-5f)
                Velocity = root.TransformVector((local - lastLocalPosition) / deltaTime);
            else
                Velocity = Vector3.zero;
            lastLocalPosition = local;
            hasLastPosition = true;
        }

        public void Haptic(float intensity)
        {
            if (Options.Haptics) GameCompat.HapticShort(Control, intensity);
        }
    }
}
