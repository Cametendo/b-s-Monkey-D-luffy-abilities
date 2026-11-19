using UnityEngine;

namespace LuffyAbilities.Visuals
{
    /// <summary>
    /// The stretched arm: a skin-coloured tube from the elbow to a flying fist.
    /// It wobbles like rubber while snapping back.
    /// </summary>
    internal sealed class RubberArm
    {
        private const int Segments = 16;
        private const float ArmWidth = 0.075f;
        private static readonly Vector3 FistShape = new Vector3(0.1f, 0.09f, 0.12f);

        private readonly LineRenderer line;
        private readonly GameObject fist;
        private Color color = VisualFactory.Skin;

        public RubberArm(string name)
        {
            line = VisualFactory.CreateLine(name + "_Arm", null, Segments, ArmWidth);
            fist = VisualFactory.CreateSphere(name + "_Fist", null);
            Object.DontDestroyOnLoad(line.gameObject);
            Object.DontDestroyOnLoad(fist);
            SetVisible(false);
        }

        public void SetVisible(bool visible)
        {
            if (visible) ApplyMaterial();
            if (line != null) line.enabled = visible;
            if (fist != null) fist.SetActive(visible);
        }

        public void SetColor(Color newColor)
        {
            color = newColor;
            if (line != null && line.enabled) ApplyMaterial();
        }

        /// <summary>Materials are resolved lazily: the fallback shader only exists once the player spawned.</summary>
        private void ApplyMaterial()
        {
            Material material = VisualFactory.LitMaterial(color);
            if (line != null) line.sharedMaterial = material;
            if (fist != null) fist.GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        /// <param name="wobble">0 = straight, 1 = full rubber wobble.</param>
        public void Draw(Vector3 from, Vector3 to, Quaternion fistRotation, float fistScale, float wobble)
        {
            if (line == null || fist == null) return;

            Vector3 axis = to - from;
            Vector3 side = Vector3.Cross(axis.normalized, Vector3.up);
            if (side.sqrMagnitude < 1e-4f) side = Vector3.right;
            side.Normalize();

            float amplitude = wobble * Mathf.Min(0.25f, axis.magnitude * 0.06f);
            for (int i = 0; i < Segments; i++)
            {
                float t = i / (float)(Segments - 1);
                float wave = Mathf.Sin(t * Mathf.PI) * Mathf.Sin(t * 9f - Time.time * 40f) * amplitude;
                line.SetPosition(i, from + axis * t + side * wave);
            }

            line.widthMultiplier = ArmWidth * Mathf.Lerp(1f, fistScale, 0.35f);
            fist.transform.SetPositionAndRotation(to, fistRotation);
            fist.transform.localScale = FistShape * fistScale;
        }

        public void Destroy()
        {
            if (line != null) Object.Destroy(line.gameObject);
            if (fist != null) Object.Destroy(fist);
        }
    }
}
