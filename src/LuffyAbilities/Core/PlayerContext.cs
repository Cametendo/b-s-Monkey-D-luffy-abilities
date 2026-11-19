using ThunderRoad;
using UnityEngine;

namespace LuffyAbilities.Core
{
    /// <summary>
    /// Per-frame snapshot of the local player's body: head, mouth, feet, hands and an
    /// auto-calibrated standing height. All gesture detection reads from here so that
    /// the "where is the player" logic exists exactly once.
    /// </summary>
    internal sealed class PlayerContext
    {
        private const float MinStandingHeight = 1.1f;
        private const float StandingHeightDecayPerSecond = 0.02f;

        public Player Player { get; private set; }
        public Creature Creature { get; private set; }
        public bool IsValid { get; private set; }

        public Transform Head { get; private set; }
        public Vector3 FeetPosition { get; private set; }
        public float HeadHeight { get; private set; }
        public float StandingHeight { get; private set; } = 1.6f;

        public HandContext Left { get; } = new HandContext(Side.Left);
        public HandContext Right { get; } = new HandContext(Side.Right);

        /// <summary>Approximate mouth position, slightly below and in front of the eyes.</summary>
        public Vector3 MouthPosition => Head.position - Head.up * 0.08f + Head.forward * 0.07f;

        /// <summary>Head forward projected on the ground plane.</summary>
        public Vector3 FlatForward
        {
            get
            {
                Vector3 f = Vector3.ProjectOnPlane(Head.forward, Vector3.up);
                return f.sqrMagnitude > 1e-4f ? f.normalized : Creature.transform.forward;
            }
        }

        public HandContext GetHand(Side side) => side == Side.Left ? Left : Right;

        /// <summary>Raised when the player body changed (level load, death, possession...).</summary>
        public event System.Action CreatureChanged;

        public void Refresh(float deltaTime)
        {
            Player player = Player.local;
            Creature creature = player != null ? player.creature : null;
            Transform head = player != null && player.head != null && player.head.cam != null ? player.head.cam.transform : null;

            bool valid = creature != null && head != null && !creature.isKilled
                         && creature.handLeft != null && creature.handRight != null;

            if (creature != Creature)
            {
                Creature = creature;
                StandingHeight = 1.6f;
                Left.Reset();
                Right.Reset();
                CreatureChanged?.Invoke();
            }

            Player = player;
            IsValid = valid;
            if (!valid) return;

            Head = head;
            FeetPosition = creature.transform.position;
            HeadHeight = head.position.y - FeetPosition.y;

            // Track the tallest recent head height as "standing" height, slowly forgetting it
            // so it adapts when the player sits down / changes play space.
            StandingHeight = Mathf.Max(MinStandingHeight,
                Mathf.Max(HeadHeight, StandingHeight * (1f - StandingHeightDecayPerSecond * deltaTime)));

            Left.Refresh(creature.handLeft, player, deltaTime);
            Right.Refresh(creature.handRight, player, deltaTime);
        }

        /// <summary>Height of a world point above the player's feet, relative to standing height (0 = floor, 1 = eyes).</summary>
        public float NormalizedHeight(Vector3 worldPoint) => (worldPoint.y - FeetPosition.y) / StandingHeight;

        public bool IsCrouching(float threshold) =>
            (Player != null && Player.crouching) || HeadHeight < StandingHeight * threshold;
    }
}
