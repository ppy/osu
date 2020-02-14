// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Overlays.Direct;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.BeatmapSet.Buttons
{
    public class HeaderDownloadButton : BeatmapDownloadTrackingComposite, IHasTooltip
    {
        private readonly bool noVideo;

        public string TooltipText => button.Enabled.Value ? "download this beatmap" : "login to download";

        private readonly IBindable<User> localUser = new Bindable<User>();

        private ShakeContainer shakeContainer;
        private HeaderButton button;

        public HeaderDownloadButton(BeatmapSetInfo beatmapSet, bool noVideo = false)
            : base(beatmapSet)
        {
            this.noVideo = noVideo;

            Width = 120;
            RelativeSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api, BeatmapManager beatmaps)
        {
            FillFlowContainer textSprites;

            AddRangeInternal(new Drawable[]
            {
                shakeContainer = new ShakeContainer
                {
                    Depth = -1,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 5,
                    Children = new Drawable[]
                    {
                        button = new HeaderButton { RelativeSizeAxes = Axes.Both },
                        new Container
                        {
                            // cannot nest inside here due to the structure of button (putting things in its own content).
                            // requires framework fix.
                            Padding = new MarginPadding { Horizontal = 10 },
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                textSprites = new FillFlowContainer
                                {
                                    Depth = -1,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    AutoSizeAxes = Axes.Both,
                                    AutoSizeDuration = 500,
                                    AutoSizeEasing = Easing.OutQuint,
                                    Direction = FillDirection.Vertical,
                                },
                                new SpriteIcon
                                {
                                    Depth = -1,
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    Icon = FontAwesome.Solid.Download,
                                    Size = new Vector2(16),
                                    Margin = new MarginPadding { Right = 5 },
                                },
                            }
                        },
                        new DownloadProgressBar(BeatmapSet.Value)
                        {
                            Depth = -2,
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                        },
                    },
                },
            });

            button.Action = () =>
            {
                if (State.Value != DownloadState.NotDownloaded)
                {
                    shakeContainer.Shake();
                    return;
                }

                beatmaps.Download(BeatmapSet.Value, noVideo);
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
                                Text = "Downloading...",
                                Font = OsuFont.GetFont(size: 13, weight: FontWeight.Bold)
                            },
                        };
                        break;

                    case DownloadState.Downloaded:
                        textSprites.Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Text = "Importing...",
                                Font = OsuFont.GetFont(size: 13, weight: FontWeight.Bold)
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
                                Text = "Download",
                                Font = OsuFont.GetFont(size: 13, weight: FontWeight.Bold)
                            },
                            new OsuSpriteText
                            {
                                Text = getVideoSuffixText(),
                                Font = OsuFont.GetFont(size: 11, weight: FontWeight.Bold)
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
            if (!BeatmapSet.Value.OnlineInfo.HasVideo)
                return string.Empty;

            return noVideo ? "without Video" : "with Video";
        }
    }
}
