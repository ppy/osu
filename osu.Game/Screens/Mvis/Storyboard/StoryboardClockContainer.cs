using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Screens.Play;
using static osu.Game.Screens.Play.GameplayClockContainer;
using osu.Framework;
using osu.Game.Configuration;

namespace osu.Game.Screens.Mvis.Storyboard
{
    /// <summary>
    /// 简化版的GameplayClockContainer，不包含对mods和gameplayStartTime的依赖
    /// </summary>
    public class ClockContainer : Container
    {
        [Cached]
        public readonly GameplayClock gameplayClock;

        private readonly FramedOffsetClock baseOffsetClock;
        private CustomedDecoupleableInterpolatingFramedClock adjustableClock;
        private WorkingBeatmap beatmap;
        private BindableBool IsPaused = new BindableBool();

        public ClockContainer(WorkingBeatmap b)
        {
            this.beatmap = b;

            adjustableClock = new CustomedDecoupleableInterpolatingFramedClock();
            adjustableClock.ChangeSource(b.Track);

            baseOffsetClock = new HardwareCorrectionOffsetClock(adjustableClock) { Offset = 0 };

            gameplayClock = new GameplayClock(baseOffsetClock);
        }

        public void Seek(double time)
        {
            adjustableClock.Seek(time);

            baseOffsetClock.ProcessFrame();
        }

        protected override void Update()
        {
            baseOffsetClock.ProcessFrame();
        }
    }
}