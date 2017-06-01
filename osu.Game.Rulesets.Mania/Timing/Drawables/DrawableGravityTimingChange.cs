using osu.Framework.Physics;

namespace osu.Game.Rulesets.Mania.Timing.Drawables
{
    public class DrawableGravityTimingChange : DrawableTimingChange
    {
        private const float acceleration = 9.8f;
        private const float terminal_velocity = 50f;

        private RigidBodySimulation sim;

        public DrawableGravityTimingChange(TimingChange timingChange)
            : base(timingChange)
        {
            sim = new RigidBodySimulation(Content);
        }

        protected override void Update()
        {
            base.Update();

            // Todo: Gravity calculations here
        }
    }
}