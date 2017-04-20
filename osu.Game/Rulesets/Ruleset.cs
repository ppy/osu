// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using System.Collections.Generic;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets
{
    public abstract class Ruleset
    {
        public virtual IEnumerable<BeatmapStatistic> GetBeatmapStatistics(WorkingBeatmap beatmap) => new BeatmapStatistic[] { };

        public abstract IEnumerable<Mod> GetModsFor(ModType type);

        /// <summary>
        /// Attempt to create a HitRenderer for the provided beatmap.
        /// </summary>
        /// <param name="beatmap"></param>
        /// <exception cref="BeatmapInvalidForRulesetException">Unable to successfully load the beatmap to be usable with this ruleset.</exception>
        /// <returns></returns>
        public abstract HitRenderer CreateHitRendererWith(WorkingBeatmap beatmap);

        public abstract DifficultyCalculator CreateDifficultyCalculator(Beatmap beatmap);

        public abstract ScoreProcessor CreateScoreProcessor();

        public virtual FontAwesome Icon => FontAwesome.fa_question_circle;

        public abstract string Description { get; }

        public abstract IEnumerable<KeyCounter> CreateGameplayKeys();

        /// <summary>
        /// Do not override this unless you are a legacy mode.
        /// </summary>
        public virtual int LegacyID => -1;
    }
}
