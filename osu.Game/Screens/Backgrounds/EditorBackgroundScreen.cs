// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Storyboards.Drawables;

namespace osu.Game.Screens.Backgrounds
{
    public partial class EditorBackgroundScreen : BackgroundScreen
    {
        private readonly WorkingBeatmap beatmap;
        private readonly Container dimContainer;

        private CancellationTokenSource? cancellationTokenSource;
        private Bindable<float> dimLevel = null!;
        private Bindable<bool> showStoryboard = null!;

        private BeatmapBackground background = null!;
        private Container storyboardContainer = null!;

        private IFrameBasedClock? clockSource;

        public EditorBackgroundScreen(WorkingBeatmap beatmap)
        {
            this.beatmap = beatmap;

            InternalChild = dimContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            dimContainer.AddRange(createContent());
            background = dimContainer.OfType<BeatmapBackground>().Single();
            storyboardContainer = dimContainer.OfType<Container>().Single();

            dimLevel = config.GetBindable<float>(OsuSetting.EditorDim);
            showStoryboard = config.GetBindable<bool>(OsuSetting.EditorShowStoryboard);
        }

        private IEnumerable<Drawable> createContent() =>
        [
            new BeatmapBackground(beatmap) { RelativeSizeAxes = Axes.Both, },
            // this kooky container nesting is here because the storyboard needs a custom clock
            // but also needs it on an isolated-enough level that doesn't break screen stack expiry logic (which happens if the clock was put on `this`),
            // or doesn't make it literally impossible to fade the storyboard in/out in real time (which happens if the fade transforms were to be applied directly to the storyboard).
            new Container
            {
                RelativeSizeAxes = Axes.Both,
                Child = new DrawableStoryboard(beatmap.Storyboard)
                {
                    Clock = clockSource ?? Clock,
                }
            }
        ];

        protected override void LoadComplete()
        {
            base.LoadComplete();

            dimLevel.BindValueChanged(_ => dimContainer.FadeColour(OsuColour.Gray(1 - dimLevel.Value), 500, Easing.OutQuint), true);
            showStoryboard.BindValueChanged(_ => updateState());
            updateState(0);
        }

        private void updateState(double duration = 500)
        {
            storyboardContainer.FadeTo(showStoryboard.Value ? 1 : 0, duration, Easing.OutQuint);
            // yes, this causes overdraw, but is also a (crude) fix for bad-looking transitions on screen entry
            // caused by the previous background on the background stack poking out from under this one and then instantly fading out
            background.FadeColour(beatmap.Storyboard.ReplacesBackground && showStoryboard.Value ? Colour4.Black : Colour4.White, duration, Easing.OutQuint);
        }

        public void ChangeClockSource(IFrameBasedClock frameBasedClock)
        {
            clockSource = frameBasedClock;
            if (IsLoaded)
                storyboardContainer.Child.Clock = frameBasedClock;
        }

        public void RefreshBackground()
        {
            cancellationTokenSource?.Cancel();
            LoadComponentsAsync(createContent(), loaded =>
            {
                dimContainer.Clear();
                dimContainer.AddRange(loaded);

                background = dimContainer.OfType<BeatmapBackground>().Single();
                storyboardContainer = dimContainer.OfType<Container>().Single();
                updateState(0);
            }, (cancellationTokenSource ??= new CancellationTokenSource()).Token);
        }

        public override bool Equals(BackgroundScreen? other)
        {
            if (other is not EditorBackgroundScreen otherBeatmapBackground)
                return false;

            return base.Equals(other) && beatmap == otherBeatmapBackground.beatmap;
        }
    }
}
