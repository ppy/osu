// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Settings;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Osu.Edit;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Replays.Types;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Configuration;
using osu.Game.Rulesets.Configuration;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.Osu.Difficulty;
using osu.Game.Rulesets.Osu.Skinning;
using osu.Game.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Osu
{
    public class OsuRuleset : Ruleset
    {
        public override DrawableRuleset CreateDrawableRulesetWith(IWorkingBeatmap beatmap, IReadOnlyList<Mod> mods) => new DrawableOsuRuleset(this, beatmap, mods);
        public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) => new OsuBeatmapConverter(beatmap);
        public override IBeatmapProcessor CreateBeatmapProcessor(IBeatmap beatmap) => new OsuBeatmapProcessor(beatmap);

        public const string SHORT_NAME = "osu";

        public override IEnumerable<KeyBinding> GetDefaultKeyBindings(int variant = 0) => new[]
        {
            new KeyBinding(InputKey.Z, OsuAction.LeftButton),
            new KeyBinding(InputKey.X, OsuAction.RightButton),
            new KeyBinding(InputKey.MouseLeft, OsuAction.LeftButton),
            new KeyBinding(InputKey.MouseRight, OsuAction.RightButton),
        };

        public override IEnumerable<Mod> ConvertLegacyMods(LegacyMods mods)
        {
            if (mods.HasFlag(LegacyMods.Nightcore))
                yield return new OsuModNightcore();
            else if (mods.HasFlag(LegacyMods.DoubleTime))
                yield return new OsuModDoubleTime();

            if (mods.HasFlag(LegacyMods.Perfect))
                yield return new OsuModPerfect();
            else if (mods.HasFlag(LegacyMods.SuddenDeath))
                yield return new OsuModSuddenDeath();

            if (mods.HasFlag(LegacyMods.Autopilot))
                yield return new OsuModAutopilot();

            if (mods.HasFlag(LegacyMods.Autoplay))
                yield return new OsuModAutoplay();

            if (mods.HasFlag(LegacyMods.Easy))
                yield return new OsuModEasy();

            if (mods.HasFlag(LegacyMods.Flashlight))
                yield return new OsuModFlashlight();

            if (mods.HasFlag(LegacyMods.HalfTime))
                yield return new OsuModHalfTime();

            if (mods.HasFlag(LegacyMods.HardRock))
                yield return new OsuModHardRock();

            if (mods.HasFlag(LegacyMods.Hidden))
                yield return new OsuModHidden();

            if (mods.HasFlag(LegacyMods.NoFail))
                yield return new OsuModNoFail();

            if (mods.HasFlag(LegacyMods.Relax))
                yield return new OsuModRelax();

            if (mods.HasFlag(LegacyMods.SpunOut))
                yield return new OsuModSpunOut();

            if (mods.HasFlag(LegacyMods.Target))
                yield return new OsuModTarget();

            if (mods.HasFlag(LegacyMods.TouchDevice))
                yield return new OsuModTouchDevice();
        }

        public override IEnumerable<Mod> GetModsFor(ModType type)
        {
            switch (type)
            {
                case ModType.DifficultyReduction:
                    return new Mod[]
                    {
                        new OsuModEasy(),
                        new OsuModNoFail(),
                        new MultiMod(new OsuModHalfTime(), new OsuModDaycore()),
                        new OsuModSpunOut(),
                    };

                case ModType.DifficultyIncrease:
                    return new Mod[]
                    {
                        new OsuModHardRock(),
                        new MultiMod(new OsuModSuddenDeath(), new OsuModPerfect()),
                        new MultiMod(new OsuModDoubleTime(), new OsuModNightcore()),
                        new OsuModHidden(),
                        new MultiMod(new OsuModFlashlight(), new OsuModBlinds()),
                    };

                case ModType.Conversion:
                    return new Mod[]
                    {
                        new OsuModTarget(),
                    };

                case ModType.Automation:
                    return new Mod[]
                    {
                        new MultiMod(new OsuModAutoplay(), new ModCinema()),
                        new OsuModRelax(),
                        new OsuModAutopilot(),
                    };

                case ModType.Fun:
                    return new Mod[]
                    {
                        new OsuModTransform(),
                        new OsuModWiggle(),
                        new OsuModSpinIn(),
                        new MultiMod(new OsuModGrow(), new OsuModDeflate()),
                        new MultiMod(new ModWindUp(), new ModWindDown()),
                        new OsuModTraceable(),
                    };

                case ModType.System:
                    return new Mod[]
                    {
                        new OsuModTouchDevice(),
                    };

                default:
                    return new Mod[] { };
            }
        }

        public override Drawable CreateIcon() => new SpriteIcon { Icon = OsuIcon.RulesetOsu };

        public override DifficultyCalculator CreateDifficultyCalculator(WorkingBeatmap beatmap) => new OsuDifficultyCalculator(this, beatmap);

        public override PerformanceCalculator CreatePerformanceCalculator(WorkingBeatmap beatmap, ScoreInfo score) => new OsuPerformanceCalculator(this, beatmap, score);

        public override HitObjectComposer CreateHitObjectComposer() => new OsuHitObjectComposer(this);

        public override string Description => "osu!";

        public override string ShortName => SHORT_NAME;

        public override RulesetSettingsSubsection CreateSettings() => new OsuSettingsSubsection(this);

        public override ISkin CreateLegacySkinProvider(ISkinSource source) => new OsuLegacySkinTransformer(source);

        public override int? LegacyID => 0;

        public override IConvertibleReplayFrame CreateConvertibleReplayFrame() => new OsuReplayFrame();

        public override IRulesetConfigManager CreateConfig(SettingsStore settings) => new OsuRulesetConfigManager(settings, RulesetInfo);

        public OsuRuleset(RulesetInfo rulesetInfo = null)
            : base(rulesetInfo)
        {
        }
    }
}
