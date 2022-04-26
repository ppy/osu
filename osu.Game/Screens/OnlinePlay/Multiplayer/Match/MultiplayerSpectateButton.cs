// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
                Action = onClick
            };
        }

        private void onClick()
        {
            var clickOperation = ongoingOperationTracker.BeginOperation();

            Client.ToggleSpectate().ContinueWith(t => endOperation());

            void endOperation() => clickOperation?.Dispose();
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
            switch (Client.LocalUser?.State)
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

            button.Enabled.Value = Client.Room != null
                                   && Client.Room.State != MultiplayerRoomState.Closed
                                   && !operationInProgress.Value;
        }

        private class ButtonWithTrianglesExposed : TriangleButton
        {
            public new Triangles Triangles => base.Triangles;
        }
    }
}
