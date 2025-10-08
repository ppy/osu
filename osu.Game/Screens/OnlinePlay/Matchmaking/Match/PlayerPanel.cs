// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API;
using osu.Game.Online.Matchmaking.Events;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match
{
    /// <summary>
    /// A panel used throughout matchmaking to represent a user, including local information like their
    /// rank and high level statistics in the matchmaking system.
    /// </summary>
    public partial class PlayerPanel : UserPanel
    {
        public static readonly Vector2 SIZE_HORIZONTAL = new Vector2(250, 100);
        public static readonly Vector2 SIZE_VERTICAL = new Vector2(150, 200);
        private static readonly Vector2 avatar_size = new Vector2(80);

        public readonly MultiplayerRoomUser RoomUser;

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private OsuSpriteText rankText = null!;
        private OsuSpriteText scoreText = null!;

        private Drawable avatarPositionTarget = null!;
        private Drawable avatarJumpTarget = null!;
        private MatchmakingAvatar avatar = null!;
        private OsuSpriteText username = null!;

        private Container scaleContainer = null!;
        private Container mainContent = null!;

        public bool Horizontal
        {
            get => horizontal;
            set
            {
                horizontal = value;
                if (IsLoaded)
                    updateLayout(false);
            }
        }

        private bool horizontal;

        public PlayerPanel(MultiplayerRoomUser user)
            : base(user.User!)
        {
            RoomUser = user;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Masking = true;
            CornerRadius = 10;
            CornerExponent = 10;

            Add(scaleContainer = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Child = mainContent = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Children = new[]
                    {
                        avatarPositionTarget = new Container
                        {
                            Origin = Anchor.Centre,
                            Size = avatar_size,
                            Child = avatarJumpTarget = new Container
                            {
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.BottomCentre,
                                RelativeSizeAxes = Axes.Both,
                                Child = avatar = new MatchmakingAvatar(User, isOwnUser: User.Id == api.LocalUser.Value.Id)
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Size = Vector2.One
                                }
                            }
                        },
                        rankText = new OsuSpriteText
                        {
                            Anchor = Anchor.BottomRight,
                            Origin = Anchor.BottomCentre,
                            Blending = BlendingParameters.Additive,
                            Margin = new MarginPadding(4),
                            Font = OsuFont.Style.Title.With(size: 70),
                        },
                        username = new OsuSpriteText
                        {
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Text = User.Username,
                            Font = OsuFont.Style.Heading1,
                        },
                        scoreText = new OsuSpriteText
                        {
                            Margin = new MarginPadding(10),
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Font = OsuFont.Style.Heading2,
                            Text = "0 pts"
                        }
                    }
                }
            });
        }

        protected override Drawable CreateLayout() => Empty();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateLayout(true);

            client.MatchRoomStateChanged += onRoomStateChanged;
            client.MatchEvent += onMatchEvent;

            onRoomStateChanged(client.Room!.MatchState);

            avatar.ScaleTo(0)
                  .ScaleTo(1, 500, Easing.OutElasticHalf)
                  .FadeIn(200);

            rankText.Hide();
            scoreText.Hide();
            username.Hide();

            using (BeginDelayedSequence(100))
            {
                username.FadeInFromZero(600);

                using (BeginDelayedSequence(100))
                {
                    scoreText.FadeInFromZero(600);

                    using (BeginDelayedSequence(100))
                    {
                        rankText.FadeTo(0.6f, 600);
                    }
                }
            }
        }

        private Vector2 avatarPosition => horizontal ? new Vector2(50) : new Vector2(75, 50);

        private void updateLayout(bool instant)
        {
            double duration = instant ? 0 : 1000;

            avatarPositionTarget.MoveTo(avatarPosition, duration, Easing.OutPow10);
            this.ResizeTo(horizontal ? SIZE_HORIZONTAL : SIZE_VERTICAL, duration, Easing.OutPow10);

            rankText.MoveTo(horizontal ? new Vector2(-40, -10) : new Vector2(-70, 0), duration, Easing.OutPow10);
            username.MoveTo(horizontal ? new Vector2(0, -46) : new Vector2(0, -86), duration, Easing.OutPow10);
            scoreText.MoveTo(horizontal ? new Vector2(0, -16) : new Vector2(0, -56), duration, Easing.OutPow10);
        }

        protected override bool OnHover(HoverEvent e)
        {
            scaleContainer.ScaleTo(1.03f, 750, Easing.OutPow10);
            mainContent.ScaleTo(1.03f, 750, Easing.OutPow10);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            scaleContainer.ScaleTo(1f, 750, Easing.OutPow10);
            mainContent.ScaleTo(1, 750, Easing.OutPow10);

            mainContent.MoveTo(Vector2.Zero, 1250, Easing.OutPow10);
            avatarPositionTarget.MoveTo(avatarPosition, 1250, Easing.OutPow10);
            base.OnHoverLost(e);
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            var offset = (avatarPositionTarget.ToLocalSpace(e.ScreenSpaceMousePosition) - avatarPositionTarget.DrawSize / 2) * 0.02f;

            mainContent.MoveTo(offset * 0.5f, 1000, Easing.OutPow10);
            avatarPositionTarget.MoveTo(avatarPosition + offset, 400, Easing.OutPow10);
            return base.OnMouseMove(e);
        }

        private void onRoomStateChanged(MatchRoomState? state) => Scheduler.Add(() =>
        {
            if (state is not MatchmakingRoomState matchmakingState)
                return;

            if (!matchmakingState.Users.UserDictionary.TryGetValue(User.Id, out MatchmakingUser? userScore))
                return;

            rankText.Text = $"#{userScore.Placement}";
            scoreText.Text = $"{userScore.Points} pts";
        });

        private void onMatchEvent(MatchServerEvent e)
        {
            switch (e)
            {
                case MatchmakingAvatarActionEvent action:
                    if (action.UserId != RoomUser.UserID)
                        break;

                    switch (action.Action)
                    {
                        case MatchmakingAvatarAction.Jump:
                            avatarJumpTarget.MoveToY(-10, 200, Easing.Out)
                                            .Then().MoveToY(0, 200, Easing.In);
                            avatarJumpTarget.ScaleTo(new Vector2(1, 1.05f), 200, Easing.Out)
                                            .Then().ScaleTo(new Vector2(1, 0.95f), 200, Easing.In)
                                            .Then().ScaleTo(Vector2.One, 800, Easing.OutElastic);
                            break;
                    }

                    break;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
            {
                client.MatchRoomStateChanged -= onRoomStateChanged;
                client.MatchEvent -= onMatchEvent;
            }
        }
    }
}
