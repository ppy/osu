// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Online.Matchmaking.Events;
using osu.Game.Online.Metadata;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Screens.OnlinePlay.Matchmaking.Match.Results;
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

        /// <summary>
        /// Perform an action in addition to showing the user's profile.
        /// This should be used to perform auxiliary tasks and not as a primary action for clicking a user panel (to maintain a consistent UX).
        /// </summary>
        public new Action? Action;

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
        private OverlayColourProvider? colourProvider { get; set; }

        [Resolved]
        private IPerformFromScreenRunner? performer { get; set; }

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private MultiplayerClient? multiplayerClient { get; set; }

        [Resolved]
        private MetadataClient? metadataClient { get; set; }

        public readonly APIUser User;
        private readonly Action viewProfile;

        private OsuSpriteText rankText = null!;
        private OsuSpriteText scoreText = null!;

        private Drawable avatarPositionTarget = null!;
        private Drawable avatarJumpTarget = null!;
        private Drawable avatar = null!;
        private OsuSpriteText username = null!;

        private Container mainContent = null!;

        private Box solidBackgroundLayer = null!;
        private Drawable background = null!;

        private OsuSpriteText quitText = null!;
        private BufferedContainer backgroundQuitTarget = null!;
        private BufferedContainer avatarQuitTarget = null!;

        private Box downloadProgressBar = null!;

        private PlayerPanelDisplayMode displayMode = PlayerPanelDisplayMode.Horizontal;
        private bool hasQuit;

        private enum InteractionSampleType
        {
            PlayerJump,
            PlayerReJump,
            OtherPlayerJump,
        }

        private Dictionary<InteractionSampleType, Sample?> interactionSamples = new Dictionary<InteractionSampleType, Sample?>();
        private readonly Dictionary<InteractionSampleType, SampleChannel?> interactionSampleChannels = new Dictionary<InteractionSampleType, SampleChannel?>();
        private double samplePitch;
        private double? lastSamplePlayback;

        public PlayerPanel(MultiplayerRoomUser user)
            : base(HoverSampleSet.Button)
        {
            ArgumentNullException.ThrowIfNull(user.User);

            User = user.User;
            RoomUser = user;

            base.Action = viewProfile = () =>
            {
                Action?.Invoke();
                profileOverlay?.ShowUser(User);
            };
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            Content.Masking = true;
            Content.CornerRadius = 10;
            Content.CornerExponent = 10;
            Content.Anchor = Anchor.Centre;
            Content.Origin = Anchor.Centre;

            Child = backgroundQuitTarget = new BufferedContainer
            {
                FrameBufferScale = new Vector2(1.5f),
                RelativeSizeAxes = Axes.Both,
                Children = new[]
                {
                    solidBackgroundLayer = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider?.Background5 ?? colours.Gray1
                    },
                    background = new UserCoverBackground
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Colour = colours.Gray7,
                        User = User
                    },
                    new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            mainContent = new Container
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Children = new[]
                                {
                                    quitText = new OsuSpriteText
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Text = "QUIT",
                                        Font = OsuFont.Default.With(weight: "Bold", size: 70),
                                        Rotation = -22.5f,
                                        Colour = OsuColour.Gray(0.3f),
                                        Blending = BlendingParameters.Additive
                                    },
                                    avatarPositionTarget = new Container
                                    {
                                        Origin = Anchor.Centre,
                                        Size = avatar_size,
                                        Child = avatarJumpTarget = new Container
                                        {
                                            Anchor = Anchor.BottomCentre,
                                            Origin = Anchor.BottomCentre,
                                            RelativeSizeAxes = Axes.Both,
                                            Child = avatar = new Container
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                RelativeSizeAxes = Axes.Both,
                                                // Needs to be re-buffered as the avatar is proxied outside of the parent buffered container.
                                                Child = avatarQuitTarget = new BufferedContainer
                                                {
                                                    FrameBufferScale = new Vector2(1.5f),
                                                    RelativeSizeAxes = Axes.Both,
                                                    Child = new MatchmakingAvatar(User, isOwnUser: User.Id == api.LocalUser.Value.Id)
                                                    {
                                                        Anchor = Anchor.Centre,
                                                        Origin = Anchor.Centre,
                                                        RelativeSizeAxes = Axes.Both,
                                                        Size = Vector2.One
                                                    }
                                                }
                                            },
                                        }
                                    },
                                    rankText = new OsuSpriteText
                                    {
                                        Alpha = 0,
                                        Anchor = Anchor.BottomRight,
                                        Origin = Anchor.BottomCentre,
                                        Blending = BlendingParameters.Additive,
                                        Margin = new MarginPadding(4),
                                        Text = "-",
                                        Font = OsuFont.Style.Title.With(size: 55),
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
                            },
                            downloadProgressBar = new Box
                            {
                                Anchor = Anchor.BottomLeft,
                                Origin = Anchor.BottomLeft,
                                RelativeSizeAxes = Axes.X,
                                Size = new Vector2(0, 4),
                                Colour = colourProvider?.Content2 ?? colours.Gray3
                            }
                        }
                    }
                }
            };

            // Allow avatar to exist outside of masking for when it jumps around and stuff.
            AddInternal(avatar.CreateProxy());

            interactionSamples = new Dictionary<InteractionSampleType, Sample?>
            {
                { InteractionSampleType.PlayerJump, audio.Samples.Get(@"Multiplayer/Matchmaking/player-jump") },
                { InteractionSampleType.PlayerReJump, audio.Samples.Get(@"Multiplayer/Matchmaking/player-rejump") },
                { InteractionSampleType.OtherPlayerJump, audio.Samples.Get(@"Multiplayer/Matchmaking/player-jump-other") }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateLayout(true);

            client.MatchRoomStateChanged += onRoomStateChanged;
            client.MatchEvent += onMatchEvent;
            client.BeatmapAvailabilityChanged += onBeatmapAvailabilityChanged;

            onRoomStateChanged(client.Room!.MatchState);

            avatar.ScaleTo(0)
                  .ScaleTo(1, 500, Easing.OutElasticHalf)
                  .FadeIn(200);

            // pick a random pitch to be used by the player for duration of this session
            samplePitch = 0.75f + RNG.NextDouble(0f, 0.75f);
        }

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

        public bool HasQuit
        {
            get => hasQuit;
            set
            {
                hasQuit = value;
                if (IsLoaded)
                    updateLayout(false);
            }
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

                    background.FadeOut(200, Easing.OutQuint);
                    solidBackgroundLayer.FadeOut(200, Easing.OutQuint);

                    this.ResizeTo(avatar_size, duration, Easing.OutPow10);
                    break;

                case PlayerPanelDisplayMode.Horizontal:
                case PlayerPanelDisplayMode.Vertical:
                    background.FadeIn(200);
                    solidBackgroundLayer.FadeIn(200);

                    using (BeginDelayedSequence(100))
                    {
                        username.FadeIn(600);

                        using (BeginDelayedSequence(100))
                        {
                            scoreText.FadeIn(600);

                            using (BeginDelayedSequence(100))
                            {
                                rankText.FadeTo(1, 600);
                            }
                        }
                    }

                    this.ResizeTo(horizontal ? size_horizontal : size_vertical, duration, Easing.OutPow10);

                    rankText.MoveTo(horizontal ? new Vector2(-40, -20) : new Vector2(-70, 0), duration, Easing.OutPow10);
                    username.MoveTo(horizontal ? new Vector2(0, -46) : new Vector2(0, -86), duration, Easing.OutPow10);
                    scoreText.MoveTo(horizontal ? new Vector2(0, -16) : new Vector2(0, -56), duration, Easing.OutPow10);
                    quitText.MoveTo(horizontal ? new Vector2(40, 0) : new Vector2(0, 40), duration, Easing.OutPow10);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            // quit text doesn't fit on avataronly mode.
            if (HasQuit && displayMode != PlayerPanelDisplayMode.AvatarOnly)
                quitText.FadeIn(duration, Easing.OutPow10);
            else
                quitText.FadeOut(duration, Easing.OutPow10);

            if (HasQuit)
            {
                backgroundQuitTarget.GrayscaleTo(1, duration, Easing.OutPow10);
                avatarQuitTarget.GrayscaleTo(1, duration, Easing.OutPow10);
            }
            else
            {
                backgroundQuitTarget.GrayscaleTo(0, duration, Easing.OutPow10);
                avatarQuitTarget.GrayscaleTo(0, duration, Easing.OutPow10);
            }
        }

        protected override void Update()
        {
            base.Update();

            // Not sure why this is required but it is.
            avatarQuitTarget.Alpha = Alpha;
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

            if (userScore.Placement == null)
                return;

            rankText.Text = userScore.Placement.Value.Ordinalize(CultureInfo.CurrentCulture);
            rankText.FadeColour(SubScreenResults.ColourForPlacement(userScore.Placement.Value));
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

                            // only play jump sample if panel is visible
                            if (Alpha > 0)
                                playJumpSample(isConsecutive);

                            break;
                    }

                    break;
            }
        }

        private void onBeatmapAvailabilityChanged(MultiplayerRoomUser user, BeatmapAvailability availability) => Scheduler.Add(() =>
        {
            if (availability.State == DownloadState.Downloading)
                downloadProgressBar.FadeIn(200, Easing.OutPow10);
            else
                downloadProgressBar.FadeOut(200, Easing.OutPow10);

            downloadProgressBar.ResizeWidthTo(availability.DownloadProgress ?? 0, 200, Easing.OutPow10);
        });

        private void playJumpSample(bool rejumping)
        {
            bool isLocalUser = User.OnlineID == client.LocalUser?.UserID;

            if (isLocalUser)
                playInteractionSample(rejumping ? InteractionSampleType.PlayerReJump : InteractionSampleType.PlayerJump);
            else
                playInteractionSample(InteractionSampleType.OtherPlayerJump);
        }

        private void playInteractionSample(InteractionSampleType sampleType)
        {
            bool enoughTimePassedSinceLastPlayback = lastSamplePlayback == null || Time.Current - lastSamplePlayback.Value >= OsuGameBase.SAMPLE_DEBOUNCE_TIME;
            if (!enoughTimePassedSinceLastPlayback)
                return;

            Sample? targetSample = interactionSamples[sampleType];
            SampleChannel? targetChannel = interactionSampleChannels.GetValueOrDefault(sampleType);

            targetChannel?.Stop();
            targetChannel = targetSample?.GetChannel();

            if (targetChannel == null)
                return;

            float horizontalPos = BoundingBox.Centre.X / Parent!.ToLocalSpace(Parent!.ScreenSpaceDrawQuad).Width;
            // rescale balance from 0..1 to -1..1
            float balance = -1f + horizontalPos * 2f;

            targetChannel.Frequency.Value = samplePitch;
            targetChannel.Balance.Value = balance * OsuGameBase.SFX_STEREO_STRENGTH;
            targetChannel.Play();

            interactionSampleChannels[sampleType] = targetChannel;

            lastSamplePlayback = Time.Current;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
            {
                client.MatchRoomStateChanged -= onRoomStateChanged;
                client.MatchEvent -= onMatchEvent;
                client.BeatmapAvailabilityChanged -= onBeatmapAvailabilityChanged;
            }
        }

        public MenuItem[] ContextMenuItems
        {
            get
            {
                List<MenuItem> items = new List<MenuItem>
                {
                    new OsuMenuItem(ContextMenuStrings.ViewProfile, MenuItemType.Highlighted, viewProfile)
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
                bool isUserBlocked() => api.LocalUserState.Blocks.Any(b => b.TargetID == User.OnlineID);
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
