// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Matchmaking;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking
{
    public partial class MatchmakingQueueBanner : CompositeDrawable
    {
        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [Resolved]
        private IPerformFromScreenRunner performer { get; set; } = null!;

        private readonly Bindable<bool> canAcceptInvitation = new Bindable<bool>();

        private Drawable statusContainer = null!;
        private Drawable invitationButtons = null!;
        private SpriteText statusText = null!;
        private Drawable background = null!;

        private MatchmakingQueueStatus? lastStatus;

        public MatchmakingQueueBanner()
        {
            AutoSizeAxes = Axes.Both;
            AlwaysPresent = true;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new Container
            {
                AutoSizeAxes = Axes.X,
                AutoSizeDuration = 200,
                AutoSizeEasing = Easing.OutQuint,
                Height = 36,
                Masking = true,
                Children = new[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Yellow
                    },
                    statusContainer = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Children = new[]
                        {
                            statusText = new OsuSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Margin = new MarginPadding(10),
                                Colour = Color4.Black,
                            },
                            invitationButtons = new FillFlowContainer
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Children = new[]
                                {
                                    new IconButton
                                    {
                                        Icon = FontAwesome.Solid.Check,
                                        IconColour = Color4.Green,
                                        Action = acceptInvitation,
                                        Enabled = { BindTarget = canAcceptInvitation }
                                    },
                                    new IconButton
                                    {
                                        Icon = FontAwesome.Solid.Times,
                                        IconColour = Color4.Red,
                                        Action = declineInvitation,
                                        Enabled = { BindTarget = canAcceptInvitation }
                                    }
                                }
                            }
                        }
                    },
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            canAcceptInvitation.BindValueChanged(onCanAcceptInvitationChanged, true);

            client.MatchmakingQueueJoined += onMatchmakingQueueJoined;
            client.MatchmakingQueueLeft += onMatchmakingQueueLeft;
            client.MatchmakingRoomInvited += onMatchmakingRoomInvited;
            client.MatchmakingRoomReady += onMatchmakingRoomReady;
            client.MatchmakingQueueStatusChanged += onMatchmakingQueueStatusChanged;

            Hide();
            FinishTransforms();
        }

        private void acceptInvitation()
        {
            client.MatchmakingAcceptInvitation().FireAndForget();
            canAcceptInvitation.Value = false;
        }

        private void declineInvitation()
        {
            client.MatchmakingDeclineInvitation().FireAndForget();
            canAcceptInvitation.Value = false;
        }

        private void onCanAcceptInvitationChanged(ValueChangedEvent<bool> e)
        {
            if (e.NewValue)
                invitationButtons.Show();
            else
                invitationButtons.Hide();
        }

        private void onMatchmakingQueueJoined() => Scheduler.Add(Show);

        private void onMatchmakingQueueLeft() => Scheduler.Add(() =>
        {
            // When joining the match, the final hide is handled by the room.
            if (lastStatus is MatchmakingQueueStatus.JoiningMatch)
                return;

            Hide();
        });

        private void onMatchmakingRoomInvited() => Scheduler.Add(() =>
        {
            canAcceptInvitation.Value = true;
        });

        private void onMatchmakingRoomReady(long roomId) => Scheduler.Add(() =>
        {
            // Perform all actions from the menu, exiting any existing multiplayer/matchmaking screen.
            performer.PerformFromScreen(_ =>
            {
                // Now that we have a fresh slate, we can join the room.
                client.JoinRoom(new Room { RoomID = roomId })
                      .FireAndForget(() => Schedule(() =>
                      {
                          performer.PerformFromScreen(screen => screen.Push(new MatchmakingScreen(client.Room!)));
                      }), _ => Hide());
            });
        });

        private void onMatchmakingQueueStatusChanged(MatchmakingQueueStatus status) => Scheduler.Add(() =>
        {
            lastStatus = status;

            switch (status)
            {
                case MatchmakingQueueStatus.Searching:
                    background.Colour = Color4.Yellow;
                    statusText.Text = "finding a match...";

                    // For the case that user is returned to the queue before clicking one of the acceptance buttons.
                    canAcceptInvitation.Value = false;
                    break;

                case MatchmakingQueueStatus.MatchFound:
                    background.Colour = Color4.LightBlue;
                    statusText.Text = "match ready!";
                    break;

                case MatchmakingQueueStatus.JoiningMatch:
                    background.Colour = Color4.LightGreen;
                    statusText.Text = "joining the match...";

                    // This state is normally set by one of the button presses, but it's slightly complicated to emulate in tests.
                    canAcceptInvitation.Value = false;
                    break;
            }
        });

        public override void Show()
        {
            statusContainer.BypassAutoSizeAxes = Axes.None;
        }

        public override void Hide()
        {
            statusContainer.BypassAutoSizeAxes = Axes.Both;
            canAcceptInvitation.Value = false;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
                client.MatchmakingQueueStatusChanged -= onMatchmakingQueueStatusChanged;
        }
    }
}
