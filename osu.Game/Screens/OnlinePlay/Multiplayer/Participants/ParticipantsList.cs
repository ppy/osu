// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Participants
{
    public class ParticipantsList : MultiplayerRoomComposite
    {
        private FillFlowContainer<ParticipantPanel> panels;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = new OsuScrollContainer
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
                panels.RemoveAll(p => !Room.Users.Contains(p.User));

                // Add panels for all users new to the room.
                foreach (var user in Room.Users.Except(panels.Select(p => p.User)))
                    panels.Add(new ParticipantPanel(user));
            }
        }
    }
}
