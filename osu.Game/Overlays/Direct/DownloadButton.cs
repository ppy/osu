// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using OpenTK;

namespace osu.Game.Overlays.Direct
{
    public class DownloadButton : OsuAnimatedButton
    {
        private readonly BeatmapSetInfo beatmapSet;
        private readonly SpriteIcon icon;
        private readonly SpriteIcon checkmark;
        private readonly BeatmapSetDownloader downloader;
        private readonly Box background;

        private OsuColour colours;

        public DownloadButton(BeatmapSetInfo beatmapSet, bool noVideo = false)
        {
            this.beatmapSet = beatmapSet;

            AddRange(new Drawable[]
            {
                downloader = new BeatmapSetDownloader(beatmapSet, noVideo),
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = float.MaxValue
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
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            downloader.DownloadState.BindValueChanged(updateState, true);
            FinishTransforms(true);
        }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(OsuColour colours, OsuGame game)
        {
            this.colours = colours;

            Action = () =>
            {
                switch (downloader.DownloadState.Value)
                {
                    case BeatmapSetDownloader.DownloadStatus.Downloading:
                        // todo: replace with ShakeContainer after https://github.com/ppy/osu/pull/2909 is merged.
                        Content.MoveToX(-5, 50, Easing.OutSine).Then()
                               .MoveToX(5, 100, Easing.InOutSine).Then()
                               .MoveToX(-5, 100, Easing.InOutSine).Then()
                               .MoveToX(0, 50, Easing.InSine);
                        break;
                    case BeatmapSetDownloader.DownloadStatus.Downloaded:
                        game.PresentBeatmap(beatmapSet);
                        break;
                    default:
                        downloader.Download();
                        break;
                }
            };
        }

        private void updateState(BeatmapSetDownloader.DownloadStatus state)
        {
            switch (state)
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
