// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match.BeatmapSelect
{
    public partial class SelectionPanel : Container
    {
        public static readonly Vector2 SIZE = new Vector2(300, 70);

        private const float corner_radius = 6;
        private const float border_width = 3;

        public readonly MultiplayerPlaylistItem Item;

        private readonly Container scaleContainer;
        private readonly BeatmapPanel beatmapPanel;
        private readonly AvatarOverlay selectionOverlay;
        private readonly Container border;
        private readonly Box flash;

        public bool AllowSelection;

        public Action<MultiplayerPlaylistItem>? Action;

        [Resolved]
        private BeatmapLookupCache beatmapLookupCache { get; set; } = null!;

        public override bool PropagatePositionalInputSubTree => AllowSelection;

        public SelectionPanel(MultiplayerPlaylistItem item)
        {
            Item = item;
            Size = SIZE;

            InternalChildren = new Drawable[]
            {
                scaleContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding(-border_width),
                            Child = border = new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Masking = true,
                                CornerRadius = corner_radius + border_width,
                                Alpha = 0,
                                Child = new Box { RelativeSizeAxes = Axes.Both },
                            }
                        },
                        beatmapPanel = new BeatmapPanel
                        {
                            RelativeSizeAxes = Axes.Both,
                            OverlayLayer =
                            {
                                Children = new[]
                                {
                                    flash = new Box
                                    {
                                        Blending = BlendingParameters.Additive,
                                        RelativeSizeAxes = Axes.Both,
                                        Alpha = 0,
                                    },
                                }
                            }
                        },
                        selectionOverlay = new AvatarOverlay
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding { Horizontal = 10 },
                            Origin = Anchor.CentreLeft,
                        },
                    }
                },
                new HoverClickSounds(),
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmapLookupCache.GetBeatmapAsync(Item.BeatmapID).ContinueWith(b => Schedule(() =>
            {
                var beatmap = b.GetResultSafely()!;

                beatmap.StarRating = Item.StarRating;

                beatmapPanel.Beatmap = beatmap;
            }));
        }

        public bool AddUser(APIUser user, bool isOwnUser = false) => selectionOverlay.AddUser(user, isOwnUser);

        public bool RemoveUser(int userId) => selectionOverlay.RemoveUser(userId);

        public bool RemoveUser(APIUser user) => RemoveUser(user.Id);

        protected override bool OnHover(HoverEvent e)
        {
            flash.FadeTo(0.2f, 50)
                 .Then()
                 .FadeTo(0.1f, 300);

            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);

            flash.FadeOut(200);
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

            flash.FadeTo(0.5f, 50)
                 .Then()
                 .FadeTo(0.1f, 400);

            return true;
        }

        public void ShowBorder() => border.Show();

        public void HideBorder() => border.Hide();

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
        private partial class BeatmapPanel : CompositeDrawable
        {
            public readonly Container OverlayLayer = new Container { RelativeSizeAxes = Axes.Both };

            public APIBeatmap? Beatmap
            {
                get => beatmap;
                set
                {
                    if (beatmap?.OnlineID == value?.OnlineID)
                        return;

                    beatmap = value;

                    if (IsLoaded)
                        updateContent();
                }
            }

            private APIBeatmap? beatmap;

            private Container content = null!;
            private UpdateableOnlineBeatmapSetCover cover = null!;

            public BeatmapPanel(APIBeatmap? beatmap = null)
            {
                this.beatmap = beatmap;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                Masking = true;
                CornerRadius = 6;

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
                    OverlayLayer,
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

                cover.OnlineInfo = beatmap?.BeatmapSet;

                if (beatmap != null)
                {
                    var panelContent = new BeatmapPanelContent(beatmap)
                    {
                        RelativeSizeAxes = Axes.Both,
                    };

                    content.Add(panelContent);

                    panelContent.FadeInFromZero(300);
                }
            }

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
            private readonly Dictionary<int, SelectionAvatar> avatars = new Dictionary<int, SelectionAvatar>();

            private readonly Container<SelectionAvatar> avatarContainer;

            private Sample? userAddedSample;
            private double? lastSamplePlayback;

            public new Axes AutoSizeAxes
            {
                get => base.AutoSizeAxes;
                set => base.AutoSizeAxes = value;
            }

            public new MarginPadding Padding
            {
                get => base.Padding;
                set => base.Padding = value;
            }

            public AvatarOverlay()
            {
                InternalChild = avatarContainer = new Container<SelectionAvatar>();
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                avatarContainer.AutoSizeAxes = AutoSizeAxes;
                avatarContainer.RelativeSizeAxes = RelativeSizeAxes;
            }

            [BackgroundDependencyLoader]
            private void load(AudioManager audio)
            {
                userAddedSample = audio.Samples.Get(@"Multiplayer/player-ready");
            }

            public bool AddUser(APIUser user, bool isOwnUser)
            {
                if (avatars.ContainsKey(user.Id))
                    return false;

                var avatar = new SelectionAvatar(user, isOwnUser)
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                };

                avatarContainer.Add(avatars[user.Id] = avatar);

                if (lastSamplePlayback == null || Time.Current - lastSamplePlayback > OsuGameBase.SAMPLE_DEBOUNCE_TIME)
                {
                    userAddedSample?.Play();
                    lastSamplePlayback = Time.Current;
                }

                updateLayout();

                avatar.FinishTransforms();

                return true;
            }

            public bool RemoveUser(int id)
            {
                if (!avatars.Remove(id, out var avatar))
                    return false;

                avatar.PopOutAndExpire();
                avatarContainer.ChangeChildDepth(avatar, float.MaxValue);

                updateLayout();

                return true;
            }

            private void updateLayout()
            {
                const double stagger = 30;
                const float spacing = 4;

                double delay = 0;
                float x = 0;

                for (int i = avatarContainer.Count - 1; i >= 0; i--)
                {
                    var avatar = avatarContainer[i];

                    if (avatar.Expired)
                        continue;

                    avatar.Delay(delay).MoveToX(x, 500, Easing.OutElasticQuarter);

                    x -= avatar.LayoutSize.X + spacing;

                    delay += stagger;
                }
            }

            public partial class SelectionAvatar : CompositeDrawable
            {
                public bool Expired { get; private set; }

                private readonly Container content;

                public SelectionAvatar(APIUser user, bool isOwnUser)
                {
                    Size = new Vector2(30);

                    InternalChildren = new Drawable[]
                    {
                        content = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Child = new MatchmakingAvatar(user, isOwnUser)
                        }
                    };
                }

                protected override void LoadComplete()
                {
                    base.LoadComplete();

                    content.ScaleTo(0)
                           .ScaleTo(1, 500, Easing.OutElasticHalf)
                           .FadeIn(200);
                }

                public void PopOutAndExpire()
                {
                    content.ScaleTo(0, 400, Easing.OutExpo);

                    this.FadeOut(100).Expire();
                    Expired = true;
                }
            }
        }
    }
}
