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
        private DecoupleableInterpolatingFramedClock adjustableClock;
        private WorkingBeatmap beatmap;
        private BindableBool IsPaused = new BindableBool();
        private Bindable<double> userAudioOffset;
        private double baseOffset => (RuntimeInfo.OS == RuntimeInfo.Platform.Windows ? 15 : 0);

        public ClockContainer(WorkingBeatmap b)
        {
            this.beatmap = b;

            baseOffsetClock = new HardwareCorrectionOffsetClock(adjustableClock) { Offset = 0 };

            adjustableClock = new DecoupleableInterpolatingFramedClock { IsCoupled = false };

            gameplayClock = new GameplayClock(adjustableClock);
        }


        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            userAudioOffset = config.GetBindable<double>(OsuSetting.AudioOffset);
            userAudioOffset.BindValueChanged(offset => baseOffsetClock.Offset = (baseOffset + offset.NewValue), true);
        }

        public void Seek(double time)
        {
            adjustableClock.Seek(time);

            adjustableClock.ProcessFrame();
        }

        public void Start()
        {
            IsPaused.Value = false;

            adjustableClock.Start();
        }

        public void Stop()
        {
            IsPaused.Value = true;

            adjustableClock.Stop();
        }

        protected override void Update()
        {
            if ( !IsPaused.Value )
                adjustableClock.ProcessFrame();
        }
    }
}