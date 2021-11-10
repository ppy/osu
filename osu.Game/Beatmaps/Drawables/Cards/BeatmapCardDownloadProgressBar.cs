// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Online;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;

namespace osu.Game.Beatmaps.Drawables.Cards
{
    public class BeatmapCardDownloadProgressBar : CompositeDrawable
    {
        private readonly BindableBool isActive = new BindableBool();
        public IBindable<bool> IsActive => isActive;

        public override bool IsPresent => true;

        private readonly BeatmapDownloadTracker tracker;
        private readonly CircularContainer background;
        private readonly CircularContainer foreground;

        private readonly Box backgroundFill;
        private readonly Box foregroundFill;

        [Resolved]
        private OsuColour colours { get; set; }

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; }

        public BeatmapCardDownloadProgressBar(APIBeatmapSet beatmapSet)
        {
            InternalChildren = new Drawable[]
            {
                tracker = new BeatmapDownloadTracker(beatmapSet),
                background = new CircularContainer
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

            tracker.State.BindValueChanged(_ => stateChanged(), true);
            tracker.Progress.BindValueChanged(_ => progressChanged(), true);
        }

        private void stateChanged()
        {
            switch (tracker.State.Value)
            {
                case DownloadState.Downloading:
                    FinishTransforms(true);
                    foregroundFill.Colour = colourProvider.Highlight1;
                    isActive.Value = true;
                    break;

                case DownloadState.Importing:
                    foregroundFill.FadeColour(colours.Yellow, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
                    isActive.Value = true;
                    break;

                default:
                    isActive.Value = false;
                    break;
            }
        }

        private void progressChanged()
        {
            double progress = tracker.Progress.Value;
            foreground.ResizeWidthTo((float)progress, progress > 0 ? BeatmapCard.TRANSITION_DURATION : 0, Easing.OutQuint);
        }
    }
}
