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

        protected EditorClockTestCase()
        {
            Clock = new EditorClock(new ControlPointInfo(), 5000, BeatDivisor) { IsCoupled = false };
        }

        protected override IReadOnlyDependencyContainer CreateLocalDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateLocalDependencies(parent));

            dependencies.Cache(BeatDivisor);
            dependencies.CacheAs<IFrameBasedClock>(Clock);
            dependencies.CacheAs<IAdjustableClock>(Clock);

            return dependencies;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Beatmap.BindValueChanged(beatmapChanged, true);
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

        protected override bool OnScroll(InputState state)
        {
            if (state.Mouse.ScrollDelta.Y > 0)
                Clock.SeekBackward(true);
            else
                Clock.SeekForward(true);

            return true;
        }
    }
}
