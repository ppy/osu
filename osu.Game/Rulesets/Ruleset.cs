// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets
{
    public abstract class Ruleset
    {
        public readonly RulesetInfo RulesetInfo;

        public virtual IEnumerable<BeatmapStatistic> GetBeatmapStatistics(WorkingBeatmap beatmap) => new BeatmapStatistic[] { };

        public IEnumerable<Mod> GetAllMods() => Enum.GetValues(typeof(ModType)).Cast<ModType>()
                                                // Get all mod types as an IEnumerable<ModType>
                                                .SelectMany(GetModsFor)
                                                // Confine all mods of each mod type into a single IEnumerable<Mod>
                                                .Where(mod => mod != null)
                                                // Filter out all null mods
                                                .SelectMany(mod => (mod as MultiMod)?.Mods ?? new[] { mod });
                                                // Resolve MultiMods as their .Mods property

        public abstract IEnumerable<Mod> GetModsFor(ModType type);

        public Mod GetAutoplayMod() => GetAllMods().First(mod => mod is ModAutoplay);

        protected Ruleset(RulesetInfo rulesetInfo)
        {
            RulesetInfo = rulesetInfo;
        }

        /// <summary>
        /// Attempt to create a hit renderer for a beatmap
        /// </summary>
        /// <param name="beatmap">The beatmap to create the hit renderer for.</param>
        /// <param name="isForCurrentRuleset">Whether the hit renderer should assume the beatmap is for the current ruleset.</param>
        /// <exception cref="BeatmapInvalidForRulesetException">Unable to successfully load the beatmap to be usable with this ruleset.</exception>
        /// <returns></returns>
        public abstract RulesetContainer CreateRulesetContainerWith(WorkingBeatmap beatmap, bool isForCurrentRuleset);

        public abstract DifficultyCalculator CreateDifficultyCalculator(Beatmap beatmap);

        public virtual Drawable CreateIcon() => new SpriteIcon { Icon = FontAwesome.fa_question_circle };

        public abstract string Description { get; }

        public virtual SettingsSubsection CreateSettings() => null;

        /// <summary>
        /// Do not override this unless you are a legacy mode.
        /// </summary>
        public virtual int LegacyID => -1;

        /// <summary>
        /// A list of available variant ids.
        /// </summary>
        public virtual IEnumerable<int> AvailableVariants => new[] { 0 };

        /// <summary>
        /// Get a list of default keys for the specified variant.
        /// </summary>
        /// <param name="variant">A variant.</param>
        /// <returns>A list of valid <see cref="KeyBinding"/>s.</returns>
        public virtual IEnumerable<KeyBinding> GetDefaultKeyBindings(int variant = 0) => new KeyBinding[] { };

        /// <summary>
        /// Gets the name for a key binding variant. This is used for display in the settings overlay.
        /// </summary>
        /// <param name="variant">The variant.</param>
        /// <returns>A descriptive name of the variant.</returns>
        public virtual string GetVariantName(int variant) => string.Empty;
    }
}
