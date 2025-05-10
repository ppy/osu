// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Multiplayer;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Participants
{
    public partial class ParticipantsList : CompositeDrawable
    {
        private FillFlowContainer<ParticipantPanel> panels = null!;
        private ParticipantPanel? currentHostPanel;

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new OsuScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                ScrollbarVisible = false,
                Child = panels = new FillFlowContainer<ParticipantPanel>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 2)
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.RoomUpdated += onRoomUpdated;
            updateState();
        }

        private void onRoomUpdated() => Scheduler.AddOnce(updateState);

        private void updateState()
        {
            if (client.Room == null)
                panels.Clear();
            else
            {
                // Remove panels for users no longer in the room.
                foreach (var p in panels)
                {
                    // Note that we *must* use reference equality here, as this call is scheduled and a user may have left and joined since it was last run.
                    if (client.Room.Users.All(u => !ReferenceEquals(p.User, u)))
                        p.Expire();
                }

                // Add panels for all users new to the room.
                foreach (var user in client.Room.Users.Except(panels.Select(p => p.User)))
                    panels.Add(new ParticipantPanel(user));

                if (currentHostPanel == null || !currentHostPanel.User.Equals(client.Room.Host))
                {
                    // Reset position of previous host back to normal, if one existing.
                    if (currentHostPanel != null && panels.Contains(currentHostPanel))
                        panels.SetLayoutPosition(currentHostPanel, 0);

                    currentHostPanel = null;

                    // Change position of new host to display above all participants.
                    if (client.Room.Host != null)
                    {
                        currentHostPanel = panels.SingleOrDefault(u => u.User.Equals(client.Room.Host));

                        if (currentHostPanel != null)
                            panels.SetLayoutPosition(currentHostPanel, -1);
                    }
                }
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
                client.RoomUpdated -= onRoomUpdated;
        }
    }
}
