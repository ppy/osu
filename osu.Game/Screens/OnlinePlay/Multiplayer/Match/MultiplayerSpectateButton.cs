// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Match
{
    public class MultiplayerSpectateButton : MultiplayerRoomComposite
    {
        public Action OnSpectateClick
        {
            set => button.Action = value;
        }

        [Resolved]
        private OngoingOperationTracker ongoingOperationTracker { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

        private IBindable<bool> operationInProgress;

        private readonly ButtonWithTrianglesExposed button;

        public MultiplayerSpectateButton()
        {
            InternalChild = button = new ButtonWithTrianglesExposed
            {
                RelativeSizeAxes = Axes.Both,
                Size = Vector2.One,
                Enabled = { Value = true },
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            operationInProgress = ongoingOperationTracker.InProgress.GetBoundCopy();
            operationInProgress.BindValueChanged(_ => updateState());
        }

        protected override void OnRoomUpdated()
        {
            base.OnRoomUpdated();

            updateState();
        }

        private void updateState()
        {
            var localUser = Client.LocalUser;

            if (localUser == null)
                return;

            Debug.Assert(Room != null);

            switch (localUser.State)
            {
                default:
                    button.Text = "Spectate";
                    button.BackgroundColour = colours.BlueDark;
                    button.Triangles.ColourDark = colours.BlueDarker;
                    button.Triangles.ColourLight = colours.Blue;
                    break;

                case MultiplayerUserState.Spectating:
                    button.Text = "Stop spectating";
                    button.BackgroundColour = colours.Gray4;
                    button.Triangles.ColourDark = colours.Gray5;
                    button.Triangles.ColourLight = colours.Gray6;
                    break;
            }

            button.Enabled.Value = Client.Room?.State != MultiplayerRoomState.Closed && !operationInProgress.Value;
        }

        private class ButtonWithTrianglesExposed : TriangleButton
        {
            public new Triangles Triangles => base.Triangles;
        }
    }
}
