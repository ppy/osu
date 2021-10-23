// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Online;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Beatmaps.Drawables.Cards.Buttons
{
    public class DownloadButton : CompositeDrawable
    {
        protected readonly DownloadIcon Download;
        protected readonly PlayIcon Play;
        protected readonly BeatmapDownloadTracker Tracker;

        private readonly CircularProgress downloadProgress;

        public DownloadButton(APIBeatmapSet beatmapSet)
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            InternalChildren = new Drawable[]
            {
                Tracker = new BeatmapDownloadTracker(beatmapSet),
                Download = new DownloadIcon(),
                downloadProgress = new CircularProgress
                {
                    Size = new Vector2(16),
                    InnerRadius = 0.1f,
                },
                Play = new PlayIcon()
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            downloadProgress.Colour = colourProvider.Highlight1;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            ((IBindable<double>)downloadProgress.Current).BindTo(Tracker.Progress);
        }

        protected class DownloadIcon : BeatmapCardIconButton
        {
            public DownloadIcon()
            {
                Icon.Icon = FontAwesome.Solid.Download;
            }
        }

        protected class PlayIcon : BeatmapCardIconButton
        {
            public PlayIcon()
            {
                Icon.Icon = FontAwesome.Regular.PlayCircle;
            }
        }

        // TODO: implement behaviour
    }
}
