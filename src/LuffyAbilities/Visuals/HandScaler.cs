using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;

namespace LuffyAbilities.Visuals
{
    /// <summary>
    /// The one owner of a hand's visual scale. Combines:
    ///  - Gear 3 inflation (animated, balloon-like),
    ///  - hiding the real hand while its stretched fist is flying.
    ///
    /// Only skinned mesh bones are touched - physics colliders and joints stay the same.
    /// The palm bone is scaled; finger mesh bones are parented to the finger colliders
    /// (not to the palm) in U12 and 1.0, so each finger bone is scaled and pushed outward
    /// from the wrist by the same factor to keep the hand in one piece.
    /// </summary>
    internal sealed class HandScaler
    {
        private const float InflateSpeed = 4f;
        private const float HiddenScale = 0.001f;

        private readonly List<Transform> fingerBones = new List<Transform>();
        private RagdollHand hand;
        private Vector3 palmOriginalScale = Vector3.one;
        private float currentInflation = 1f;
        private bool modified;

        public float TargetInflation { get; set; } = 1f;
        public bool Hidden { get; set; }

        /// <summary>Current animated inflation (other visuals use it to stay in sync).</summary>
        public float Inflation => currentInflation;

        public void Bind(RagdollHand newHand)
        {
            if (newHand == hand) return;
            Restore();
            hand = newHand;
            fingerBones.Clear();
            if (hand == null) return;

            if (hand.meshBone != null) palmOriginalScale = hand.meshBone.localScale;
            foreach (RagdollHand.Finger finger in hand.fingers)
            {
                AddBone(finger.proximal);
                AddBone(finger.intermediate);
                AddBone(finger.distal);
            }
        }

        private void AddBone(RagdollHand.Finger.Bone bone)
        {
            if (bone != null && bone.mesh != null) fingerBones.Add(bone.mesh);
        }

        /// <summary>Call in LateUpdate so it runs after the game posed the hand.</summary>
        public void Update(float deltaTime)
        {
            if (hand == null || hand.meshBone == null) return;

            currentInflation = Mathf.MoveTowards(currentInflation, TargetInflation, InflateSpeed * deltaTime);
            bool inflating = !Mathf.Approximately(currentInflation, TargetInflation) && TargetInflation > 1f;
            float wobble = inflating ? Mathf.Sin(Time.time * 30f) * 0.04f * currentInflation : 0f;
            float scale = Hidden ? HiddenScale : currentInflation + wobble;

            if (Mathf.Approximately(scale, 1f))
            {
                if (modified) ApplyRestScale();
                return;
            }

            ApplyScale(scale);
        }

        private void ApplyScale(float scale)
        {
            modified = true;
            Vector3 wrist = hand.meshBone.position;
            hand.meshBone.localScale = palmOriginalScale * scale;

            foreach (Transform bone in fingerBones)
            {
                if (bone == null || bone.parent == null) continue;
                Vector3 rest = bone.parent.position;
                bone.position = wrist + (rest - wrist) * scale;
                bone.localScale = Vector3.one * scale;
            }
        }

        private void ApplyRestScale()
        {
            modified = false;
            if (hand == null) return;
            if (hand.meshBone != null) hand.meshBone.localScale = palmOriginalScale;
            foreach (Transform bone in fingerBones)
            {
                if (bone == null) continue;
                bone.localPosition = Vector3.zero; // finger meshes sit at their parent's origin
                bone.localScale = Vector3.one;
            }
        }

        public void Restore()
        {
            ApplyRestScale();
            TargetInflation = 1f;
            currentInflation = 1f;
            Hidden = false;
        }
    }
}
