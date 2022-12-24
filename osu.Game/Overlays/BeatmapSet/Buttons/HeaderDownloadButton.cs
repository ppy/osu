// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;
using osuTK;
using osuTK.Graphics;
using APIUser = osu.Game.Online.API.Requests.Responses.APIUser;
using CommonStrings = osu.Game.Localisation.CommonStrings;

namespace osu.Game.Overlays.BeatmapSet.Buttons
{
    public partial class HeaderDownloadButton : CompositeDrawable, IHasTooltip
    {
        private const int text_size = 12;

        private readonly bool noVideo;

        public LocalisableString TooltipText => BeatmapsetsStrings.ShowDetailsDownloadDefault;

        private readonly IBindable<APIUser> localUser = new Bindable<APIUser>();

        private ShakeContainer shakeContainer;
        private HeaderButton button;

        private BeatmapDownloadTracker downloadTracker;

        private readonly APIBeatmapSet beatmapSet;

        public HeaderDownloadButton(APIBeatmapSet beatmapSet, bool noVideo = false)
        {
            this.beatmapSet = beatmapSet;
            this.noVideo = noVideo;

            Width = 120;
            RelativeSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api, BeatmapModelDownloader beatmaps)
        {
            FillFlowContainer textSprites;

            InternalChildren = new Drawable[]
            {
                shakeContainer = new ShakeContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 5,
                    Child = button = new HeaderButton { RelativeSizeAxes = Axes.Both },
                },
                downloadTracker = new BeatmapDownloadTracker(beatmapSet),
            };

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
                new DownloadProgressBar(beatmapSet)
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                },
            });

            button.Action = () =>
            {
                if (downloadTracker.State.Value != DownloadState.NotDownloaded)
                {
                    shakeContainer.Shake();
                    return;
                }

                beatmaps.Download(beatmapSet, noVideo);
            };

            localUser.BindTo(api.LocalUser);
            localUser.BindValueChanged(userChanged, true);
            button.Enabled.BindValueChanged(enabledChanged, true);

            downloadTracker.State.BindValueChanged(state =>
            {
                switch (state.NewValue)
                {
                    case DownloadState.Downloading:
                        textSprites.Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Text = CommonStrings.Downloading,
                                Font = OsuFont.GetFont(size: text_size, weight: FontWeight.Bold)
                            },
                        };
                        break;

                    case DownloadState.Importing:
                        textSprites.Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Text = CommonStrings.Importing,
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
                                Text = BeatmapsetsStrings.ShowDetailsDownloadDefault,
                                Font = OsuFont.GetFont(size: text_size, weight: FontWeight.Bold)
                            },
                            new OsuSpriteText
                            {
                                Text = getVideoSuffixText(),
                                Font = OsuFont.GetFont(size: text_size - 2, weight: FontWeight.Bold)
                            },
                        };
                        this.FadeIn(200);
                        break;
                }
            }, true);
        }

        private void userChanged(ValueChangedEvent<APIUser> e) => button.Enabled.Value = !(e.NewValue is GuestUser);

        private void enabledChanged(ValueChangedEvent<bool> e) => this.FadeColour(e.NewValue ? Color4.White : Color4.Gray, 200, Easing.OutQuint);

        private LocalisableString getVideoSuffixText()
        {
            if (!beatmapSet.HasVideo)
                return string.Empty;

            return noVideo ? BeatmapsetsStrings.ShowDetailsDownloadNoVideo : BeatmapsetsStrings.ShowDetailsDownloadVideo;
        }
    }
}
