// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.Direct
{
    public class DownloadButton : OsuClickableContainer
    {
        private readonly SpriteIcon icon;
        private readonly SpriteIcon checkmark;
        private readonly BeatmapSetDownloader downloader;
        private readonly Box background;

        private OsuColour colours;

        public DownloadButton(BeatmapSetInfo set, bool noVideo = false)
        {
            Children = new Drawable[]
            {
                downloader = new BeatmapSetDownloader(set, noVideo),
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    CornerRadius = 17,
                    Masking = true,
                    Child = background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                },
                icon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(13),
                    Icon = FontAwesome.fa_download,
                },
                checkmark = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    X = 8,
                    Size = Vector2.Zero,
                    Icon = FontAwesome.fa_check,
                }
            };

            Action = () =>
            {
                if (downloader.DownloadState.Value == BeatmapSetDownloader.DownloadStatus.Downloading)
                {
                    Content.MoveToX(-5, 50, Easing.OutSine).Then()
                           .MoveToX(5, 100, Easing.InOutSine).Then()
                           .MoveToX(-5, 100, Easing.InOutSine).Then()
                           .MoveToX(0, 50, Easing.InSine);
                }
                else if (downloader.DownloadState.Value == BeatmapSetDownloader.DownloadStatus.Downloaded)
                {
                    // TODO: Jump to song select with this set when the capability is implemented
                }
                else
                {
                    downloader.Download();
                }
            };

            downloader.DownloadState.ValueChanged += _ => updateState();

            Colour = Color4.White;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateState();
        }

        [BackgroundDependencyLoader(permitNulls:true)]
        private void load(OsuColour colours)
        {
            this.colours = colours;
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            Content.ScaleTo(0.9f, 1000, Easing.Out);
            return base.OnMouseDown(state, args);
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            Content.ScaleTo(1f, 500, Easing.OutElastic);
            return base.OnMouseUp(state, args);
        }

        protected override bool OnHover(InputState state)
        {
            Content.ScaleTo(1.1f, 500, Easing.OutElastic);
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            Content.ScaleTo(1f, 500, Easing.OutElastic);
        }

        private void updateState()
        {
            if (!IsLoaded)
                return;

            switch (downloader.DownloadState.Value)
            {
                case BeatmapSetDownloader.DownloadStatus.NotDownloaded:
                    background.FadeColour(colours.Gray4, 500, Easing.InOutExpo);
                    icon.MoveToX(0, 500, Easing.InOutExpo);
                    checkmark.ScaleTo(Vector2.Zero, 500, Easing.InOutExpo);
                    break;

                case BeatmapSetDownloader.DownloadStatus.Downloading:
                    background.FadeColour(colours.Blue, 500, Easing.InOutExpo);
                    icon.MoveToX(0, 500, Easing.InOutExpo);
                    checkmark.ScaleTo(Vector2.Zero, 500, Easing.InOutExpo);
                    break;

                case BeatmapSetDownloader.DownloadStatus.Downloaded:
                    background.FadeColour(colours.Green, 500, Easing.InOutExpo);
                    icon.MoveToX(-8, 500, Easing.InOutExpo);
                    checkmark.ScaleTo(new Vector2(13), 500, Easing.InOutExpo);
                    break;
            }
        }
    }
}
