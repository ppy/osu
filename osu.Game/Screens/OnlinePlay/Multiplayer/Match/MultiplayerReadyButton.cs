// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Components;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Match
{
    public class MultiplayerReadyButton : MultiplayerRoomComposite
    {
        public Bindable<PlaylistItem> SelectedItem => button.SelectedItem;

        [Resolved]
        private IAPIProvider api { get; set; }

        [CanBeNull]
        private MultiplayerRoomUser localUser;

        [Resolved]
        private OsuColour colours { get; set; }

        private SampleChannel sampleReadyCount;

        private readonly ButtonWithTrianglesExposed button;

        private int countReady;

        public MultiplayerReadyButton()
        {
            InternalChild = button = new ButtonWithTrianglesExposed
            {
                RelativeSizeAxes = Axes.Both,
                Size = Vector2.One,
                Enabled = { Value = true },
                Action = onClick
            };
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleReadyCount = audio.Samples.Get(@"SongSelect/select-difficulty");
        }

        protected override void OnRoomUpdated()
        {
            base.OnRoomUpdated();

            // this method is called on leaving the room, so the local user may not exist in the room any more.
            localUser = Room?.Users.SingleOrDefault(u => u.User?.Id == api.LocalUser.Value.Id);

            button.Enabled.Value = Client.Room?.State == MultiplayerRoomState.Open;
            updateState();
        }

        private void updateState()
        {
            if (localUser == null)
                return;

            Debug.Assert(Room != null);

            int newCountReady = Room.Users.Count(u => u.State == MultiplayerUserState.Ready);

            string countText = $"({newCountReady} / {Room.Users.Count} ready)";

            switch (localUser.State)
            {
                case MultiplayerUserState.Idle:
                    button.Text = "Ready";
                    updateButtonColour(true);
                    break;

                case MultiplayerUserState.Ready:
                    if (Room?.Host?.Equals(localUser) == true)
                    {
                        button.Text = $"Start match {countText}";
                        updateButtonColour(true);
                    }
                    else
                    {
                        button.Text = $"Waiting for host... {countText}";
                        updateButtonColour(false);
                    }

                    break;
            }

            if (newCountReady != countReady)
            {
                countReady = newCountReady;
                Scheduler.AddOnce(playSound);
            }
        }

        private void playSound()
        {
            if (sampleReadyCount == null)
                return;

            sampleReadyCount.Frequency.Value = 0.77f + countReady * 0.06f;
            sampleReadyCount.Play();
        }

        private void updateButtonColour(bool green)
        {
            if (green)
            {
                button.BackgroundColour = colours.Green;
                button.Triangles.ColourDark = colours.Green;
                button.Triangles.ColourLight = colours.GreenLight;
            }
            else
            {
                button.BackgroundColour = colours.YellowDark;
                button.Triangles.ColourDark = colours.YellowDark;
                button.Triangles.ColourLight = colours.Yellow;
            }
        }

        private void onClick()
        {
            if (localUser == null)
                return;

            if (localUser.State == MultiplayerUserState.Idle)
                Client.ChangeState(MultiplayerUserState.Ready).CatchUnobservedExceptions(true);
            else
            {
                if (Room?.Host?.Equals(localUser) == true)
                    Client.StartMatch().CatchUnobservedExceptions(true);
                else
                    Client.ChangeState(MultiplayerUserState.Idle).CatchUnobservedExceptions(true);
            }
        }

        private class ButtonWithTrianglesExposed : ReadyButton
        {
            public new Triangles Triangles => base.Triangles;
        }
    }
}
