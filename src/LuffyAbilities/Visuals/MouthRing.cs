using UnityEngine;

namespace LuffyAbilities.Visuals
{
    /// <summary>The pulsing interaction circle shown in front of the mouth for Gear 3.</summary>
    internal sealed class MouthRing
    {
        private const int Points = 48;
        private const float Radius = 0.055f;

        private readonly LineRenderer outer;
        private readonly LineRenderer inner;
        private Color currentColor;
        private bool hasMaterial;

        public MouthRing()
        {
            outer = VisualFactory.CreateLine("LuffyAbilities_Gear3Ring", null, Points, 0.008f, loop: true);
            inner = VisualFactory.CreateLine("LuffyAbilities_Gear3RingInner", null, Points, 0.004f, loop: true);
            Object.DontDestroyOnLoad(outer.gameObject);
            Object.DontDestroyOnLoad(inner.gameObject);
            Hide();
        }

        public void Show(Vector3 center, Quaternion facing, Color color)
        {
            if (outer == null || inner == null) return;
            if (!hasMaterial || color != currentColor)
            {
                Material material = VisualFactory.UnlitMaterial(color);
                outer.sharedMaterial = material;
                inner.sharedMaterial = material;
                currentColor = color;
                hasMaterial = material != null;
            }

            float pulse = 1f + Mathf.Sin(Time.time * 8f) * 0.08f;
            DrawCircle(outer, center, facing, Radius * pulse);
            DrawCircle(inner, center, facing, Radius * 0.55f / pulse);
            outer.enabled = inner.enabled = true;
        }

        public void Hide()
        {
            if (outer != null) outer.enabled = false;
            if (inner != null) inner.enabled = false;
        }

        private static void DrawCircle(LineRenderer line, Vector3 center, Quaternion facing, float radius)
        {
            for (int i = 0; i < Points; i++)
            {
                float angle = i / (float)Points * Mathf.PI * 2f;
                Vector3 local = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
                line.SetPosition(i, center + facing * local);
            }
        }

        public void Destroy()
        {
            if (outer != null) Object.Destroy(outer.gameObject);
            if (inner != null) Object.Destroy(inner.gameObject);
        }
    }
}
