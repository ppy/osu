// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match.BeatmapSelect
{
    public partial class BeatmapSelectPanel : Container
    {
        public static readonly Vector2 SIZE = new Vector2(300, 70);

        public bool AllowSelection { get; set; }

        public readonly MultiplayerPlaylistItem Item;

        public Action<MultiplayerPlaylistItem>? Action { private get; init; }

        private const float corner_radius = 6;
        private const float border_width = 3;

        private Container scaleContainer = null!;
        private AvatarOverlay selectionOverlay = null!;
        private Drawable lighting = null!;

        private Container border = null!;
        private Container mainContent = null!;

        public override bool PropagatePositionalInputSubTree => AllowSelection;

        public BeatmapSelectPanel(MultiplayerPlaylistItem item)
        {
            Item = item;
            Size = SIZE;
        }

        [BackgroundDependencyLoader]
        private void load(BeatmapLookupCache lookupCache, OverlayColourProvider colourProvider)
        {
            InternalChild = scaleContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children = new[]
                {
                    mainContent = new Container
                    {
                        Masking = true,
                        CornerRadius = corner_radius,
                        CornerExponent = 10,
                        RelativeSizeAxes = Axes.Both,
                        Children = new[]
                        {
                            new HoverClickSounds(),
                            lighting = new Box
                            {
                                Blending = BlendingParameters.Additive,
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0,
                            },
                            selectionOverlay = new AvatarOverlay
                            {
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopRight,
                            }
                        }
                    },
                    border = new Container
                    {
                        Alpha = 0,
                        Masking = true,
                        CornerRadius = corner_radius,
                        Blending = BlendingParameters.Additive,
                        CornerExponent = 10,
                        RelativeSizeAxes = Axes.Both,
                        BorderThickness = border_width,
                        BorderColour = colourProvider.Light1,
                        EdgeEffect = new EdgeEffectParameters
                        {
                            Type = EdgeEffectType.Glow,
                            Radius = 40,
                            Roundness = 300,
                            Colour = colourProvider.Light3.Opacity(0.1f),
                        },
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                AlwaysPresent = true,
                                Alpha = 0,
                                Colour = Color4.Black,
                                RelativeSizeAxes = Axes.Both,
                            },
                        }
                    },
                }
            };
            lookupCache.GetBeatmapAsync(Item.BeatmapID).ContinueWith(b => Schedule(() =>
            {
                var beatmap = b.GetResultSafely()!;
                beatmap.StarRating = Item.StarRating;

                mainContent.Add(new BeatmapPanel(beatmap)
                {
                    Depth = float.MaxValue,
                    RelativeSizeAxes = Axes.Both
                });
            }));
        }

        public bool AddUser(APIUser user, bool isOwnUser = false) => selectionOverlay.AddUser(user, isOwnUser);
        public bool RemoveUser(APIUser user) => selectionOverlay.RemoveUser(user.Id);

        protected override bool OnHover(HoverEvent e)
        {
            lighting.FadeTo(0.2f, 50)
                    .Then()
                    .FadeTo(0.1f, 300);

            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);

            lighting.FadeOut(200);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button == MouseButton.Left)
            {
                scaleContainer.ScaleTo(0.95f, 400, Easing.OutExpo);
                return true;
            }

            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButton.Left)
            {
                scaleContainer.ScaleTo(1f, 500, Easing.OutElasticHalf);
            }
        }

        protected override bool OnClick(ClickEvent e)
        {
            Action?.Invoke(Item);

            lighting.FadeTo(0.5f, 50)
                    .Then()
                    .FadeTo(0.1f, 400);

            return true;
        }

        public void ShowChosenBorder()
        {
            border.FadeTo(1, 1000, Easing.OutQuint);
        }

        public void ShowBorder()
        {
            border.FadeTo(1, 80, Easing.OutQuint)
                  .Then()
                  .FadeTo(0.7f, 800, Easing.OutQuint);
        }

        public void HideBorder()
        {
            border.FadeOut(500, Easing.OutQuint);
        }

        public void FadeInAndEnterFromBelow(double duration = 500, double delay = 0, float distance = 200)
        {
            scaleContainer
                .FadeOut()
                .MoveToY(distance)
                .Delay(delay)
                .FadeIn(duration / 2)
                .MoveToY(0, duration, Easing.OutExpo);
        }

        public void PopOutAndExpire(double duration = 400, double delay = 0, Easing easing = Easing.InCubic)
        {
            AllowSelection = false;

            scaleContainer.Delay(delay)
                          .ScaleTo(0, duration, easing)
                          .FadeOut(duration);

            this.Delay(delay + duration).FadeOut().Expire();
        }

        // TODO: combine following two classes with above implementation for simplicity?
        private partial class BeatmapPanel : CompositeDrawable, IHasContextMenu
        {
            private readonly APIBeatmap beatmap;

            private Container content = null!;
            private UpdateableOnlineBeatmapSetCover cover = null!;

            public BeatmapPanel(APIBeatmap beatmap)
            {
                this.beatmap = beatmap;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                Masking = true;
                CornerRadius = 6;
                CornerExponent = 10;

                InternalChildren = new Drawable[]
                {
                    cover = new UpdateableOnlineBeatmapSetCover(BeatmapSetCoverType.Card, timeBeforeLoad: 0, timeBeforeUnload: 10000)
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = ColourInfo.GradientHorizontal(
                            colourProvider.Background4.Opacity(0.7f),
                            colourProvider.Background4.Opacity(0.4f)
                        )
                    },
                    content = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                updateContent();
                FinishTransforms(true);
            }

            private void updateContent()
            {
                foreach (var child in content.Children)
                    child.FadeOut(300).Expire();

                cover.OnlineInfo = beatmap.BeatmapSet;

                var panelContent = new BeatmapPanelContent(beatmap)
                {
                    RelativeSizeAxes = Axes.Both,
                };

                content.Add(panelContent);

                panelContent.FadeInFromZero(300);
            }

            [Resolved]
            private BeatmapSetOverlay? beatmapSetOverlay { get; set; }

            public MenuItem[] ContextMenuItems => new MenuItem[]
            {
                new OsuMenuItem(ContextMenuStrings.ViewBeatmap, MenuItemType.Highlighted, () =>
                {
                    beatmapSetOverlay?.FetchAndShowBeatmapSet(beatmap.BeatmapSet!.OnlineID);
                }),
            };

            private partial class BeatmapPanelContent : CompositeDrawable
            {
                private readonly APIBeatmap beatmap;

                public BeatmapPanelContent(APIBeatmap beatmap)
                {
                    this.beatmap = beatmap;
                }

                [BackgroundDependencyLoader]
                private void load()
                {
                    InternalChild = new FillFlowContainer
                    {
                        Direction = FillDirection.Vertical,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Padding = new MarginPadding { Horizontal = 12 },
                        Children = new Drawable[]
                        {
                            new TruncatingSpriteText
                            {
                                Text = new RomanisableString(beatmap.Metadata.TitleUnicode, beatmap.Metadata.TitleUnicode),
                                Font = OsuFont.Default.With(size: 19, weight: FontWeight.SemiBold),
                                RelativeSizeAxes = Axes.X,
                            },
                            new TextFlowContainer(s =>
                            {
                                s.Font = OsuFont.GetFont(size: 16, weight: FontWeight.SemiBold);
                            }).With(d =>
                            {
                                d.RelativeSizeAxes = Axes.X;
                                d.AutoSizeAxes = Axes.Y;
                                d.AddText("by ");
                                d.AddText(new RomanisableString(beatmap.Metadata.ArtistUnicode, beatmap.Metadata.Artist));
                            }),
                            new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Horizontal,
                                Margin = new MarginPadding { Top = 6 },
                                Spacing = new Vector2(4),
                                Children = new Drawable[]
                                {
                                    new StarRatingDisplay(new StarDifficulty(beatmap.StarRating, 0), StarRatingDisplaySize.Small)
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                    },
                                    new TruncatingSpriteText
                                    {
                                        Text = beatmap.DifficultyName,
                                        Font = OsuFont.Default.With(size: 16, weight: FontWeight.SemiBold),
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                    },
                                }
                            },
                        },
                    };
                }
            }
        }

        private partial class AvatarOverlay : CompositeDrawable
        {
            private readonly Container<SelectionAvatar> avatars;

            private Sample? userAddedSample;
            private double? lastSamplePlayback;

            public AvatarOverlay()
            {
                AutoSizeAxes = Axes.Both;

                InternalChild = avatars = new Container<SelectionAvatar>
                {
                    AutoSizeAxes = Axes.X,
                    Height = SelectionAvatar.AVATAR_SIZE,
                };

                Padding = new MarginPadding(5);
            }

            [BackgroundDependencyLoader]
            private void load(AudioManager audio)
            {
                userAddedSample = audio.Samples.Get(@"Multiplayer/player-ready");
            }

            public bool AddUser(APIUser user, bool isOwnUser)
            {
                if (avatars.Any(a => a.User.Id == user.Id))
                    return false;

                var avatar = new SelectionAvatar(user, isOwnUser);

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
