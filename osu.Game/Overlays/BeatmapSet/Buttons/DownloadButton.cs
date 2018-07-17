// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using OpenTK;

namespace osu.Game.Overlays.BeatmapSet.Buttons
{
    public class DownloadButton : HeaderButton
    {
        public DownloadButton(BeatmapSetInfo set, bool noVideo = false)
        {
            Width = 120;

            BeatmapSetDownloader downloader;
            Add(new Container
            {
                Depth = -1,
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding { Horizontal = 10 },
                Children = new Drawable[]
                {
                    downloader = new BeatmapSetDownloader(set, noVideo),
                    new FillFlowContainer
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Children = new[]
                        {
                            new OsuSpriteText
                            {
                                Text = "Download",
                                TextSize = 13,
                                Font = @"Exo2.0-Bold",
                            },
                            new OsuSpriteText
                            {
                                Text = set.OnlineInfo.HasVideo && noVideo ? "without Video" : string.Empty,
                                TextSize = 11,
                                Font = @"Exo2.0-Bold",
                            },
                        },
                    },
                    new SpriteIcon
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        Icon = FontAwesome.fa_download,
                        Size = new Vector2(16),
                        Margin = new MarginPadding { Right = 5 },
                    },
                },
            });

            Action = () =>
            {
                if (downloader.DownloadState.Value == BeatmapSetDownloader.DownloadStatus.Downloading)
                {
                    Content.MoveToX(-5, 50, Easing.OutSine).Then()
                           .MoveToX(5, 100, Easing.InOutSine).Then()
                           .MoveToX(-5, 100, Easing.InOutSine).Then()
                           .MoveToX(0, 50, Easing.InSine);
                    return;
                }

                downloader.Download();
            };

            downloader.DownloadState.ValueChanged += state =>
            {
                switch (state)
                {
                    case BeatmapSetDownloader.DownloadStatus.Downloaded:
                        this.FadeOut(200);
                        break;
                    case BeatmapSetDownloader.DownloadStatus.NotDownloaded:
                        this.FadeIn(200);
                        break;
                }
            };
        }
    }
}
