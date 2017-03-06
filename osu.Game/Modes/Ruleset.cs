// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Modes.Objects;
using osu.Game.Modes.UI;
using System.Reflection;
using System.IO;
using System.Linq;

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

        public static ICollection<PlayMode> PlayModes => availableRulesets.Keys;

        public abstract ScoreOverlay CreateScoreOverlay();

        public virtual IEnumerable<BeatmapStatistic> GetBeatmapStatistics(WorkingBeatmap beatmap) => new BeatmapStatistic[] { };

        public abstract IEnumerable<Mod> GetModsFor(ModType type);

        public abstract ScoreProcessor CreateScoreProcessor(int hitObjectCount);

        public abstract HitRenderer CreateHitRendererWith(Beatmap beatmap);

        public abstract HitObjectParser CreateHitObjectParser();

        public abstract DifficultyCalculator CreateDifficultyCalculator(Beatmap beatmap);

        public static void Register(Ruleset ruleset) => availableRulesets.TryAdd(ruleset.PlayMode, ruleset.GetType());

        protected abstract PlayMode PlayMode { get; }

        public virtual FontAwesome Icon => FontAwesome.fa_question_circle;

        public abstract string Description { get; }

        public static Ruleset GetRuleset(PlayMode mode)
        {
            Type type;

            if (!availableRulesets.TryGetValue(mode, out type))
                return null;

            return Activator.CreateInstance(type) as Ruleset;
        }
        
        public static void LoadRulesetsFrom(string directory, string filter = "", AppDomain domain = null)
        {
            if (domain == null)
            {
                domain = AppDomain.CreateDomain("rulesetLoader");
                LoadRulesetsFrom(directory, filter, domain);
                AppDomain.Unload(domain);
            }
            else
            {
                foreach (string file in Directory.EnumerateFiles(directory))
                {
                    try
                    {
                        if (!(file.Contains(filter)&&file.EndsWith(".dll")))
                            continue;
                        
                        var assembly = domain.Load(AssemblyName.GetAssemblyName(file));
                        var rulesets = assembly.GetTypes().Where((Type t) => t.IsSubclassOf(typeof(Ruleset)));
                        if (rulesets.Count() > 0)
                            Assembly.LoadFile(file);

                        foreach (Type rulesetType in rulesets)
                            Register(Activator.CreateInstance(rulesetType) as Ruleset);
                    }
                    catch (Exception) { }
                }
            }
        }
    }
}
