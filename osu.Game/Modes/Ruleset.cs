// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Modes.Mods;
using osu.Game.Modes.UI;
using osu.Game.Screens.Play;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace osu.Game.Modes
{
    public class BeatmapStatistic
    {
        public FontAwesome Icon;
        public string Content;
        public string Name;
    }

    public abstract class Ruleset
    {
        private static ConcurrentDictionary<PlayMode, Type> availableRulesets = new ConcurrentDictionary<PlayMode, Type>();

        public static IEnumerable<PlayMode> PlayModes => availableRulesets.Keys;

        public virtual IEnumerable<BeatmapStatistic> GetBeatmapStatistics(WorkingBeatmap beatmap) => new BeatmapStatistic[] { };

        public abstract IEnumerable<Mod> GetModsFor(ModType type);

        public abstract HitRenderer CreateHitRendererWith(WorkingBeatmap beatmap);

        public abstract DifficultyCalculator CreateDifficultyCalculator(Beatmap beatmap);

        public abstract ScoreProcessor CreateScoreProcessor();

        public static void Register(Ruleset ruleset) => availableRulesets.TryAdd(ruleset.PlayMode, ruleset.GetType());

        protected abstract PlayMode PlayMode { get; }

        public virtual FontAwesome Icon => FontAwesome.fa_question_circle;

        public abstract string Description { get; }

        public abstract IEnumerable<KeyCounter> CreateGameplayKeys();

        public static Ruleset GetRuleset(PlayMode mode)
        {
            Type type;

            if (!availableRulesets.TryGetValue(mode, out type))
                return null;

            return Activator.CreateInstance(type) as Ruleset;
        }

    }
}
