// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Input;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
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
        protected EditorClock Clock { get; private set; }

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateLocalDependencies(IReadOnlyDependencyContainer parent)
            => dependencies = new DependencyContainer(parent);

        private OsuGameBase osuGame;

        [BackgroundDependencyLoader]
        private void load(OsuGameBase osuGame)
        {
            this.osuGame = osuGame;

            dependencies.Cache(BeatDivisor);

            osuGame.Beatmap.ValueChanged += reinitializeClock;
            reinitializeClock(osuGame.Beatmap.Value);
        }

        protected override void Dispose(bool isDisposing)
        {
            osuGame.Beatmap.ValueChanged -= reinitializeClock;

            base.Dispose(isDisposing);
        }

        private void reinitializeClock(WorkingBeatmap working)
        {
            Clock = new EditorClock(working.Beatmap.ControlPointInfo, BeatDivisor) { IsCoupled = false };
            dependencies.CacheAs<IFrameBasedClock>(Clock);
            dependencies.CacheAs<IAdjustableClock>(Clock);
        }

        protected override bool OnWheel(InputState state)
        {
            if (state.Mouse.WheelDelta > 0)
                Clock.SeekBackward(true);
            else
                Clock.SeekForward(true);

            return true;
        }
    }
}
