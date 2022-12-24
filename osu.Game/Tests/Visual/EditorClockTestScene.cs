// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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
    public abstract partial class EditorClockTestScene : OsuManualInputManagerTestScene
    {
        [Cached]
        private readonly OverlayColourProvider overlayColour = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        protected readonly BindableBeatDivisor BeatDivisor = new BindableBeatDivisor();

        protected EditorClock EditorClock;

        private readonly Bindable<double> frequencyAdjustment = new BindableDouble(1);

        private IBeatmap editorClockBeatmap;
        protected virtual bool ScrollUsingMouseWheel => true;

        protected override Container<Drawable> Content => content;

        private readonly Container<Drawable> content = new Container { RelativeSizeAxes = Axes.Both };

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            editorClockBeatmap = CreateEditorClockBeatmap();

            base.Content.AddRange(new Drawable[]
            {
                EditorClock = new EditorClock(editorClockBeatmap, BeatDivisor),
                content
            });

            dependencies.Cache(BeatDivisor);
            dependencies.CacheAs(EditorClock);

            return dependencies;
        }

        protected override void LoadComplete()
        {
            Beatmap.Value = CreateWorkingBeatmap(editorClockBeatmap);

            base.LoadComplete();

            Beatmap.BindValueChanged(beatmapChanged, true);

            AddSliderStep("editor clock rate", 0.0, 2.0, 1.0, v => frequencyAdjustment.Value = v);
        }

        protected virtual IBeatmap CreateEditorClockBeatmap() => new Beatmap();

        private void beatmapChanged(ValueChangedEvent<WorkingBeatmap> e)
        {
            e.OldValue?.Track.RemoveAdjustment(AdjustableProperty.Frequency, frequencyAdjustment);
            e.NewValue.Track.AddAdjustment(AdjustableProperty.Frequency, frequencyAdjustment);
            EditorClock.ChangeSource(e.NewValue.Track);
        }

        protected override bool OnScroll(ScrollEvent e)
        {
            if (!ScrollUsingMouseWheel)
                return false;

            if (e.ScrollDelta.Y > 0)
                EditorClock.SeekBackward(true);
            else
                EditorClock.SeekForward(true);

            return true;
        }
    }
}
