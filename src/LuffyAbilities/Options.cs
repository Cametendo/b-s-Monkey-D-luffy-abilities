using ThunderRoad;

namespace LuffyAbilities
{
    /// <summary>
    /// In-game settings (Book / Menu -> Mods -> Luffy Abilities).
    /// The field initialisers MUST match the value at <c>defaultValueIndex</c> of the value source.
    /// </summary>
    public static class Options
    {
        private const string General = "General";
        private const string Fling = "Gum-Gum Fling";
        private const string Gear2 = "Gear Second";
        private const string Gear3 = "Gear Third";

        // ---------------------------------------------------------------- value sources

        public static ModOptionFloat[] Durations() => Range(10f, 300f, 10f, "s");            // 60 -> index 5
        public static ModOptionFloat[] PunchSpeeds() => Range(2f, 6f, 0.5f, " m/s");         // 3.5 -> index 3
        public static ModOptionFloat[] Reaches() => Range(3f, 20f, 1f, " m");                // 8 -> index 5
        public static ModOptionFloat[] Multipliers() => Range(1f, 5f, 0.25f, "x");           // 2.5 -> index 6, 2 -> 4
        public static ModOptionFloat[] ForceScales() => Range(0.25f, 4f, 0.25f, "x");        // 1 -> index 3
        public static ModOptionFloat[] HoldTimes() => Range(0.25f, 3f, 0.25f, "s");          // 1 -> index 3

        private static ModOptionFloat[] Range(float start, float end, float step, string suffix)
        {
            int count = UnityEngine.Mathf.RoundToInt((end - start) / step) + 1;
            var values = new ModOptionFloat[count];
            for (int i = 0; i < count; i++)
            {
                float v = start + step * i;
                values[i] = new ModOptionFloat(v.ToString("0.##") + suffix, v);
            }
            return values;
        }

        // ---------------------------------------------------------------- general

        [ModOptionCategory(General, 0)]
        [ModOption("Gear duration (full resource)",
            "How long ONE gear lasts when your Mana/Focus starts at 100%. Gear 2 + Gear 3 together drain twice as fast (half the time). Ignored with infinite Mana/Focus.",
            nameof(Durations), defaultValueIndex = 5, order = 0)]
        public static float GearDurationSeconds = 60f;

        [ModOptionCategory(General, 0)]
        [ModOption("Haptics", "Controller vibration feedback for gestures.", defaultValueIndex = 1, order = 1)]
        public static bool Haptics = true;

        // ---------------------------------------------------------------- fling

        [ModOptionCategory(Fling, 1)]
        [ModOption("Enable Gum-Gum Fling", "Make a fist (grip) with an empty hand and punch fast to stretch your arm.", defaultValueIndex = 1, order = 0)]
        public static bool FlingEnabled = true;

        [ModOptionCategory(Fling, 1)]
        [ModOption("Punch speed to fling", "How fast your punch must be to stretch the arm.", nameof(PunchSpeeds), defaultValueIndex = 3, order = 1)]
        public static float FlingPunchSpeed = 3.5f;

        [ModOptionCategory(Fling, 1)]
        [ModOption("Arm reach", "Maximum stretch distance.", nameof(Reaches), defaultValueIndex = 5, order = 2)]
        public static float FlingReach = 8f;

        [ModOptionCategory(Fling, 1)]
        [ModOption("Impact force", "Scales knock-back and damage of stretched punches.", nameof(ForceScales), defaultValueIndex = 3, order = 3)]
        public static float FlingForceScale = 1f;

        [ModOptionCategory(Fling, 1)]
        [ModOption("Gum-Gum Rocket", "Keep the fist closed when your stretched fist hits a wall to pull yourself there.", defaultValueIndex = 1, order = 4)]
        public static bool RocketEnabled = true;

        // ---------------------------------------------------------------- gear 2

        [ModOptionCategory(Gear2, 2)]
        [ModOption("Enable Gear Second", "Crouch and hold a fist near your knee to toggle Gear 2.", defaultValueIndex = 1, order = 0)]
        public static bool Gear2Enabled = true;

        [ModOptionCategory(Gear2, 2)]
        [ModOption("Gear 2 movement speed", "Movement/jump speed multiplier while in Gear 2.", nameof(Multipliers), defaultValueIndex = 6, order = 1)]
        public static float Gear2MoveMultiplier = 2.5f;

        [ModOptionCategory(Gear2, 2)]
        [ModOption("Gear 2 fling speed", "How much faster stretched punches travel in Gear 2.", nameof(Multipliers), defaultValueIndex = 6, order = 2)]
        public static float Gear2FlingMultiplier = 2.5f;

        [ModOptionCategory(Gear2, 2)]
        [ModOption("Pose hold time", "How long to hold the Gear 2 pose.", nameof(HoldTimes), defaultValueIndex = 3, order = 3)]
        public static float Gear2HoldTime = 1f;

        [ModOptionCategory(Gear2, 2)]
        [ModOption("Steam effect", "Show steam puffs on your arms in Gear 2.", defaultValueIndex = 1, order = 4)]
        public static bool Gear2Steam = true;

        // ---------------------------------------------------------------- gear 3

        [ModOptionCategory(Gear3, 3)]
        [ModOption("Enable Gear Third", "Hold a hand at your mouth, then press that hand's trigger on the ring to toggle Gear 3.", defaultValueIndex = 1, order = 0)]
        public static bool Gear3Enabled = true;

        [ModOptionCategory(Gear3, 3)]
        [ModOption("Gear 3 fist size", "How big your fists inflate.", nameof(Multipliers), defaultValueIndex = 6, order = 1)]
        public static float Gear3FistScale = 2.5f;

        [ModOptionCategory(Gear3, 3)]
        [ModOption("Gear 3 impact", "Force/damage multiplier for punches and flings in Gear 3.", nameof(Multipliers), defaultValueIndex = 8, order = 2)]
        public static float Gear3ImpactMultiplier = 3f;
    }
}
