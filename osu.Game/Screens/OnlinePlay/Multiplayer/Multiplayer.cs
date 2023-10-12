// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Screens.OnlinePlay.Lounge;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public partial class Multiplayer : OnlinePlayScreen
    {
        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.RoomUpdated += onRoomUpdated;
            client.LoadAborted += onLoadAborted;
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

        private void onLoadAborted()
        {
            // If the server aborts gameplay for this user (due to loading too slow), exit gameplay screens.
            if (!this.IsCurrentScreen())
            {
                Logger.Log("Gameplay aborted because loading the beatmap took too long.", LoggingTarget.Runtime, LogLevel.Important);
                this.MakeCurrent();
            }
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);

            if (client.Room == null)
                return;

            Debug.Assert(client.LocalUser != null);

            if (!(e.Last is MultiplayerPlayerLoader playerLoader))
                return;

            // Nothing needs to be done if already in the idle state (e.g. via load being aborted by the server).
            if (client.LocalUser.State == MultiplayerUserState.Idle)
                return;

            // If gameplay wasn't finished, then we have a simple path back to the idle state by aborting gameplay.
            if (!playerLoader.GameplayPassed)
            {
                client.AbortGameplay().FireAndForget();
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

        public void Join(Room room, string? password) => Schedule(() => Lounge.Join(room, password));

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
                client.RoomUpdated -= onRoomUpdated;
        }
    }
}
