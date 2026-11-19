using LuffyAbilities.Compat;
using LuffyAbilities.Core;
using LuffyAbilities.Visuals;
using ThunderRoad;
using UnityEngine;

namespace LuffyAbilities.Abilities
{
    /// <summary>
    /// Gum-Gum Pistol for one hand: make an empty fist and punch fast -> the arm stretches
    /// forward, hits whatever is in the way, and snaps back.
    /// Hitting a wall while still holding the fist = Gum-Gum Rocket (pulls you there).
    /// Gear 2 makes the fist travel faster, Gear 3 makes it bigger and hit harder.
    /// </summary>
    internal sealed class GumGumFling : Ability
    {
        private enum State { Idle, Extending, Anchored, Retracting }

        private const float BaseTravelSpeed = 30f;      // m/s
        private const float RetractSpeedFactor = 1.4f;
        private const float RearmSpeed = 1.5f;          // hand must slow below this before the next fling
        private const float MinForwardDot = 0.2f;       // ignore back-swings
        private const float BaseFistRadius = 0.07f;
        private const float BaseStrength = 7f;          // ~m/s knock-back
        private const float AnchorHoldTime = 0.2f;
        private const float RocketMinSpeed = 6f;
        private const float RocketMaxSpeed = 16f;

        private readonly Side side;
        private readonly RubberArm arm;

        private State state = State.Idle;
        private bool armed = true;
        private float cooldownUntil;
        private float anchoredUntil;
        private float travelSpeed;
        private float travelled;
        private Vector3 direction;
        private Vector3 tip;

        public GumGumFling(ModServices services, Side side) : base(services)
        {
            this.side = side;
            arm = new RubberArm("LuffyAbilities_" + side);
            Gears.Changed += OnGearChanged;
        }

        public bool IsBusy => state != State.Idle;

        private HandContext Hand => Player.GetHand(side);
        private HandScaler Scaler => Services.GetScaler(side);
        private float FistScale => Scaler.Inflation;

        // ------------------------------------------------------------------ detection (Update)

        public override void Update(float deltaTime)
        {
            HandContext hand = Hand;
            if (hand.Speed < RearmSpeed) armed = true;

            if (state != State.Idle || !Options.FlingEnabled || !armed || Time.time < cooldownUntil) return;
            if (!hand.IsFist || hand.Speed < Options.FlingPunchSpeed) return;

            Vector3 velocityDir = hand.Velocity.normalized;
            if (Vector3.Dot(velocityDir, Player.Head.forward) < MinForwardDot) return;

            Launch(hand, velocityDir);
        }

        private void Launch(HandContext hand, Vector3 velocityDir)
        {
            // Mostly the punch direction, nudged toward the elbow->hand line for stable aim.
            Vector3 forearm = (hand.Position - hand.Elbow.position).normalized;
            direction = (velocityDir * 0.75f + forearm * 0.25f).normalized;

            tip = hand.Position;
            travelled = 0f;
            travelSpeed = BaseTravelSpeed * Gears.FlingSpeedMultiplier;
            armed = false;
            state = State.Extending;

            Scaler.Hidden = true;
            arm.SetVisible(true);
            Services.Energy.PayFling(Player.Creature);
            hand.Haptic(0.6f);
        }

        // ------------------------------------------------------------------ physics (FixedUpdate)

        public override void FixedUpdate(float deltaTime)
        {
            switch (state)
            {
                case State.Extending: Extend(deltaTime); break;
                case State.Anchored: if (Time.time >= anchoredUntil) state = State.Retracting; break;
                case State.Retracting: Retract(deltaTime); break;
            }
        }

        private void Extend(float deltaTime)
        {
            float step = travelSpeed * deltaTime;
            float radius = BaseFistRadius * FistScale;

            if (Impact.SphereCast(tip, radius, direction, step, Player.Creature, out HitTarget target))
            {
                tip = target.Point - direction * radius;
                OnHit(target);
                return;
            }

            tip += direction * step;
            travelled += step;
            if (travelled >= Options.FlingReach) state = State.Retracting;
        }

        private void OnHit(HitTarget target)
        {
            HandContext hand = Hand;

            if (target.Kind == HitKind.World)
            {
                if (Options.RocketEnabled && hand.Grip) Rocket(target.Point);
                else state = State.Retracting;
                hand.Haptic(0.5f);
                return;
            }

            // Faster fists (Gear 2) hit a little harder too; Gear 3 is the big multiplier.
            float strength = BaseStrength * Options.FlingForceScale * Gears.ImpactMultiplier
                             * Mathf.Sqrt(Gears.FlingSpeedMultiplier);
            Impact.Apply(target, direction, strength);
            hand.Haptic(1f);
            state = State.Retracting;
        }

        private void Rocket(Vector3 anchor)
        {
            Vector3 toAnchor = anchor - Player.Head.position;
            float speed = Mathf.Clamp(toAnchor.magnitude * 2.2f, RocketMinSpeed, RocketMaxSpeed)
                          * Mathf.Sqrt(Gears.FlingSpeedMultiplier);
            Vector3 launch = toAnchor.normalized * speed + Vector3.up * 2f;

            Locomotion locomotion = Player.Player.locomotion;
            GameCompat.AddPlayerVelocity(locomotion, launch - locomotion.velocity);

            state = State.Anchored;
            anchoredUntil = Time.time + AnchorHoldTime;
        }

        private void Retract(float deltaTime)
        {
            Vector3 home = Hand.Position;
            tip = Vector3.MoveTowards(tip, home, travelSpeed * RetractSpeedFactor * deltaTime);
            if ((tip - home).sqrMagnitude < 0.12f * 0.12f) Finish();
        }

        private void Finish()
        {
            state = State.Idle;
            cooldownUntil = Time.time + Gears.FlingCooldown;
            Scaler.Hidden = false;
            arm.SetVisible(false);
        }

        // ------------------------------------------------------------------ visuals (LateUpdate)

        public override void LateUpdate(float deltaTime)
        {
            if (state == State.Idle) return;
            HandContext hand = Hand;
            float wobble = state == State.Retracting ? 1f : 0.15f;
            Quaternion rotation = Quaternion.LookRotation(direction, Player.Head.up);
            arm.Draw(hand.Elbow.position, tip, rotation, FistScale, wobble);
        }

        // ------------------------------------------------------------------ lifecycle

        private void OnGearChanged(Gear gear, bool active)
        {
            if (gear == Gear.Second) arm.SetColor(active ? VisualFactory.Gear2Skin : VisualFactory.Skin);
        }

        public override void Reset()
        {
            if (state != State.Idle) Finish();
            armed = true;
            cooldownUntil = 0f;
        }

        public override void Dispose()
        {
            Reset();
            Gears.Changed -= OnGearChanged;
            arm.Destroy();
        }
    }
}
