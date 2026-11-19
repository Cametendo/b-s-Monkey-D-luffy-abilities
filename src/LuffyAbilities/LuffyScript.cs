using System;
using System.Collections.Generic;
using LuffyAbilities.Abilities;
using LuffyAbilities.Compat;
using LuffyAbilities.Core;
using LuffyAbilities.Visuals;
using ThunderRoad;
using UnityEngine;

namespace LuffyAbilities
{
    /// <summary>
    /// Mod entry point. The game creates ThunderScripts automatically for every DLL in a mod
    /// folder (U12 and 1.0), so no JSON / catalog data is needed.
    /// </summary>
    public class LuffyScript : ThunderScript
    {
        private ModServices services;
        private readonly List<Ability> abilities = new List<Ability>();
        private bool wasValid;

        public override void ScriptLoaded(ModManager.ModData modData)
        {
            base.ScriptLoaded(modData);
            Debug.Log($"[LuffyAbilities] Loaded for Blade & Sorcery {GameCompat.GameLabel} (resource: {GameCompat.ResourceName}).");
        }

        public override void ScriptEnable()
        {
            base.ScriptEnable();
            services = new ModServices();
            services.Player.CreatureChanged += OnCreatureChanged;

            foreach (Side side in new[] { Side.Left, Side.Right })
            {
                var fling = new GumGumFling(services, side);
                abilities.Add(fling);
                abilities.Add(new GiantFistPunch(services, side, fling));
            }
            abilities.Add(new GearSecond(services));
            abilities.Add(new GearThird(services));
        }

        public override void ScriptDisable()
        {
            if (services != null)
            {
                ResetAll();
                services.Player.CreatureChanged -= OnCreatureChanged;
                RunAll(DisposeStep, 0f);
            }
            abilities.Clear();
            services = null;
            base.ScriptDisable();
        }

        public override void ScriptUpdate()
        {
            base.ScriptUpdate();
            if (services == null) return;

            float dt = Time.deltaTime;
            services.Player.Refresh(dt);

            if (!services.Player.IsValid)
            {
                if (wasValid) ResetAll(); // died / menu: drop gears and visuals
                wasValid = false;
                return;
            }

            if (!wasValid) OnBecameValid();
            wasValid = true;

            RunAll(UpdateStep, dt);
            services.Energy.Tick(services.Player.Creature, dt);
        }

        public override void ScriptFixedUpdate()
        {
            base.ScriptFixedUpdate();
            if (services == null || !wasValid) return;
            float dt = Time.fixedDeltaTime;
            RunAll(FixedUpdateStep, dt);
        }

        public override void ScriptLateUpdate()
        {
            base.ScriptLateUpdate();
            if (services == null || !wasValid) return;
            float dt = Time.deltaTime;
            RunAll(LateUpdateStep, dt);
            services.LeftScaler.Update(dt);
            services.RightScaler.Update(dt);
        }

        // ------------------------------------------------------------------ lifecycle helpers

        private void OnBecameValid()
        {
            Creature creature = services.Player.Creature;
            services.LeftScaler.Bind(creature.handLeft);
            services.RightScaler.Bind(creature.handRight);

            // A shader that certainly renders in this game, in case Shader.Find comes up empty.
            if (VisualFactory.FallbackShader == null)
            {
                Renderer renderer = creature.GetComponentInChildren<SkinnedMeshRenderer>();
                if (renderer != null && renderer.sharedMaterial != null)
                    VisualFactory.FallbackShader = renderer.sharedMaterial.shader;
            }
        }

        private void OnCreatureChanged() => ResetAll();

        /// <summary>"No limits afterwards": switching off simply restores the normal state.</summary>
        private void ResetAll()
        {
            services.Gears.DeactivateAll();
            services.Energy.Reset();
            RunAll(ResetStep, 0f);
            services.LeftScaler.Restore();
            services.RightScaler.Restore();
            services.LeftScaler.Bind(null);
            services.RightScaler.Bind(null);
            wasValid = false;
        }

        private static readonly Action<Ability, float> UpdateStep = (a, dt) => a.Update(dt);
        private static readonly Action<Ability, float> FixedUpdateStep = (a, dt) => a.FixedUpdate(dt);
        private static readonly Action<Ability, float> LateUpdateStep = (a, dt) => a.LateUpdate(dt);
        private static readonly Action<Ability, float> ResetStep = (a, _) => a.Reset();
        private static readonly Action<Ability, float> DisposeStep = (a, _) => a.Dispose();

        /// <summary>Runs a step on every ability; one broken ability never takes the others (or the game loop) down.</summary>
        private void RunAll(Action<Ability, float> step, float deltaTime)
        {
            foreach (Ability ability in abilities)
            {
                try
                {
                    step(ability, deltaTime);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[LuffyAbilities] {ability.GetType().Name}: {e}");
                }
            }
        }
    }
}
