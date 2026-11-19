using System;
using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;

namespace LuffyAbilities.Abilities
{
    internal enum HitKind
    {
        None,
        Creature,
        Item,
        Rigidbody,
        World
    }

    internal struct HitTarget
    {
        public HitKind Kind;
        public Collider Collider;
        public Creature Creature;
        public RagdollPart Part;
        public Item Item;
        public Rigidbody Body;
        public Vector3 Point;
        public Vector3 Normal;
    }

    /// <summary>
    /// Shared "what did my fist hit and what happens to it" logic, used by both the
    /// stretched fling punch and the Gear 3 close-range punch boost.
    /// </summary>
    internal static class Impact
    {
        private const int MaxHits = 32;
        private const float BaseDamage = 6f;
        private const float DestabilizeStrength = 5f;

        private static readonly RaycastHit[] CastBuffer = new RaycastHit[MaxHits];
        private static readonly Collider[] OverlapBuffer = new Collider[MaxHits];
        private static readonly Comparison<RaycastHit> ByDistance = (a, b) => a.distance.CompareTo(b.distance);

        /// <summary>Sphere-cast that skips the player, their held items and triggers. Returns the closest valid hit.</summary>
        public static bool SphereCast(Vector3 origin, float radius, Vector3 direction, float distance, Creature self, out HitTarget target)
        {
            target = default;
            int count = Physics.SphereCastNonAlloc(origin, radius, direction, CastBuffer, distance,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            if (count == 0) return false;

            Array.Sort(CastBuffer, 0, count, Comparer<RaycastHit>.Create(ByDistance));
            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = CastBuffer[i];
                if (!Classify(hit.collider, self, out target)) continue;

                // distance 0 means we started inside the collider - use the closest point instead.
                target.Point = hit.distance > 0f ? hit.point : hit.collider.ClosestPoint(origin);
                target.Normal = hit.distance > 0f ? hit.normal : -direction;
                return true;
            }
            return false;
        }

        /// <summary>All distinct creatures/items around a point (for close-range punches).</summary>
        public static int Overlap(Vector3 center, float radius, Creature self, List<HitTarget> results)
        {
            results.Clear();
            int count = Physics.OverlapSphereNonAlloc(center, radius, OverlapBuffer,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                if (!Classify(OverlapBuffer[i], self, out HitTarget target)) continue;
                if (target.Kind == HitKind.World) continue;
                if (Contains(results, target)) continue;

                target.Point = OverlapBuffer[i].ClosestPoint(center);
                target.Normal = (center - target.Point).normalized;
                results.Add(target);
            }
            return results.Count;
        }

        private static bool Contains(List<HitTarget> list, HitTarget target)
        {
            foreach (HitTarget existing in list)
            {
                if (target.Creature != null && existing.Creature == target.Creature) return true;
                if (target.Item != null && existing.Item == target.Item) return true;
                if (target.Body != null && existing.Body == target.Body) return true;
            }
            return false;
        }

        public static bool Classify(Collider collider, Creature self, out HitTarget target)
        {
            target = new HitTarget { Collider = collider };
            if (collider == null) return false;

            // The player's own locomotion capsule, body and anything attached to the rig.
            if (collider.GetComponentInParent<Player>() != null) return false;

            RagdollPart part = collider.GetComponentInParent<RagdollPart>();
            if (part != null && part.ragdoll != null && part.ragdoll.creature != null)
            {
                if (part.ragdoll.creature == self) return false;
                target.Kind = HitKind.Creature;
                target.Creature = part.ragdoll.creature;
                target.Part = part;
                target.Body = collider.attachedRigidbody;
                return true;
            }

            Item item = collider.GetComponentInParent<Item>();
            if (item != null)
            {
                if (item.holder != null) return false; // holstered / in a rack
                if (item.mainHandler != null && item.mainHandler.creature == self) return false;
                target.Kind = HitKind.Item;
                target.Item = item;
                target.Body = collider.attachedRigidbody;
                return true;
            }

            Rigidbody body = collider.attachedRigidbody;
            if (body != null && !body.isKinematic)
            {
                target.Kind = HitKind.Rigidbody;
                target.Body = body;
                return true;
            }

            target.Kind = HitKind.World;
            return true;
        }

        /// <summary>
        /// Knock back and damage a target.
        /// <paramref name="strength"/> is roughly the velocity change in m/s given to the target.
        /// </summary>
        public static void Apply(HitTarget target, Vector3 direction, float strength)
        {
            direction.Normalize();
            switch (target.Kind)
            {
                case HitKind.Creature:
                    HitCreature(target, direction, strength);
                    break;
                case HitKind.Item:
                case HitKind.Rigidbody:
                    PushBody(target.Body, target.Point, direction, strength);
                    break;
            }
        }

        private static void HitCreature(HitTarget target, Vector3 direction, float strength)
        {
            Creature creature = target.Creature;
            if (creature == null || creature.isPlayer) return;

            if (!creature.isKilled)
            {
                var damage = new DamageStruct(DamageType.Blunt, BaseDamage * strength / DestabilizeStrength);
                damage.hitRagdollPart = target.Part;
                var collision = new CollisionInstance(damage)
                {
                    contactPoint = target.Point,
                    contactNormal = target.Normal,
                    impactVelocity = direction * strength
                };
                try
                {
                    creature.Damage(collision);
                }
                catch (Exception e)
                {
                    // Another mod's hit listener failing must not break our punch.
                    Debug.LogWarning("[LuffyAbilities] Damage event threw: " + e.Message);
                }

                if (!creature.isKilled)
                {
                    creature.TryPush(Creature.PushType.Hit, direction, strength >= DestabilizeStrength ? 3 : 1);
                    if (strength >= DestabilizeStrength && creature.ragdoll != null)
                        creature.ragdoll.SetState(Ragdoll.State.Destabilized);
                }
            }

            // Throw the whole body, the struck part a bit harder.
            if (creature.ragdoll == null) return;
            Vector3 launch = direction * strength + Vector3.up * strength * 0.15f;
            foreach (RagdollPart part in creature.ragdoll.parts)
            {
                if (part == null || part.physicBody == null || part.physicBody.isKinematic) continue;
                float scale = part == target.Part ? 1f : 0.6f;
                part.physicBody.AddForce(launch * scale, ForceMode.VelocityChange);
            }
        }

        private static void PushBody(Rigidbody body, Vector3 point, Vector3 direction, float strength)
        {
            if (body == null || body.isKinematic) return;
            // Light objects get ~strength m/s, heavy ones (>20 kg) proportionally less.
            float impulse = strength * Mathf.Min(body.mass, 20f);
            body.AddForceAtPosition(direction * impulse, point, ForceMode.Impulse);
        }
    }
}
