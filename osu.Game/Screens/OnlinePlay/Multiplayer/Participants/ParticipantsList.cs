// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Online.Multiplayer;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Participants
{
    public class ParticipantsList : MultiplayerRoomComposite
    {
        private FillFlowContainer<ParticipantPanel> panels;

        private Sample userJoinSample;
        private Sample userLeftSample;
        private Sample userKickedSample;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
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

            userJoinSample = audio.Samples.Get(@"Multiplayer/player-joined");
            userLeftSample = audio.Samples.Get(@"Multiplayer/player-left");
            userKickedSample = audio.Samples.Get(@"Multiplayer/player-kicked");
        }

        protected override void UserJoined(MultiplayerRoomUser user)
        {
            base.UserJoined(user);

            userJoinSample?.Play();
        }

        protected override void UserLeft(MultiplayerRoomUser user)
        {
            base.UserLeft(user);

            userLeftSample?.Play();
        }

        protected override void UserKicked(MultiplayerRoomUser user)
        {
            base.UserKicked(user);

            userKickedSample?.Play();
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
            }
        }
    }
}
