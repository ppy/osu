// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Play;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public class MultiplayerPlayerLoader : PlayerLoader
    {
        public bool GameplayPassed => player?.GameplayState.HasPassed == true;

        [Resolved]
        private MultiplayerClient multiplayerClient { get; set; }

        private Player player;

        public MultiplayerPlayerLoader(Func<Player> createPlayer)
            : base(createPlayer)
        {
        }

        protected override bool ReadyForGameplay =>
            base.ReadyForGameplay
            // The server is forcefully starting gameplay.
            || multiplayerClient.LocalUser?.State == MultiplayerUserState.Playing;

        protected override void OnPlayerLoaded()
        {
            base.OnPlayerLoaded();

            multiplayerClient.ChangeState(MultiplayerUserState.Loaded)
                             .ContinueWith(task => failAndBail(task.Exception?.Message ?? "Server error"), TaskContinuationOptions.NotOnRanToCompletion);
        }

        private void failAndBail(string message = null)
        {
            if (!string.IsNullOrEmpty(message))
                Logger.Log(message, LoggingTarget.Runtime, LogLevel.Important);

            Schedule(() =>
            {
                if (this.IsCurrentScreen())
                    this.Exit();
            });
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            base.OnSuspending(e);
            player = (Player)e.Next;
        }
    }
}
