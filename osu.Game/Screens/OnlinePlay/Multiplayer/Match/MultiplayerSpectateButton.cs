// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.Multiplayer;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Match
{
    public partial class MultiplayerSpectateButton : MultiplayerRoomComposite
    {
        [Resolved]
        private OngoingOperationTracker ongoingOperationTracker { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

        private IBindable<bool> operationInProgress;

        private readonly RoundedButton button;

        public MultiplayerSpectateButton()
        {
            InternalChild = button = new RoundedButton
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

            Client.ToggleSpectate().ContinueWith(_ => endOperation());

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
                    break;

                case MultiplayerUserState.Spectating:
                    button.Text = "Stop spectating";
                    button.BackgroundColour = colours.Gray4;
                    break;
            }

            button.Enabled.Value = Client.Room != null
                                   && Client.Room.State != MultiplayerRoomState.Closed
                                   && !operationInProgress.Value;
        }
    }
}
