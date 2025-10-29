// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match
{
    /// <summary>
    /// A component which maintains the layout of the players in a matchmaking room.
    /// Can be controlled to display the panels in a certain location and in multiple styles.
    /// </summary>
    public partial class PlayerPanelOverlay : CompositeDrawable
    {
        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        private Container<PlayerPanel> panels = null!;
        private PlayerPanelCellContainer gridLayout = null!;
        private PlayerPanelCellContainer splitLayoutLeft = null!;
        private PlayerPanelCellContainer splitLayoutRight = null!;

        private PanelDisplayStyle displayStyle;
        private Drawable? displayArea;
        private bool isAnimatingToDisplayArea;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                gridLayout = new PlayerPanelCellContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Spacing = new Vector2(20),
                },
                splitLayoutLeft = new PlayerPanelCellContainer
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(5),
                },
                splitLayoutRight = new PlayerPanelCellContainer
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(5),
                },
                panels = new Container<PlayerPanel>
                {
                    RelativeSizeAxes = Axes.Both
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Set position/size so we don't initially animate.
            Position = getFinalPosition();
            Size = getFinalSize();

            client.MatchRoomStateChanged += onRoomStateChanged;
            client.UserJoined += onUserJoined;
            client.UserLeft += onUserLeft;

            if (client.Room != null)
            {
                onRoomStateChanged(client.Room.MatchState);
                foreach (var user in client.Room.Users)
                    onUserJoined(user);
            }

            updateDisplay();
        }

        public PanelDisplayStyle DisplayStyle
        {
            set
            {
                displayStyle = value;
                if (IsLoaded)
                    updateDisplay();
            }
        }

        public Drawable? DisplayArea
        {
            set
            {
                displayArea = value;
                isAnimatingToDisplayArea = true;
            }
        }

        private void onUserJoined(MultiplayerRoomUser user) => Scheduler.Add(() =>
        {
            panels.Add(new PlayerPanel(user)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Scale = new Vector2(0.8f)
            });

            updateDisplay();
        });

        private void onUserLeft(MultiplayerRoomUser user) => Scheduler.Add(() =>
        {
            panels.Single(p => p.RoomUser.Equals(user)).HasQuit = true;
            updateDisplay();
        });

        private void onRoomStateChanged(MatchRoomState? state) => Scheduler.Add(updateDisplay);

        private void updateDisplay()
        {
            gridLayout.ReleasePanels();
            splitLayoutLeft.ReleasePanels();
            splitLayoutRight.ReleasePanels();

            switch (displayStyle)
            {
                case PanelDisplayStyle.Grid:
                    foreach (var panel in panels)
                    {
                        panel.FadeTo(1, 200);
                        panel.DisplayMode = PlayerPanelDisplayMode.Vertical;
                    }

                    gridLayout.AcquirePanels(panels.ToArray());
                    break;

                case PanelDisplayStyle.Split:
                    foreach (var panel in panels)
                    {
                        panel.FadeTo(1, 200);
                        panel.DisplayMode = PlayerPanelDisplayMode.Horizontal;
                    }

                    int leftCount = (int)Math.Ceiling(panels.Count / 2f);

                    splitLayoutLeft.AcquirePanels(panels.Take(leftCount).ToArray());
                    splitLayoutRight.AcquirePanels(panels.Skip(leftCount).ToArray());
                    break;

                case PanelDisplayStyle.Hidden:
                    foreach (var panel in panels)
                        panel.FadeTo(0, 200);
                    return;
            }
        }

        protected override void Update()
        {
            base.Update();

            var targetPos = getFinalPosition();
            var targetSize = getFinalSize();

            double duration = isAnimatingToDisplayArea ? 60 : 0;

            if (Time.Elapsed > 0)
            {
                Position = new Vector2(
                    (float)Interpolation.DampContinuously(Position.X, targetPos.X, duration, Time.Elapsed),
                    (float)Interpolation.DampContinuously(Position.Y, targetPos.Y, duration, Time.Elapsed)
                );

                Size = new Vector2(
                    (float)Interpolation.DampContinuously(Size.X, targetSize.X, duration, Time.Elapsed),
                    (float)Interpolation.DampContinuously(Size.Y, targetSize.Y, duration, Time.Elapsed)
                );
            }

            // If we don't track the animating state, the animation will also occur when resizing the window.
            isAnimatingToDisplayArea &= !Precision.AlmostEquals(Size, targetSize, 0.5f);
        }

        private Vector2 getFinalPosition()
            => displayArea == null ? Vector2.Zero : Parent!.ToLocalSpace(displayArea.ScreenSpaceDrawQuad.TopLeft);

        private Vector2 getFinalSize()
            => displayArea == null ? Parent!.DrawSize : Parent!.ToLocalSpace(displayArea.ScreenSpaceDrawQuad.BottomRight) - Parent!.ToLocalSpace(displayArea.ScreenSpaceDrawQuad.TopLeft);

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
            {
                client.MatchRoomStateChanged -= onRoomStateChanged;
                client.UserJoined -= onUserJoined;
                client.UserLeft -= onUserLeft;
            }
        }

        private partial class PlayerPanelCellContainer : FillFlowContainer<PlayerPanelCell>
        {
            [Resolved]
            private MultiplayerClient client { get; set; } = null!;

            public void AcquirePanels(PlayerPanel[] panels)
            {
                while (Count < panels.Length)
                {
                    Add(new PlayerPanelCell
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    });
                }

                while (Count > panels.Length)
                    Remove(Children[^1], true);

                for (int i = 0; i < panels.Length; i++)
                {
                    // We'll invalidate the layout position to represent the new placements and the re-flow will happen in UpdateAfterChildren().
                    // But the cells expect their positions to be valid as they're updated, which won't be the case until the re-flow happens.
                    int i2 = i;
                    ScheduleAfterChildren(() => Children[i2].AcquirePanel(panels[i2]));

                    if (client.Room?.MatchState is not MatchmakingRoomState matchmakingState)
                        continue;

                    if (matchmakingState.Users.UserDictionary.TryGetValue(panels[i].User.Id, out MatchmakingUser? user) && user.Placement != null)
                        SetLayoutPosition(Children[i], user.Placement.Value);
                    else
                        SetLayoutPosition(Children[i], float.MaxValue);
                }
            }

            public void ReleasePanels()
            {
                // Matches the schedule in AcquirePanels.
                ScheduleAfterChildren(() =>
                {
                    foreach (var panel in Children)
                        panel.ReleasePanel();
                });
            }
        }

        private partial class PlayerPanelCell : Drawable
        {
            private PlayerPanel? panel;
            private bool isAnimating;

            public void AcquirePanel(PlayerPanel panel)
            {
                this.panel = panel;
                isAnimating = true;
            }

            public void ReleasePanel()
            {
                panel = null;
            }

            protected override void Update()
            {
                base.Update();

                if (panel?.Parent == null)
                    return;

                Size = panel.Size * panel.Scale;

                var targetPos = getFinalPosition();

                double duration = isAnimating ? 60 : 0;

                if (Time.Elapsed > 0)
                {
                    panel.Position = new Vector2(
                        (float)Interpolation.DampContinuously(panel.Position.X, targetPos.X, duration, Time.Elapsed),
                        (float)Interpolation.DampContinuously(panel.Position.Y, targetPos.Y, duration, Time.Elapsed)
                    );
                }

                // If we don't track the animating state, the animation will also occur when resizing the window.
                isAnimating &= !Precision.AlmostEquals(panel.Position, targetPos, 0.5f);

                Vector2 getFinalPosition()
                    => panel.Parent.ToLocalSpace(ScreenSpaceDrawQuad.Centre) - panel.AnchorPosition;
            }
        }
    }

    public enum PanelDisplayStyle
    {
        Grid,
        Split,
        Hidden
    }
}
