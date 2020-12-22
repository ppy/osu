// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.RealtimeMultiplayer;
using osu.Game.Screens.Multi.Components;
using osuTK;

namespace osu.Game.Screens.Multi.RealtimeMultiplayer.Match
{
    public class RealtimeReadyButton : RealtimeRoomComposite
    {
        public Bindable<PlaylistItem> SelectedItem => button.SelectedItem;

        [Resolved]
        private IAPIProvider api { get; set; }

        [CanBeNull]
        private MultiplayerRoomUser localUser;

        [Resolved]
        private OsuColour colours { get; set; }

        private readonly ButtonWithTrianglesExposed button;

        public RealtimeReadyButton()
        {
            InternalChild = button = new ButtonWithTrianglesExposed
            {
                RelativeSizeAxes = Axes.Both,
                Size = Vector2.One,
                Enabled = { Value = true },
                Action = onClick
            };
        }

        protected override void OnRoomChanged()
        {
            base.OnRoomChanged();

            localUser = Room?.Users.Single(u => u.User?.Id == api.LocalUser.Value.Id);
            button.Enabled.Value = Client.Room?.State == MultiplayerRoomState.Open;
            updateState();
        }

        private void updateState()
        {
            if (localUser == null)
                return;

            Debug.Assert(Room != null);

            switch (localUser.State)
            {
                case MultiplayerUserState.Idle:
                    button.Text = "Ready";
                    updateButtonColour(true);
                    break;

                case MultiplayerUserState.Ready:
                    if (Room?.Host?.Equals(localUser) == true)
                    {
                        int countReady = Room.Users.Count(u => u.State == MultiplayerUserState.Ready);
                        button.Text = $"Start match ({countReady} / {Room.Users.Count} ready)";
                        updateButtonColour(true);
                    }
                    else
                    {
                        button.Text = "Waiting for host...";
                        updateButtonColour(false);
                    }

                    break;
            }
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
                Client.ChangeState(MultiplayerUserState.Ready);
            else
            {
                if (Room?.Host?.Equals(localUser) == true)
                    Client.StartMatch();
                else
                    Client.ChangeState(MultiplayerUserState.Idle);
            }
        }

        private class ButtonWithTrianglesExposed : ReadyButton
        {
            public new Triangles Triangles => base.Triangles;
        }
    }
}
