using System.Collections.Generic;
using UnityEngine;

namespace LuffyAbilities.Visuals
{
    /// <summary>
    /// Builds every visual of the mod from code (no asset bundle needed, so nothing
    /// can break between game versions). Materials are cached per colour.
    /// </summary>
    internal static class VisualFactory
    {
        public static readonly Color Skin = new Color(0.96f, 0.76f, 0.62f);
        public static readonly Color Gear2Skin = new Color(1f, 0.55f, 0.55f);
        public static readonly Color Steam = new Color(0.95f, 0.95f, 0.95f, 0.55f);
        public static readonly Color RingOn = new Color(1f, 0.55f, 0.1f);
        public static readonly Color RingOff = new Color(0.35f, 0.75f, 1f);

        // URP first (B&S renders with URP), then generic fallbacks.
        private static readonly string[] LitShaders =
        {
            "Universal Render Pipeline/Lit", "Universal Render Pipeline/Simple Lit",
            "Universal Render Pipeline/Unlit", "Unlit/Color", "Sprites/Default"
        };

        private static readonly string[] UnlitShaders =
        {
            "Universal Render Pipeline/Unlit", "Universal Render Pipeline/Particles/Unlit",
            "Sprites/Default", "Unlit/Color", "Universal Render Pipeline/Lit"
        };

        private static readonly Dictionary<string, Material> Cache = new Dictionary<string, Material>();
        private static Mesh sphereMesh;

        /// <summary>Fallback shader taken from a renderer that is guaranteed to draw correctly.</summary>
        public static Shader FallbackShader { get; set; }

        public static Material LitMaterial(Color color) => GetMaterial("lit", LitShaders, color);
        public static Material UnlitMaterial(Color color) => GetMaterial("unlit", UnlitShaders, color);

        /// <summary>
        /// Returns a cached material, or null if no usable shader exists yet (never throws).
        /// Failures are not cached so a later call can succeed once <see cref="FallbackShader"/> is known.
        /// </summary>
        private static Material GetMaterial(string kind, string[] shaderNames, Color color)
        {
            string key = kind + ColorUtility.ToHtmlStringRGBA(color);
            if (Cache.TryGetValue(key, out Material cached) && cached != null) return cached;

            Shader shader = null;
            foreach (string name in shaderNames)
            {
                shader = Shader.Find(name);
                if (shader != null) break;
            }
            if (shader == null) shader = FallbackShader;
            if (shader == null) return null;

            var material = new Material(shader) { name = "LuffyAbilities_" + key };
            SetColor(material, color);
            Cache[key] = material;
            return material;
        }

        public static void SetColor(Material material, Color color)
        {
            if (material == null) return;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        }

        public static Mesh SphereMesh
        {
            get
            {
                if (sphereMesh != null) return sphereMesh;
                GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphereMesh = temp.GetComponent<MeshFilter>().sharedMesh;
                Object.Destroy(temp);
                return sphereMesh;
            }
        }

        /// <summary>A collider-less sphere renderer.</summary>
        public static GameObject CreateSphere(string name, Material material)
        {
            var go = new GameObject(name);
            go.AddComponent<MeshFilter>().sharedMesh = SphereMesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return go;
        }

        public static LineRenderer CreateLine(string name, Material material, int points, float width, bool loop = false)
        {
            var go = new GameObject(name);
            var line = go.AddComponent<LineRenderer>();
            line.sharedMaterial = material;
            line.positionCount = points;
            line.widthMultiplier = width;
            line.loop = loop;
            line.useWorldSpace = true;
            line.numCapVertices = 6;
            line.numCornerVertices = 4;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            return line;
        }

        /// <summary>Puffy steam made of small spheres that grow and vanish (works without textures).</summary>
        public static ParticleSystem CreateSteam(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.1f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.15f, 0.45f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.06f);
            main.startColor = Steam;
            main.gravityModifier = -0.05f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 60;

            var emission = ps.emission;
            emission.rateOverTime = 22f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.04f;

            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 0.4f), new Keyframe(0.35f, 1f), new Keyframe(1f, 0f)));

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Mesh;
            renderer.mesh = SphereMesh;
            renderer.sharedMaterial = UnlitMaterial(Steam);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            return ps;
        }
    }
}
