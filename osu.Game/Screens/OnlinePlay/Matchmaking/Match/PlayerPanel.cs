// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Online.Matchmaking.Events;
using osu.Game.Online.Metadata;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Screens.Play;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match
{
    /// <summary>
    /// A panel used throughout matchmaking to represent a user, including local information like their
    /// rank and high level statistics in the matchmaking system.
    /// </summary>
    public partial class PlayerPanel : OsuClickableContainer, IHasContextMenu
    {
        private static readonly Vector2 size_horizontal = new Vector2(250, 100);
        private static readonly Vector2 size_vertical = new Vector2(150, 200);
        private static readonly Vector2 avatar_size = new Vector2(80);

        public readonly MultiplayerRoomUser RoomUser;

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private UserProfileOverlay? profileOverlay { get; set; }

        [Resolved]
        private ChannelManager? channelManager { get; set; }

        [Resolved]
        private ChatOverlay? chatOverlay { get; set; }

        [Resolved]
        private IDialogOverlay? dialogOverlay { get; set; }

        [Resolved]
        protected OverlayColourProvider? ColourProvider { get; private set; }

        [Resolved]
        private IPerformFromScreenRunner? performer { get; set; }

        [Resolved]
        protected OsuColour Colours { get; private set; } = null!;

        [Resolved]
        private MultiplayerClient? multiplayerClient { get; set; }

        [Resolved]
        private MetadataClient? metadataClient { get; set; }

        private OsuSpriteText rankText = null!;
        private OsuSpriteText scoreText = null!;

        private Drawable avatarPositionTarget = null!;
        private Drawable avatarJumpTarget = null!;
        private MatchmakingAvatar avatar = null!;
        private OsuSpriteText username = null!;

        private Container mainContent = null!;

        private PlayerPanelDisplayMode displayMode = PlayerPanelDisplayMode.Horizontal;

        public PlayerPanelDisplayMode DisplayMode
        {
            get => displayMode;
            set
            {
                displayMode = value;
                if (IsLoaded)
                    updateLayout(false);
            }
        }

        public readonly APIUser User;

        /// <summary>
        /// Perform an action in addition to showing the user's profile.
        /// This should be used to perform auxiliary tasks and not as a primary action for clicking a user panel (to maintain a consistent UX).
        /// </summary>
        public new Action? Action;

        protected Action ViewProfile { get; private set; } = null!;

        public Box SolidBackgroundLayer { get; private set; } = null!;

        protected Drawable? Background { get; private set; }

        public PlayerPanel(MultiplayerRoomUser user)
            : base(HoverSampleSet.Button)
        {
            ArgumentNullException.ThrowIfNull(user.User);

            User = user.User;
            RoomUser = user;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(SolidBackgroundLayer = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = ColourProvider?.Background5 ?? Colours.Gray1
            });

            Background = new UserCoverBackground
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                User = User
            };
            if (Background != null)
                Add(Background);

            base.Action = ViewProfile = () =>
            {
                Action?.Invoke();
                profileOverlay?.ShowUser(User);
            };

            Content.Masking = true;
            Content.CornerRadius = 10;
            Content.CornerExponent = 10;
            Content.Anchor = Anchor.Centre;
            Content.Origin = Anchor.Centre;

            Add(new Container
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
                            Alpha = 0,
                            Anchor = Anchor.BottomRight,
                            Origin = Anchor.BottomCentre,
                            Blending = BlendingParameters.Additive,
                            Margin = new MarginPadding(4),
                            Font = OsuFont.Style.Title.With(size: 70),
                        },
                        username = new OsuSpriteText
                        {
                            Alpha = 0,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Text = User.Username,
                            Font = OsuFont.Style.Heading1,
                        },
                        scoreText = new OsuSpriteText
                        {
                            Alpha = 0,
                            Margin = new MarginPadding(10),
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Font = OsuFont.Style.Heading2,
                            Text = "0 pts"
                        }
                    }
                }
            });

            // Allow avatar to exist outside of masking for when it jumps around and stuff.
            AddInternal(avatar.CreateProxy());
        }

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
        }

        private bool horizontal => displayMode == PlayerPanelDisplayMode.Horizontal;

        private Vector2 avatarPosition
        {
            get
            {
                switch (displayMode)
                {
                    case PlayerPanelDisplayMode.AvatarOnly:
                        return avatar_size / 2;

                    case PlayerPanelDisplayMode.Horizontal:
                        return new Vector2(50);

                    case PlayerPanelDisplayMode.Vertical:
                        return new Vector2(75, 50);

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void updateLayout(bool instant)
        {
            double duration = instant ? 0 : 1000;

            avatarPositionTarget.MoveTo(avatarPosition, duration, Easing.OutPow10);

            switch (displayMode)
            {
                case PlayerPanelDisplayMode.AvatarOnly:
                    rankText.Hide();
                    scoreText.Hide();
                    username.Hide();

                    Background.FadeOut(200, Easing.OutQuint);
                    SolidBackgroundLayer.FadeOut(200, Easing.OutQuint);

                    this.ResizeTo(avatar_size, duration, Easing.OutPow10);
                    break;

                case PlayerPanelDisplayMode.Horizontal:
                case PlayerPanelDisplayMode.Vertical:
                    Background.FadeIn(200);
                    SolidBackgroundLayer.FadeIn(200);

                    using (BeginDelayedSequence(100))
                    {
                        username.FadeIn(600);

                        using (BeginDelayedSequence(100))
                        {
                            scoreText.FadeIn(600);

                            using (BeginDelayedSequence(100))
                            {
                                rankText.FadeTo(0.6f, 600);
                            }
                        }
                    }

                    this.ResizeTo(horizontal ? size_horizontal : size_vertical, duration, Easing.OutPow10);

                    rankText.MoveTo(horizontal ? new Vector2(-40, -10) : new Vector2(-70, 0), duration, Easing.OutPow10);
                    username.MoveTo(horizontal ? new Vector2(0, -46) : new Vector2(0, -86), duration, Easing.OutPow10);
                    scoreText.MoveTo(horizontal ? new Vector2(0, -16) : new Vector2(0, -56), duration, Easing.OutPow10);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override bool OnHover(HoverEvent e)
        {
            Content.ScaleTo(1.03f, 2000, Easing.OutPow10);
            mainContent.ScaleTo(1.03f, 2000, Easing.OutPow10);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            Content.ScaleTo(1f, 750, Easing.OutPow10);
            mainContent.ScaleTo(1, 750, Easing.OutPow10);

            mainContent.MoveTo(Vector2.Zero, 1250, Easing.OutPow10);
            avatarPositionTarget.MoveTo(avatarPosition, 1250, Easing.OutPow10);
            base.OnHoverLost(e);
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            var offset = (avatarPositionTarget.ToLocalSpace(e.ScreenSpaceMousePosition) - avatarPositionTarget.DrawSize / 2) * 0.02f;

            mainContent.MoveTo(offset * 0.5f, 2000, Easing.OutPow10);
            avatarPositionTarget.MoveTo(avatarPosition + offset, 2000, Easing.OutPow10);
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

        private int consecutiveJumps;

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
                            var movement = avatarJumpTarget.Delay(0);
                            var scale = avatarJumpTarget.Delay(0);

                            // only increase height if the user jumps again while in a "jumped" state.
                            // this avoids building up large jumps from very quick spam, and adds a timing game.
                            bool isConsecutive = avatarJumpTarget.Y < 0;

                            if (isConsecutive)
                            {
                                consecutiveJumps++;

                                if (avatarJumpTarget.Y > 0)
                                    movement = movement.MoveToY(0);

                                movement = movement.MoveToY(5, 100, Easing.Out);
                                scale = scale.ScaleTo(new Vector2(1, 0.95f), 100, Easing.Out);
                            }
                            else
                            {
                                consecutiveJumps = 0;
                            }

                            float multiplier = 1 + 0.3f * Math.Min(10, consecutiveJumps);

                            movement.Then().MoveToY(-10 * multiplier, 200, Easing.Out)
                                    .Then().MoveToY(0, 200, Easing.In);

                            scale.Then().ScaleTo(new Vector2(1, 1.05f), 200, Easing.Out)
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

        public MenuItem[] ContextMenuItems
        {
            get
            {
                List<MenuItem> items = new List<MenuItem>
                {
                    new OsuMenuItem(ContextMenuStrings.ViewProfile, MenuItemType.Highlighted, ViewProfile)
                };

                if (User.Equals(api.LocalUser.Value))
                    return items.ToArray();

                items.Add(new OsuMenuItem(UsersStrings.CardSendMessage, MenuItemType.Standard, () =>
                {
                    channelManager?.OpenPrivateChannel(User);
                    chatOverlay?.Show();
                }));

                items.Add(!isUserBlocked()
                    ? new OsuMenuItem(UsersStrings.BlocksButtonBlock, MenuItemType.Destructive, () => dialogOverlay?.Push(ConfirmBlockActionDialog.Block(User)))
                    : new OsuMenuItem(UsersStrings.BlocksButtonUnblock, MenuItemType.Standard, () => dialogOverlay?.Push(ConfirmBlockActionDialog.Unblock(User))));

                if (isUserOnline())
                {
                    items.Add(new OsuMenuItem(ContextMenuStrings.SpectatePlayer, MenuItemType.Standard, () =>
                    {
                        if (isUserOnline())
                            performer?.PerformFromScreen(s => s.Push(new SoloSpectatorScreen(User)));
                    }));

                    if (canInviteUser())
                    {
                        items.Add(new OsuMenuItem(ContextMenuStrings.InvitePlayer, MenuItemType.Standard, () =>
                        {
                            if (canInviteUser())
                                multiplayerClient!.InvitePlayer(User.Id);
                        }));
                    }
                }

                return items.ToArray();

                bool isUserOnline() => metadataClient?.GetPresence(User.OnlineID) != null;
                bool canInviteUser() => isUserOnline() && multiplayerClient?.Room?.Users.All(u => u.UserID != User.Id) == true;
                bool isUserBlocked() => api.Blocks.Any(b => b.TargetID == User.OnlineID);
            }
        }
    }

    public enum PlayerPanelDisplayMode
    {
        AvatarOnly,
        Horizontal,
        Vertical
    }
}
