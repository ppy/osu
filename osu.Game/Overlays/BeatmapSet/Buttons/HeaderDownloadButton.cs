// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Overlays.BeatmapListing.Panels;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.BeatmapSet.Buttons
{
    public class HeaderDownloadButton : BeatmapDownloadTrackingComposite, IHasTooltip
    {
        private const int text_size = 17;

        private readonly bool noVideo;
        private readonly bool IsMini;
        private readonly bool NoSuffix;

        public LocalisableString TooltipText => button.Enabled.Value ? "下载该谱面" : "请先登录再进行下载";

        private readonly IBindable<User> localUser = new Bindable<User>();
        private BindableBool UseSayobot = new BindableBool();

        private ShakeContainer shakeContainer;
        private HeaderButton button;

        public HeaderDownloadButton(BeatmapSetInfo beatmapSet, bool noVideo = false, bool IsMini = false, bool NoSuffix = false)
            : base(beatmapSet)
        {
            this.noVideo = noVideo;
            this.IsMini = IsMini;
            this.NoSuffix = NoSuffix;

            Width = 120;
            RelativeSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api, BeatmapManager beatmaps, MConfigManager mfconfig)
        {
            FillFlowContainer textSprites;

            AddInternal(shakeContainer = new ShakeContainer
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 5,
                Child = button = new HeaderButton { RelativeSizeAxes = Axes.Both },
            });

            button.AddRange(new Drawable[]
            {
                new Container
                {
                    Padding = new MarginPadding { Horizontal = 10 },
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        textSprites = new FillFlowContainer
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            AutoSizeAxes = Axes.Both,
                            AutoSizeDuration = 500,
                            AutoSizeEasing = Easing.OutQuint,
                            Direction = FillDirection.Vertical,
                        },
                        new SpriteIcon
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Icon = FontAwesome.Solid.Download,
                            Size = new Vector2(18),
                        },
                    }
                },
                new DownloadProgressBar(BeatmapSet.Value)
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                },
            });

            button.Action = () =>
            {
                if (State.Value != DownloadState.NotDownloaded)
                {
                    shakeContainer.Shake();
                    return;
                }

                beatmaps.Download(BeatmapSet.Value, mfconfig.Get<bool>(MSetting.UseSayobot), noVideo, IsMini);
            };

            localUser.BindTo(api.LocalUser);
            localUser.BindValueChanged(userChanged, true);
            button.Enabled.BindValueChanged(enabledChanged, true);

            State.BindValueChanged(state =>
            {
                switch (state.NewValue)
                {
                    case DownloadState.Downloading:
                        textSprites.Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Text = "下载中...",
                                Font = OsuFont.GetFont(size: text_size, weight: FontWeight.Bold)
                            },
                        };
                        break;

                    case DownloadState.Importing:
                        textSprites.Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Text = "导入中...",
                                Font = OsuFont.GetFont(size: text_size, weight: FontWeight.Bold)
                            },
                        };
                        break;

                    case DownloadState.LocallyAvailable:
                        this.FadeOut(200);
                        break;

                    case DownloadState.NotDownloaded:
                        textSprites.Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Text = "下载",
                                Font = OsuFont.GetFont(size: text_size, weight: FontWeight.Bold)
                            },
                            new OsuSpriteText
                            {
                                Text = NoSuffix ? string.Empty : getVideoSuffixText(),
                                Font = OsuFont.GetFont(size: text_size - 2, weight: FontWeight.Bold)
                            },
                        };
                        this.FadeIn(200);
                        break;
                }
            }, true);
        }

        private void userChanged(ValueChangedEvent<User> e) => button.Enabled.Value = !(e.NewValue is GuestUser);

        private void enabledChanged(ValueChangedEvent<bool> e) => this.FadeColour(e.NewValue ? Color4.White : Color4.Gray, 200, Easing.OutQuint);

        private string getVideoSuffixText()
        {
            if (!BeatmapSet.Value.OnlineInfo.HasVideo && !BeatmapSet.Value.OnlineInfo.HasStoryboard)
                return string.Empty;

            return (IsMini == true ? "Mini" : (noVideo ? "不带视频" : "带视频"));
        }
    }
}
