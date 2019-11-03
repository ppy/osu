// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Replays.Types;
using osu.Game.Rulesets.Taiko.Replays;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Taiko.Beatmaps;
using osu.Game.Rulesets.Taiko.Difficulty;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Taiko
{
    public class TaikoRuleset : Ruleset
    {
        public override DrawableRuleset CreateDrawableRulesetWith(IWorkingBeatmap beatmap, IReadOnlyList<Mod> mods) => new DrawableTaikoRuleset(this, beatmap, mods);
        public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) => new TaikoBeatmapConverter(beatmap);

        public const string SHORT_NAME = "taiko";

        public override IEnumerable<KeyBinding> GetDefaultKeyBindings(int variant = 0) => new[]
        {
            new KeyBinding(InputKey.MouseLeft, TaikoAction.LeftCentre),
            new KeyBinding(InputKey.MouseRight, TaikoAction.LeftRim),
            new KeyBinding(InputKey.D, TaikoAction.LeftRim),
            new KeyBinding(InputKey.F, TaikoAction.LeftCentre),
            new KeyBinding(InputKey.J, TaikoAction.RightCentre),
            new KeyBinding(InputKey.K, TaikoAction.RightRim),
        };

        public override IEnumerable<Mod> ConvertLegacyMods(LegacyMods mods)
        {
            if (mods.HasFlag(LegacyMods.Nightcore))
                yield return new TaikoModNightcore();
            else if (mods.HasFlag(LegacyMods.DoubleTime))
                yield return new TaikoModDoubleTime();

            if (mods.HasFlag(LegacyMods.Perfect))
                yield return new TaikoModPerfect();
            else if (mods.HasFlag(LegacyMods.SuddenDeath))
                yield return new TaikoModSuddenDeath();

            if (mods.HasFlag(LegacyMods.Autoplay))
                yield return new TaikoModAutoplay();

            if (mods.HasFlag(LegacyMods.Easy))
                yield return new TaikoModEasy();

            if (mods.HasFlag(LegacyMods.Flashlight))
                yield return new TaikoModFlashlight();

            if (mods.HasFlag(LegacyMods.HalfTime))
                yield return new TaikoModHalfTime();

            if (mods.HasFlag(LegacyMods.HardRock))
                yield return new TaikoModHardRock();

            if (mods.HasFlag(LegacyMods.Hidden))
                yield return new TaikoModHidden();

            if (mods.HasFlag(LegacyMods.NoFail))
                yield return new TaikoModNoFail();

            if (mods.HasFlag(LegacyMods.Relax))
                yield return new TaikoModRelax();
        }

        public override IEnumerable<Mod> GetModsFor(ModType type)
        {
            switch (type)
            {
                case ModType.DifficultyReduction:
                    return new Mod[]
                    {
                        new TaikoModEasy(),
                        new TaikoModNoFail(),
                        new MultiMod(new TaikoModHalfTime(), new TaikoModDaycore()),
                    };

                case ModType.DifficultyIncrease:
                    return new Mod[]
                    {
                        new TaikoModHardRock(),
                        new MultiMod(new TaikoModSuddenDeath(), new TaikoModPerfect()),
                        new MultiMod(new TaikoModDoubleTime(), new TaikoModNightcore()),
                        new TaikoModHidden(),
                        new TaikoModFlashlight(),
                    };

                case ModType.Automation:
                    return new Mod[]
                    {
                        new MultiMod(new TaikoModAutoplay(), new ModCinema()),
                        new TaikoModRelax(),
                    };

                case ModType.Fun:
                    return new Mod[]
                    {
                        new MultiMod(new ModWindUp(), new ModWindDown())
                    };

                default:
                    return new Mod[] { };
            }
        }

        public override string Description => "osu!taiko";

        public override string ShortName => SHORT_NAME;

        public override Drawable CreateIcon() => new SpriteIcon { Icon = OsuIcon.RulesetTaiko };

        public override DifficultyCalculator CreateDifficultyCalculator(WorkingBeatmap beatmap) => new TaikoDifficultyCalculator(this, beatmap);

        public override PerformanceCalculator CreatePerformanceCalculator(WorkingBeatmap beatmap, ScoreInfo score) => new TaikoPerformanceCalculator(this, beatmap, score);

        public override int? LegacyID => 1;

        public override IConvertibleReplayFrame CreateConvertibleReplayFrame() => new TaikoReplayFrame();

        public TaikoRuleset(RulesetInfo rulesetInfo = null)
            : base(rulesetInfo)
        {
        }
    }
}
