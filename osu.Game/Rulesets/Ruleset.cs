// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Framework.IO.Stores;
using osu.Game.Beatmaps;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Replays.Types;
using osu.Game.Rulesets.UI;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Configuration;
using osu.Game.Rulesets.Configuration;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Scoring;
using osu.Game.Skinning;

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

        public ModAutoplay GetAutoplayMod() => GetAllMods().OfType<ModAutoplay>().First();

        public virtual ISkin CreateLegacySkinProvider(ISkinSource source) => null;

        protected Ruleset(RulesetInfo rulesetInfo = null)
        {
            RulesetInfo = rulesetInfo ?? createRulesetInfo();
        }

        /// <summary>
        /// Attempt to create a hit renderer for a beatmap
        /// </summary>
        /// <param name="beatmap">The beatmap to create the hit renderer for.</param>
        /// <param name="mods">The <see cref="Mod"/>s to apply.</param>
        /// <exception cref="BeatmapInvalidForRulesetException">Unable to successfully load the beatmap to be usable with this ruleset.</exception>
        /// <returns></returns>
        public abstract DrawableRuleset CreateDrawableRulesetWith(IWorkingBeatmap beatmap, IReadOnlyList<Mod> mods);

        /// <summary>
        /// Creates a <see cref="IBeatmapConverter"/> to convert a <see cref="IBeatmap"/> to one that is applicable for this <see cref="Ruleset"/>.
        /// </summary>
        /// <param name="beatmap">The <see cref="IBeatmap"/> to be converted.</param>
        /// <returns>The <see cref="IBeatmapConverter"/>.</returns>
        public abstract IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap);

        /// <summary>
        /// Optionally creates a <see cref="IBeatmapProcessor"/> to alter a <see cref="IBeatmap"/> after it has been converted.
        /// </summary>
        /// <param name="beatmap">The <see cref="IBeatmap"/> to be processed.</param>
        /// <returns>The <see cref="IBeatmapProcessor"/>.</returns>
        public virtual IBeatmapProcessor CreateBeatmapProcessor(IBeatmap beatmap) => null;

        public abstract DifficultyCalculator CreateDifficultyCalculator(WorkingBeatmap beatmap);

        public virtual PerformanceCalculator CreatePerformanceCalculator(WorkingBeatmap beatmap, ScoreInfo score) => null;

        public virtual HitObjectComposer CreateHitObjectComposer() => null;

        public virtual Drawable CreateIcon() => new SpriteIcon { Icon = FontAwesome.Solid.QuestionCircle };

        public virtual IResourceStore<byte[]> CreateResourceStore() => new NamespacedResourceStore<byte[]>(new DllResourceStore(GetType().Assembly.Location), @"Resources");

        public abstract string Description { get; }

        public virtual RulesetSettingsSubsection CreateSettings() => null;

        /// <summary>
        /// Creates the <see cref="IRulesetConfigManager"/> for this <see cref="Ruleset"/>.
        /// </summary>
        /// <param name="settings">The <see cref="SettingsStore"/> to store the settings.</param>
        public virtual IRulesetConfigManager CreateConfig(SettingsStore settings) => null;

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
