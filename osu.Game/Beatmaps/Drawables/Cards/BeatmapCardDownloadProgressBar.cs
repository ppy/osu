// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Online;
using osu.Game.Overlays;

namespace osu.Game.Beatmaps.Drawables.Cards
{
    public class BeatmapCardDownloadProgressBar : CompositeDrawable
    {
        public IBindable<DownloadState> State => state;
        private readonly Bindable<DownloadState> state = new Bindable<DownloadState>();

        public IBindable<double> Progress => progress;
        private readonly BindableDouble progress = new BindableDouble();

        public override bool IsPresent => true;

        private readonly CircularContainer foreground;

        private readonly Box backgroundFill;
        private readonly Box foregroundFill;

        [Resolved]
        private OsuColour colours { get; set; }

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; }

        public BeatmapCardDownloadProgressBar()
        {
            InternalChildren = new Drawable[]
            {
                new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Child = backgroundFill = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    }
                },
                foreground = new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Child = foregroundFill = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            backgroundFill.Colour = colourProvider.Background6;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            state.BindValueChanged(_ => stateChanged(), true);
            progress.BindValueChanged(_ => progressChanged(), true);
        }

        private void stateChanged()
        {
            switch (state.Value)
            {
                case DownloadState.Downloading:
                    FinishTransforms(true);
                    foregroundFill.Colour = colourProvider.Highlight1;
                    break;

                case DownloadState.Importing:
                    foregroundFill.FadeColour(colours.Yellow, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
                    break;
            }
        }

        private void progressChanged()
        {
            foreground.ResizeWidthTo((float)progress.Value, progress.Value > 0 ? BeatmapCard.TRANSITION_DURATION : 0, Easing.OutQuint);
        }
    }
}
