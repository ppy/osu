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

        public override void OnResuming(IScreen last)
        {
            base.OnResuming(last);

            if (client.Room == null)
                return;

            Debug.Assert(client.LocalUser != null);

            switch (client.LocalUser.State)
            {
                case MultiplayerUserState.Spectating:
                    break;

                case MultiplayerUserState.WaitingForLoad:
                case MultiplayerUserState.Loaded:
                case MultiplayerUserState.Playing:
                    client.AbortGameplay();
                    break;

                default:
                    client.ChangeState(MultiplayerUserState.Idle);
                    break;
            }
        }

        protected override string ScreenTitle => "Multiplayer";

        protected override RoomManager CreateRoomManager() => new MultiplayerRoomManager();

        protected override LoungeSubScreen CreateLounge() => new MultiplayerLoungeSubScreen();
    }
}
