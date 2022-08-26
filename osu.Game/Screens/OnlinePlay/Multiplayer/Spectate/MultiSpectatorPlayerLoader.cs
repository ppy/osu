// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Game.Scoring;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Play;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    /// <summary>
    /// Used to load a single <see cref="MultiSpectatorPlayer"/> in a <see cref="MultiSpectatorScreen"/>.
    /// </summary>
    public class MultiSpectatorPlayerLoader : SpectatorPlayerLoader
    {
        public MultiSpectatorPlayerLoader([NotNull] Score score, [NotNull] Func<MultiSpectatorPlayer> createPlayer)
            : base(score, createPlayer)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            PlayerSettings.Expire();
        }

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
        }

        protected override void LogoExiting(OsuLogo logo)
        {
        }
    }
}
