// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual
{
    /// <summary>
    /// A player that exposes many components that would otherwise not be available, for testing purposes.
    /// </summary>
    public partial class TestReplayPlayer : ReplayPlayer
    {
        protected override bool PauseOnFocusLost { get; }

        public new DrawableRuleset DrawableRuleset => base.DrawableRuleset;

        /// <summary>
        /// Mods from *player* (not OsuScreen).
        /// </summary>
        public new Bindable<IReadOnlyList<Mod>> Mods => base.Mods;

        public new HUDOverlay HUDOverlay => base.HUDOverlay;

        public new GameplayClockContainer GameplayClockContainer => base.GameplayClockContainer;

        public new ScoreProcessor ScoreProcessor => base.ScoreProcessor;

        public new HealthProcessor HealthProcessor => base.HealthProcessor;

        public new bool PauseCooldownActive => base.PauseCooldownActive;

        /// <summary>
        /// Instantiate a replay player that renders an autoplay mod.
        /// </summary>
        public TestReplayPlayer(bool allowPause = true, bool showResults = true, bool pauseOnFocusLost = false)
            : base((beatmap, mods) => mods.OfType<ModAutoplay>().First().CreateScoreFromReplayData(beatmap, mods), new PlayerConfiguration
            {
                AllowPause = allowPause,
                ShowResults = showResults
            })
        {
            PauseOnFocusLost = pauseOnFocusLost;
        }

        /// <summary>
        /// Instantiate a replay player that renders the provided replay.
        /// </summary>
        public TestReplayPlayer(Score score, bool allowPause = true, bool showResults = true, bool pauseOnFocusLost = false)
            : base(score, new PlayerConfiguration
            {
                AllowPause = allowPause,
                ShowResults = showResults
            })
        {
            PauseOnFocusLost = pauseOnFocusLost;
        }
    }
}
