// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Screens.OnlinePlay.Lounge;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public class Multiplayer : OnlinePlayScreen
    {
        [Resolved]
        private MultiplayerClient client { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.RoomUpdated += onRoomUpdated;
            onRoomUpdated();
        }

        private void onRoomUpdated()
        {
            if (client.Room == null)
                return;

            Debug.Assert(client.LocalUser != null);

            // If the user exits gameplay before score submission completes, we'll transition to idle when results has been prepared.
            if (client.LocalUser.State == MultiplayerUserState.Results && this.IsCurrentScreen())
                transitionFromResults();
        }

        public override void OnResuming(IScreen last)
        {
            base.OnResuming(last);

            if (client.Room == null)
                return;

            if (!(last is MultiplayerPlayerLoader playerLoader))
                return;

            // If gameplay wasn't finished, then we have a simple path back to the idle state by aborting gameplay.
            if (!playerLoader.GameplayPassed)
            {
                client.AbortGameplay();
                return;
            }

            // If gameplay was completed and the user went all the way to results, we'll transition to idle here.
            // Otherwise, the transition will happen in onRoomUpdated().
            transitionFromResults();
        }

        private void transitionFromResults()
        {
            Debug.Assert(client.LocalUser != null);

            if (client.LocalUser.State == MultiplayerUserState.Results)
                client.ChangeState(MultiplayerUserState.Idle);
        }

        protected override string ScreenTitle => "Multiplayer";

        protected override RoomManager CreateRoomManager() => new MultiplayerRoomManager();

        protected override LoungeSubScreen CreateLounge() => new MultiplayerLoungeSubScreen();

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client != null)
                client.RoomUpdated -= onRoomUpdated;
        }
    }
}
