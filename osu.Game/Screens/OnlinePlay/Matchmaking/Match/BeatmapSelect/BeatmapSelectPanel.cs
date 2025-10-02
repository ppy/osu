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
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Database;
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
        public static readonly Vector2 SIZE = new Vector2(BeatmapCard.WIDTH, BeatmapCardNormal.HEIGHT);

        public bool AllowSelection { get; set; }

        public readonly MultiplayerPlaylistItem Item;

        public Action<MultiplayerPlaylistItem>? Action { private get; init; }

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
                        CornerRadius = BeatmapCard.CORNER_RADIUS,
                        CornerExponent = 10,
                        RelativeSizeAxes = Axes.Both,
                        Children = new[]
                        {
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
                        CornerRadius = BeatmapCard.CORNER_RADIUS,
                        CornerExponent = 10,
                        Blending = BlendingParameters.Additive,
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

                mainContent.Add(new BeatmapCardMatchmaking(beatmap)
                {
                    Depth = float.MaxValue,
                    Action = () => Action?.Invoke(Item),
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
            lighting.FadeTo(0.5f, 50)
                    .Then()
                    .FadeTo(0.1f, 400);

            // pass through to let the beatmap card handle actual click.
            return false;
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
