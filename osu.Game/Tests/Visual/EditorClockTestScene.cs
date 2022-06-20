// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Screens.Edit;

namespace osu.Game.Tests.Visual
{
    /// <summary>
    /// Provides a clock, beat-divisor, and scrolling capability for test cases of editor components that
    /// are preferrably tested within the presence of a clock and seek controls.
    /// </summary>
    public abstract class EditorClockTestScene : OsuManualInputManagerTestScene
    {
        [Cached]
        private readonly OverlayColourProvider overlayColour = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        protected readonly BindableBeatDivisor BeatDivisor = new BindableBeatDivisor();

        [Cached]
        protected new readonly EditorClock Clock;

        private readonly Bindable<double> frequencyAdjustment = new BindableDouble(1);

        protected virtual bool ScrollUsingMouseWheel => true;

        protected EditorClockTestScene()
        {
            Clock = new EditorClock(new Beatmap(), BeatDivisor) { IsCoupled = false };
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            dependencies.Cache(BeatDivisor);
            dependencies.CacheAs(Clock);

            return dependencies;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Beatmap.BindValueChanged(beatmapChanged, true);

            AddSliderStep("editor clock rate", 0.0, 2.0, 1.0, v => frequencyAdjustment.Value = v);
        }

        private void beatmapChanged(ValueChangedEvent<WorkingBeatmap> e)
        {
            e.OldValue?.Track.RemoveAdjustment(AdjustableProperty.Frequency, frequencyAdjustment);

            Clock.Beatmap = e.NewValue.Beatmap;
            Clock.ChangeSource(e.NewValue.Track);
            Clock.ProcessFrame();

            e.NewValue.Track.AddAdjustment(AdjustableProperty.Frequency, frequencyAdjustment);
        }

        protected override void Update()
        {
            base.Update();

            Clock.ProcessFrame();
        }

        protected override bool OnScroll(ScrollEvent e)
        {
            if (!ScrollUsingMouseWheel)
                return false;

            if (e.ScrollDelta.Y > 0)
                Clock.SeekBackward(true);
            else
                Clock.SeekForward(true);

            return true;
        }
    }
}
