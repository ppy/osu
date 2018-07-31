// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API;
using osu.Game.Users;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.BeatmapSet.Buttons
{
    public class DownloadButton : HeaderButton, IHasTooltip
    {
        public string TooltipText => Enabled ? null : "You gotta be an osu!supporter to download for now 'yo";

        private readonly IBindable<User> localUser = new Bindable<User>();

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

        [BackgroundDependencyLoader]
        private void load(APIAccess api)
        {
            localUser.BindTo(api.LocalUser);
            localUser.BindValueChanged(userChanged, true);
            Enabled.BindValueChanged(enabledChanged, true);
        }

        private void userChanged(User user) => Enabled.Value = user.IsSupporter;

        private void enabledChanged(bool enabled) => this.FadeColour(enabled ? Color4.White : Color4.Gray, 200, Easing.OutQuint);
    }
}
