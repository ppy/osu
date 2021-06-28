using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;

namespace Mvis.Plugin.RulesetPanel.Components.MusicHelpers
{
    public class RateAdjustableContainer : Container
    {
        public double Rate
        {
            get => clock.Rate;
            set => clock.Rate = value;
        }

        private readonly StopwatchClock clock;

        public RateAdjustableContainer()
        {
            ProcessCustomClock = true;
            Clock = new FramedClock(clock = new StopwatchClock());
            clock.Start();
        }
    }
}
