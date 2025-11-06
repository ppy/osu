// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapSet;
using osu.Game.Resources.Localisation.Web;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match.BeatmapSelect
{
    public partial class BeatmapCardMatchmaking : BeatmapCard
    {
        private readonly APIBeatmap beatmap;

        protected override Drawable IdleContent => idleBottomContent;
        protected override Drawable DownloadInProgressContent => downloadProgressBar;

        public const float HEIGHT = 80;

        [Cached]
        private readonly BeatmapCardContent content;

        private BeatmapCardThumbnail thumbnail = null!;
        private CollapsibleButtonContainer buttonContainer = null!;

        private FillFlowContainer idleBottomContent = null!;
        private BeatmapCardDownloadProgressBar downloadProgressBar = null!;

        public AvatarOverlay SelectionOverlay = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private BeatmapSetOverlay? beatmapSetOverlay { get; set; }

        public BeatmapCardMatchmaking(APIBeatmap beatmap)
            : base(beatmap.BeatmapSet!, false)
        {
            this.beatmap = beatmap;
            content = new BeatmapCardContent(HEIGHT);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Width = WIDTH;
            Height = HEIGHT;

            FillFlowContainer leftIconArea = null!;
            FillFlowContainer titleBadgeArea = null!;
            GridContainer artistContainer = null!;

            Child = content.With(c =>
            {
                c.MainContent = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        thumbnail = new BeatmapCardThumbnail(BeatmapSet, BeatmapSet, keepLoaded: true)
                        {
                            Name = @"Left (icon) area",
                            Size = new Vector2(HEIGHT),
                            Padding = new MarginPadding { Right = CORNER_RADIUS },
                            Child = leftIconArea = new FillFlowContainer
                            {
                                Margin = new MarginPadding(4),
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(1)
                            }
                        },
                        buttonContainer = new CollapsibleButtonContainer(BeatmapSet, allowNavigationToBeatmap: false, keepBackgroundLoaded: true)
                        {
                            X = HEIGHT - CORNER_RADIUS,
                            Width = WIDTH - HEIGHT + CORNER_RADIUS,
                            FavouriteState = { BindTarget = FavouriteState },
                            ButtonsCollapsedWidth = 0,
                            ButtonsExpandedWidth = 24,
                            Children = new Drawable[]
                            {
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Direction = FillDirection.Vertical,
                                    Children = new Drawable[]
                                    {
                                        new GridContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            ColumnDimensions = new[]
                                            {
                                                new Dimension(),
                                                new Dimension(GridSizeMode.AutoSize),
                                            },
                                            RowDimensions = new[]
                                            {
                                                new Dimension(GridSizeMode.AutoSize)
                                            },
                                            Content = new[]
                                            {
                                                new Drawable[]
                                                {
                                                    new TruncatingSpriteText
                                                    {
                                                        Text = new RomanisableString(BeatmapSet.TitleUnicode, BeatmapSet.Title),
                                                        Font = OsuFont.Default.With(size: 18f, weight: FontWeight.SemiBold),
                                                        RelativeSizeAxes = Axes.X,
                                                    },
                                                    titleBadgeArea = new FillFlowContainer
                                                    {
                                                        Anchor = Anchor.BottomRight,
                                                        Origin = Anchor.BottomRight,
                                                        AutoSizeAxes = Axes.Both,
                                                        Direction = FillDirection.Horizontal,
                                                    }
                                                }
                                            }
                                        },
                                        artistContainer = new GridContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            ColumnDimensions = new[]
                                            {
                                                new Dimension(),
                                                new Dimension(GridSizeMode.AutoSize)
                                            },
                                            RowDimensions = new[]
                                            {
                                                new Dimension(GridSizeMode.AutoSize)
                                            },
                                            Content = new[]
                                            {
                                                new[]
                                                {
                                                    new TruncatingSpriteText
                                                    {
                                                        Text = createArtistText(),
                                                        Font = OsuFont.Default.With(size: 14f, weight: FontWeight.SemiBold),
                                                        RelativeSizeAxes = Axes.X,
                                                    },
                                                    Empty()
                                                },
                                            }
                                        },
                                        new LinkFlowContainer(s =>
                                        {
                                            s.Shadow = false;
                                            s.Font = OsuFont.GetFont(size: 11f, weight: FontWeight.SemiBold);
                                        }).With(d =>
                                        {
                                            d.AutoSizeAxes = Axes.Both;
                                            d.Margin = new MarginPadding { Top = 1 };
                                            d.AddText("mapped by ", t => t.Colour = colourProvider.Content2);
                                            d.AddUserLink(BeatmapSet.Author);
                                        }),
                                    }
                                },
                                new Container
                                {
                                    Name = @"Bottom content",
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Children = new Drawable[]
                                    {
                                        idleBottomContent = new FillFlowContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Direction = FillDirection.Vertical,
                                            Spacing = new Vector2(0, 2),
                                            AlwaysPresent = true,
                                            Children = new Drawable[]
                                            {
                                                new Container
                                                {
                                                    Masking = true,
                                                    CornerRadius = CORNER_RADIUS,
                                                    RelativeSizeAxes = Axes.X,
                                                    AutoSizeAxes = Axes.Y,
                                                    Children = new Drawable[]
                                                    {
                                                        new Box
                                                        {
                                                            Colour = colours.ForStarDifficulty(beatmap.StarRating).Darken(0.8f),
                                                            RelativeSizeAxes = Axes.Both,
                                                        },
                                                        new FillFlowContainer
                                                        {
                                                            Padding = new MarginPadding(4),
                                                            RelativeSizeAxes = Axes.X,
                                                            AutoSizeAxes = Axes.Y,
                                                            Direction = FillDirection.Horizontal,
                                                            Spacing = new Vector2(6, 0),
                                                            Children = new Drawable[]
                                                            {
                                                                new StarRatingDisplay(new StarDifficulty(beatmap.StarRating, 0), StarRatingDisplaySize.Small, animated: true)
                                                                {
                                                                    Origin = Anchor.CentreLeft,
                                                                    Anchor = Anchor.CentreLeft,
                                                                    Scale = new Vector2(0.9f),
                                                                },
                                                                new TruncatingSpriteText
                                                                {
                                                                    Text = beatmap.DifficultyName,
                                                                    Font = OsuFont.Style.Caption1.With(weight: FontWeight.Bold),
                                                                    Anchor = Anchor.CentreLeft,
                                                                    Origin = Anchor.CentreLeft,
                                                                }
                                                            }
                                                        },
                                                    }
                                                },
                                            }
                                        },
                                        downloadProgressBar = new BeatmapCardDownloadProgressBar
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Height = 5,
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            State = { BindTarget = DownloadTracker.State },
                                            Progress = { BindTarget = DownloadTracker.Progress }
                                        }
                                    }
                                },
                                SelectionOverlay = new AvatarOverlay
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                }
                            }
                        }
                    }
                };
                c.Expanded.BindTarget = Expanded;
            });

            if (BeatmapSet.HasVideo)
                leftIconArea.Add(new VideoIconPill { IconSize = new Vector2(16) });

            if (BeatmapSet.HasStoryboard)
                leftIconArea.Add(new StoryboardIconPill { IconSize = new Vector2(16) });

            if (BeatmapSet.FeaturedInSpotlight)
            {
                titleBadgeArea.Add(new SpotlightBeatmapBadge
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Margin = new MarginPadding { Left = 4 }
                });
            }

            if (BeatmapSet.HasExplicitContent)
            {
                titleBadgeArea.Add(new ExplicitContentBeatmapBadge
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Margin = new MarginPadding { Left = 4 }
                });
            }

            if (BeatmapSet.TrackId != null)
            {
                artistContainer.Content[0][1] = new FeaturedArtistBeatmapBadge
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Margin = new MarginPadding { Left = 4 }
                };
            }
        }

        private LocalisableString createArtistText()
        {
            var romanisableArtist = new RomanisableString(BeatmapSet.ArtistUnicode, BeatmapSet.Artist);
            return BeatmapsetsStrings.ShowDetailsByArtist(romanisableArtist);
        }

        protected override void UpdateState()
        {
            base.UpdateState();

            bool showDetails = IsHovered;

            buttonContainer.ShowDetails.Value = showDetails;
            thumbnail.Dimmed.Value = showDetails;
        }

        public override MenuItem[] ContextMenuItems
        {
            get
            {
                List<MenuItem> items = new List<MenuItem>
                {
                    new OsuMenuItem(ContextMenuStrings.ViewBeatmap, MenuItemType.Highlighted, () => beatmapSetOverlay?.FetchAndShowBeatmap(beatmap.OnlineID))
                };

                foreach (var button in buttonContainer.Buttons)
                {
                    if (button.Enabled.Value)
                        items.Add(new OsuMenuItem(button.TooltipText.ToSentence(), MenuItemType.Standard, () => button.TriggerClick()));
                }

                return items.ToArray();
            }
        }

        public partial class AvatarOverlay : CompositeDrawable
        {
            private readonly Container<SelectionAvatar> avatars;

            private Sample? userAddedSample;
            private double? lastSamplePlayback;

            [Resolved]
            private IAPIProvider api { get; set; } = null!;

            public AvatarOverlay()
            {
                AutoSizeAxes = Axes.Both;

                InternalChild = avatars = new Container<SelectionAvatar>
                {
                    AutoSizeAxes = Axes.X,
                    Height = SelectionAvatar.AVATAR_SIZE,
                };

                Padding = new MarginPadding { Vertical = 5 };
            }

            [BackgroundDependencyLoader]
            private void load(AudioManager audio)
            {
                userAddedSample = audio.Samples.Get(@"Multiplayer/player-ready");
            }

            public bool AddUser(APIUser user)
            {
                if (avatars.Any(a => a.User.Id == user.Id))
                    return false;

                var avatar = new SelectionAvatar(user, user.Equals(api.LocalUser.Value));

                avatars.Add(avatar);

                if (lastSamplePlayback == null || Time.Current - lastSamplePlayback > OsuGameBase.SAMPLE_DEBOUNCE_TIME)
                {
                    userAddedSample?.Play();
                    lastSamplePlayback = Time.Current;
                }

                updateAvatarLayout();

                avatar.FinishTransforms();

                return true;
            }

            public bool RemoveUser(int id)
            {
                if (avatars.SingleOrDefault(a => a.User.Id == id) is not SelectionAvatar avatar)
                    return false;

                avatar.PopOutAndExpire();
                avatars.ChangeChildDepth(avatar, float.MaxValue);

                updateAvatarLayout();

                return true;
            }

            private void updateAvatarLayout()
            {
                const double stagger = 30;
                const float spacing = 4;

                double delay = 0;
                float x = 0;

                for (int i = avatars.Count - 1; i >= 0; i--)
                {
                    var avatar = avatars[i];

                    if (avatar.Expired)
                        continue;

                    avatar.Delay(delay).MoveToX(x, 500, Easing.OutElasticQuarter);

                    x -= avatar.LayoutSize.X + spacing;

                    delay += stagger;
                }
            }

            public partial class SelectionAvatar : CompositeDrawable
            {
                public const float AVATAR_SIZE = 30;

                public APIUser User { get; }

                public bool Expired { get; private set; }

                private readonly MatchmakingAvatar avatar;

                public SelectionAvatar(APIUser user, bool isOwnUser)
                {
                    User = user;
                    Size = new Vector2(AVATAR_SIZE);

                    InternalChild = avatar = new MatchmakingAvatar(user, isOwnUser)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    };
                }

                protected override void LoadComplete()
                {
                    base.LoadComplete();

                    avatar.ScaleTo(0)
                          .ScaleTo(1, 500, Easing.OutElasticHalf)
                          .FadeIn(200);
                }

                public void PopOutAndExpire()
                {
                    avatar.ScaleTo(0, 400, Easing.OutExpo);

                    this.FadeOut(100).Expire();
                    Expired = true;
                }
            }
        }
    }
}
