// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public partial class MultiplayerPlayerLoader : PlayerLoader
    {
        public bool GameplayPassed => player?.GameplayState.HasPassed == true;

        public override bool AllowUserExit => false;

        [Resolved]
        private MultiplayerClient multiplayerClient { get; set; } = null!;

        [Resolved]
        private OsuGame? game { get; set; }

        private Player? player;

        public MultiplayerPlayerLoader(Func<Player> createPlayer)
            : base(createPlayer)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            PlayerSettings.Add(new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Child = new HoldForMenuButton(true)
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Margin = new MarginPadding(10),
                    Action = () =>
                    {
                        if (this.IsCurrentScreen())
                            this.Exit();
                    }
                }
            });
        }

        protected override bool ReadyForGameplay =>
            (
                // The user is ready to enter gameplay.
                base.ReadyForGameplay
                // And the server has received the message that we're loaded.
                && multiplayerClient.LocalUser?.State == MultiplayerUserState.Loaded
            )
            // Or the server is forcefully starting gameplay.
            || multiplayerClient.LocalUser?.State == MultiplayerUserState.Playing;

        protected override void OnPlayerLoaded()
        {
            base.OnPlayerLoaded();

            game?.Window?.Flash();

            multiplayerClient.ChangeState(MultiplayerUserState.Loaded)
                             .ContinueWith(task => failAndBail(task.Exception?.Message ?? "Server error"), TaskContinuationOptions.NotOnRanToCompletion);
        }

        private void failAndBail(string? message = null)
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
