// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual
{
    /// <summary>
    /// A player that exposes many components that would otherwise not be available, for testing purposes.
    /// </summary>
    public class TestPlayer : Player
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

        public readonly List<JudgementResult> Results = new List<JudgementResult>();

        public TestPlayer(bool allowPause = true, bool showResults = true, bool pauseOnFocusLost = false)
            : base(new PlayerConfiguration
            {
                AllowPause = allowPause,
                ShowResults = showResults
            })
        {
            PauseOnFocusLost = pauseOnFocusLost;
        }

        protected override void PrepareReplay()
        {
            var replayGeneratingMod = Mods.Value.OfType<ICreateReplay>().FirstOrDefault();

            if (replayGeneratingMod != null)
            {
                // This logic should really not exist (and tests should be instantiating a ReplayPlayer), but a lot of base work is required to make that happen.
                DrawableRuleset?.SetReplayScore(replayGeneratingMod.CreateReplayScore(GameplayBeatmap.PlayableBeatmap, Mods.Value));
                return;
            }

            base.PrepareReplay();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            ScoreProcessor.NewJudgement += r => Results.Add(r);
        }
    }
}
