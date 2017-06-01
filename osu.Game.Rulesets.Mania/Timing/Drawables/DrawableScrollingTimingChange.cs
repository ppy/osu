namespace osu.Game.Rulesets.Mania.Timing.Drawables
{
    public class DrawableScrollingTimingChange : DrawableTimingChange
    {
        public DrawableScrollingTimingChange(TimingChange timingChange)
            : base(timingChange)
        {
        }

        protected override void Update()
        {
            base.Update();

            Content.Y = (float)(TimingChange.Time - Time.Current);
        }
    }
}