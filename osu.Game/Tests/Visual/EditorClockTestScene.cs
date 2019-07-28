// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Input.Events;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Screens.Edit;

namespace osu.Game.Tests.Visual
{
    /// <summary>
    /// Provides a clock, beat-divisor, and scrolling capability for test cases of editor components that
    /// are preferrably tested within the presence of a clock and seek controls.
    /// </summary>
    public abstract class EditorClockTestScene : OsuTestScene
    {
        protected readonly BindableBeatDivisor BeatDivisor = new BindableBeatDivisor();
        protected new readonly EditorClock Clock;

        protected EditorClockTestScene()
        {
            Clock = new EditorClock(new ControlPointInfo(), 5000, BeatDivisor) { IsCoupled = false };
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

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

        private void beatmapChanged(ValueChangedEvent<WorkingBeatmap> e)
        {
            Clock.ControlPointInfo = e.NewValue.Beatmap.ControlPointInfo;
            Clock.ChangeSource((IAdjustableClock)e.NewValue.Track ?? new StopwatchClock());
            Clock.ProcessFrame();
        }

        protected override void Update()
        {
            base.Update();

            Clock.ProcessFrame();
        }

        protected override bool OnScroll(ScrollEvent e)
        {
            if (e.ScrollDelta.Y > 0)
                Clock.SeekBackward(true);
            else
                Clock.SeekForward(true);

            return true;
        }
    }
}
