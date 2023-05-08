// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Participants
{
    public partial class ParticipantsList : MultiplayerRoomComposite
    {
        private FillFlowContainer<ParticipantPanel> panels;

        [CanBeNull]
        private ParticipantPanel currentHostPanel;

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

        protected override void OnRoomUpdated()
        {
            base.OnRoomUpdated();

            if (Room == null)
                panels.Clear();
            else
            {
                // Remove panels for users no longer in the room.
                foreach (var p in panels)
                {
                    // Note that we *must* use reference equality here, as this call is scheduled and a user may have left and joined since it was last run.
                    if (Room.Users.All(u => !ReferenceEquals(p.User, u)))
                        p.Expire();
                }

                // Add panels for all users new to the room.
                foreach (var user in Room.Users.Except(panels.Select(p => p.User)))
                    panels.Add(new ParticipantPanel(user));

                if (currentHostPanel == null || !currentHostPanel.User.Equals(Room.Host))
                {
                    // Reset position of previous host back to normal, if one existing.
                    if (currentHostPanel != null && panels.Contains(currentHostPanel))
                        panels.SetLayoutPosition(currentHostPanel, 0);

                    currentHostPanel = null;

                    // Change position of new host to display above all participants.
                    if (Room.Host != null)
                    {
                        currentHostPanel = panels.SingleOrDefault(u => u.User.Equals(Room.Host));

                        if (currentHostPanel != null)
                            panels.SetLayoutPosition(currentHostPanel, -1);
                    }
                }
            }
        }
    }
}
