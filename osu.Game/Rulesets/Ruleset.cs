// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Replays.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets.Difficulty;

namespace osu.Game.Rulesets
{
    public abstract class Ruleset
    {
        public readonly RulesetInfo RulesetInfo;

        public IEnumerable<Mod> GetAllMods() => Enum.GetValues(typeof(ModType)).Cast<ModType>()
                                                    // Confine all mods of each mod type into a single IEnumerable<Mod>
                                                    .SelectMany(GetModsFor)
                                                    // Filter out all null mods
                                                    .Where(mod => mod != null)
                                                    // Resolve MultiMods as their .Mods property
                                                    .SelectMany(mod => (mod as MultiMod)?.Mods ?? new[] { mod });

        public abstract IEnumerable<Mod> GetModsFor(ModType type);

        /// <summary>
        /// Converts mods from legacy enum values. Do not override if you're not a legacy ruleset.
        /// </summary>
        /// <param name="mods">The legacy enum which will be converted</param>
        /// <returns>An enumerable of constructed <see cref="Mod"/>s</returns>
        public virtual IEnumerable<Mod> ConvertLegacyMods(LegacyMods mods) => new Mod[] { };

        public Mod GetAutoplayMod() => GetAllMods().First(mod => mod is ModAutoplay);

        protected Ruleset(RulesetInfo rulesetInfo = null)
        {
            RulesetInfo = rulesetInfo ?? createRulesetInfo();
        }

        /// <summary>
        /// Attempt to create a hit renderer for a beatmap
        /// </summary>
        /// <param name="beatmap">The beatmap to create the hit renderer for.</param>
        /// <exception cref="BeatmapInvalidForRulesetException">Unable to successfully load the beatmap to be usable with this ruleset.</exception>
        /// <returns></returns>
        public abstract RulesetContainer CreateRulesetContainerWith(WorkingBeatmap beatmap);

        public abstract IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap);

        public virtual IBeatmapProcessor CreateBeatmapProcessor(IBeatmap beatmap) => null;

        public abstract DifficultyCalculator CreateDifficultyCalculator(IBeatmap beatmap, Mod[] mods = null);

        public virtual PerformanceCalculator CreatePerformanceCalculator(IBeatmap beatmap, Score score) => null;

        public virtual HitObjectComposer CreateHitObjectComposer() => null;

        public virtual Drawable CreateIcon() => new SpriteIcon { Icon = FontAwesome.fa_question_circle };

        public abstract string Description { get; }

        public virtual SettingsSubsection CreateSettings() => null;

        /// <summary>
        /// Do not override this unless you are a legacy mode.
        /// </summary>
        public virtual int? LegacyID => null;

        /// <summary>
        /// A unique short name to reference this ruleset in online requests.
        /// </summary>
        public abstract string ShortName { get; }

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

        /// <summary>
        /// For rulesets which support legacy (osu-stable) replay conversion, this method will create an empty replay frame
        /// for conversion use.
        /// </summary>
        /// <returns>An empty frame for the current ruleset, or null if unsupported.</returns>
        public virtual IConvertibleReplayFrame CreateConvertibleReplayFrame() => null;

        /// <summary>
        /// Create a ruleset info based on this ruleset.
        /// </summary>
        /// <returns>A filled <see cref="RulesetInfo"/>.</returns>
        private RulesetInfo createRulesetInfo() => new RulesetInfo
        {
            Name = Description,
            ShortName = ShortName,
            InstantiationInfo = GetType().AssemblyQualifiedName,
            ID = LegacyID,
            Available = true
        };
    }
}
