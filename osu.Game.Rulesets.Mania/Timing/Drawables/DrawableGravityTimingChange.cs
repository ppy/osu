using System;
using osu.Framework.Graphics;
using osu.Framework.MathUtils;
using osu.Framework.Physics;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.Timing.Drawables
{
    public class DrawableGravityTimingChange : DrawableTimingChange
    {
        private const double acceleration = 9.8f;
        private const double terminal_velocity = 50f;

        private const double duration = 2000;


        public DrawableGravityTimingChange(TimingChange timingChange)
            : base(timingChange)
        {
        }

        protected override void Update()
        {
            base.Update();

            var parent = (TimingChangeContainer)Parent;

            double elapsed = Math.Max(0, Time.Current - TimingChange.Time);

            // @ Current == TimingChange.Time - duration -> Y = TimingChange.Time
            // @ Current == TimingChange.Time -> Y = 0
            // @ Current == TimingChange.Time + x -> Y = -f(x)

            double acceleration = elapsed / 2 / 1000 / 1000;

            Content.Y = (float)(TimingChange.Time - 1 / 2f * acceleration * elapsed * elapsed);
        }
    }
}