using LuffyAbilities.Visuals;
using ThunderRoad;

namespace LuffyAbilities.Core
{
    /// <summary>Shared services every ability gets (dependency bag, created once by the script).</summary>
    internal sealed class ModServices
    {
        public ModServices()
        {
            Gears = new GearState();
            Energy = new GearEnergy(Gears);
        }

        public PlayerContext Player { get; } = new PlayerContext();
        public GearState Gears { get; }
        public GearEnergy Energy { get; }
        public HandScaler LeftScaler { get; } = new HandScaler();
        public HandScaler RightScaler { get; } = new HandScaler();

        public HandScaler GetScaler(Side side) => side == Side.Left ? LeftScaler : RightScaler;
    }

    /// <summary>
    /// Base class for an ability. Update hooks are only called while the player context is valid.
    /// </summary>
    internal abstract class Ability
    {
        protected Ability(ModServices services) { Services = services; }

        protected ModServices Services { get; }
        protected PlayerContext Player => Services.Player;
        protected GearState Gears => Services.Gears;

        public virtual void Update(float deltaTime) { }
        public virtual void FixedUpdate(float deltaTime) { }
        public virtual void LateUpdate(float deltaTime) { }

        /// <summary>Drop all transient state (player died, level changed, mod disabled).</summary>
        public virtual void Reset() { }

        /// <summary>Destroy created objects (mod unloaded).</summary>
        public virtual void Dispose() => Reset();
    }
}
