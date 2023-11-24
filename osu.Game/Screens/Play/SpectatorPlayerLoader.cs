// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Scoring;
using osu.Game.Screens.Menu;

namespace osu.Game.Screens.Play
{
    public partial class SpectatorPlayerLoader : PlayerLoader
    {
        public readonly ScoreInfo Score;

        public SpectatorPlayerLoader(Score score, Func<SpectatorPlayer> createPlayer)
            : base(createPlayer)
        {
            if (score.Replay == null)
                throw new ArgumentException($"{nameof(score)} must have a non-null {nameof(score.Replay)}.", nameof(score));

            Score = score.ScoreInfo;
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            // these will be reverted thanks to PlayerLoader's lease.
            Mods.Value = Score.Mods;
            Ruleset.Value = Score.Ruleset;

            base.OnEntering(e);
        }

        protected override void LogoExiting(OsuLogo logo)
        {
            const double logo_transition = 250;

            base.LogoExiting(logo);
            logo.ScaleTo(0.2f, logo_transition / 2, Easing.Out);
            logo.FadeOut(logo_transition / 2, Easing.Out);
        }
    }
}
