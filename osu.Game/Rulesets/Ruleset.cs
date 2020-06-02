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
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Skinning;
using osu.Game.Users;
using JetBrains.Annotations;

namespace osu.Game.Rulesets
{
    public abstract class Ruleset
    {
        public RulesetInfo RulesetInfo { get; internal set; }

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
        /// <param name="mods">The legacy enum which will be converted.</param>
        /// <returns>An enumerable of constructed <see cref="Mod"/>s.</returns>
        public virtual IEnumerable<Mod> ConvertFromLegacyMods(LegacyMods mods) => Array.Empty<Mod>();

        /// <summary>
        /// Converts mods to legacy enum values. Do not override if you're not a legacy ruleset.
        /// </summary>
        /// <param name="mods">The mods which will be converted.</param>
        /// <returns>A single bitwise enumerable value representing (to the best of our ability) the mods.</returns>
        public virtual LegacyMods ConvertToLegacyMods(Mod[] mods)
        {
            var value = LegacyMods.None;

            foreach (var mod in mods)
            {
                switch (mod)
                {
                    case ModNoFail _:
                        value |= LegacyMods.NoFail;
                        break;

                    case ModEasy _:
                        value |= LegacyMods.Easy;
                        break;

                    case ModHidden _:
                        value |= LegacyMods.Hidden;
                        break;

                    case ModHardRock _:
                        value |= LegacyMods.HardRock;
                        break;

                    case ModSuddenDeath _:
                        value |= LegacyMods.SuddenDeath;
                        break;

                    case ModDoubleTime _:
                        value |= LegacyMods.DoubleTime;
                        break;

                    case ModRelax _:
                        value |= LegacyMods.Relax;
                        break;

                    case ModHalfTime _:
                        value |= LegacyMods.HalfTime;
                        break;

                    case ModFlashlight _:
                        value |= LegacyMods.Flashlight;
                        break;
                }
            }

            return value;
        }

        [CanBeNull]
        public ModAutoplay GetAutoplayMod() => GetAllMods().OfType<ModAutoplay>().FirstOrDefault();

        public virtual ISkin CreateLegacySkinProvider(ISkinSource source, IBeatmap beatmap) => null;

        protected Ruleset()
        {
            RulesetInfo = new RulesetInfo
            {
                Name = Description,
                ShortName = ShortName,
                ID = (this as ILegacyRuleset)?.LegacyID,
                InstantiationInfo = GetType().AssemblyQualifiedName,
                Available = true,
            };
        }

        /// <summary>
        /// Attempt to create a hit renderer for a beatmap
        /// </summary>
        /// <param name="beatmap">The beatmap to create the hit renderer for.</param>
        /// <param name="mods">The <see cref="Mod"/>s to apply.</param>
        /// <exception cref="BeatmapInvalidForRulesetException">Unable to successfully load the beatmap to be usable with this ruleset.</exception>
        /// <returns></returns>
        public abstract DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod> mods = null);

        /// <summary>
        /// Creates a <see cref="ScoreProcessor"/> for this <see cref="Ruleset"/>.
        /// </summary>
        /// <returns>The score processor.</returns>
        public virtual ScoreProcessor CreateScoreProcessor() => new ScoreProcessor();

        /// <summary>
        /// Creates a <see cref="HealthProcessor"/> for this <see cref="Ruleset"/>.
        /// </summary>
        /// <returns>The health processor.</returns>
        public virtual HealthProcessor CreateHealthProcessor(double drainStartTime) => new DrainingHealthProcessor(drainStartTime);

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

        public virtual IResourceStore<byte[]> CreateResourceStore() => new NamespacedResourceStore<byte[]>(new DllResourceStore(GetType().Assembly), @"Resources");

        public abstract string Description { get; }

        public virtual RulesetSettingsSubsection CreateSettings() => null;

        /// <summary>
        /// Creates the <see cref="IRulesetConfigManager"/> for this <see cref="Ruleset"/>.
        /// </summary>
        /// <param name="settings">The <see cref="SettingsStore"/> to store the settings.</param>
        public virtual IRulesetConfigManager CreateConfig(SettingsStore settings) => null;

        /// <summary>
        /// A unique short name to reference this ruleset in online requests.
        /// </summary>
        public abstract string ShortName { get; }

        /// <summary>
        /// The playing verb to be shown in the <see cref="UserActivity.SoloGame.Status"/>.
        /// </summary>
        public virtual string PlayingVerb => "Playing solo";

        /// <summary>
        /// A list of available variant ids.
        /// </summary>
        public virtual IEnumerable<int> AvailableVariants => new[] { 0 };

        /// <summary>
        /// Get a list of default keys for the specified variant.
        /// </summary>
        /// <param name="variant">A variant.</param>
        /// <returns>A list of valid <see cref="KeyBinding"/>s.</returns>
        public virtual IEnumerable<KeyBinding> GetDefaultKeyBindings(int variant = 0) => Array.Empty<KeyBinding>();

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
    }
}
