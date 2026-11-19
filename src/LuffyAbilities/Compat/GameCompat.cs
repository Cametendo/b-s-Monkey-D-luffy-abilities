using ThunderRoad;
using UnityEngine;
#if BAS_V1
using ThunderRoad.Skill.SpellPower;
#endif

namespace LuffyAbilities.Compat
{
    /// <summary>
    /// The only place that knows about API differences between game versions.
    /// Everything else in the mod talks to these helpers.
    ///
    ///  U12 : the casting resource is Mana  (Mana.currentMana / maxMana / infiniteMana)
    ///  1.0+: the casting resource is Focus (Mana.currentFocus / MaxFocus / infiniteFocus)
    /// </summary>
    internal static class GameCompat
    {
#if BAS_U12
        public const string GameLabel = "U12";
        public const string ResourceName = "Mana";

        public static bool ResourceIsInfinite => Mana.infiniteMana;
        public static float GetResource(Mana mana) => mana.currentMana;
        public static float GetMaxResource(Mana mana) => mana.maxMana;
        public static void SetResource(Mana mana, float value) => mana.currentMana = Mathf.Clamp(value, 0f, mana.maxMana);

        public static void AddPlayerVelocity(Locomotion locomotion, Vector3 deltaVelocity)
        {
            if (locomotion != null && locomotion.rb != null)
                locomotion.rb.AddForce(deltaVelocity, ForceMode.VelocityChange);
        }

#else
        public const string GameLabel = "1.0";
        public const string ResourceName = "Focus";

        public static bool ResourceIsInfinite => Mana.infiniteFocus;
        public static float GetResource(Mana mana) => mana.currentFocus;
        public static float GetMaxResource(Mana mana) => mana.MaxFocus;
        public static void SetResource(Mana mana, float value) => mana.currentFocus = Mathf.Clamp(value, 0f, mana.MaxFocus);

        public static void AddPlayerVelocity(Locomotion locomotion, Vector3 deltaVelocity)
        {
            if (locomotion != null && locomotion.physicBody != null)
                locomotion.physicBody.AddForce(deltaVelocity, ForceMode.VelocityChange);
        }

#endif

        public static void HapticShort(PlayerControl.Hand hand, float intensity) => hand?.HapticShort(intensity);

        /// <summary>True when the hand is busy with telekinesis (both versions keep it on the caster).</summary>
        public static bool IsUsingTelekinesis(RagdollHand hand)
        {
            var tk = hand?.caster?.telekinesis;
            return tk != null && tk.catchedHandle != null;
        }
    }
}
