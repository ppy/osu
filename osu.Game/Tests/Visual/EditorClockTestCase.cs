// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Input;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Screens.Compose;

namespace osu.Game.Tests.Visual
{
    /// <summary>
    /// Provides a clock, beat-divisor, and scrolling capability for test cases of editor components that
    /// are preferrably tested within the presence of a clock and seek controls.
    /// </summary>
    public abstract class EditorClockTestCase : OsuTestCase
    {
        protected readonly BindableBeatDivisor BeatDivisor = new BindableBeatDivisor();
        protected readonly EditorClock Clock;

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateLocalDependencies(IReadOnlyDependencyContainer parent)
            => dependencies = new DependencyContainer(parent);

        private OsuGameBase osuGame;

        protected EditorClockTestCase()
        {
            Clock = new EditorClock(new ControlPointInfo(), BeatDivisor) { IsCoupled = false };
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase osuGame)
        {
            this.osuGame = osuGame;

            dependencies.Cache(BeatDivisor);
            dependencies.CacheAs<IFrameBasedClock>(Clock);
            dependencies.CacheAs<IAdjustableClock>(Clock);

            osuGame.Beatmap.ValueChanged += beatmapChanged;
            beatmapChanged(osuGame.Beatmap.Value);
        }

        private void beatmapChanged(WorkingBeatmap working)
        {
            Clock.ControlPointInfo = working.Beatmap.ControlPointInfo;
            Clock.ChangeSource((IAdjustableClock)working.Track ?? new StopwatchClock());
            Clock.ProcessFrame();
        }

        protected override void Update()
        {
            base.Update();

            Clock.ProcessFrame();
        }

        protected override bool OnWheel(InputState state)
        {
            if (state.Mouse.WheelDelta > 0)
                Clock.SeekBackward(true);
            else
                Clock.SeekForward(true);

            return true;
        }

        protected override void Dispose(bool isDisposing)
        {
            osuGame.Beatmap.ValueChanged -= beatmapChanged;

            base.Dispose(isDisposing);
        }
    }
}
