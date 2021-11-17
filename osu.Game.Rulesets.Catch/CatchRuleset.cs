// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Catch.Mods;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Catch.Replays;
using osu.Game.Rulesets.Replays.Types;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Difficulty;
using osu.Game.Rulesets.Catch.Scoring;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using System;
using osu.Framework.Extensions.EnumExtensions;
using osu.Game.Rulesets.Catch.Edit;
using osu.Game.Rulesets.Catch.Skinning.Legacy;
using osu.Game.Rulesets.Edit;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Catch
{
    public class CatchRuleset : Ruleset, ILegacyRuleset
    {
        public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod> mods = null) => new DrawableCatchRuleset(this, beatmap, mods);

        public override ScoreProcessor CreateScoreProcessor() => new CatchScoreProcessor();

        public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) => new CatchBeatmapConverter(beatmap, this);

        public override IBeatmapProcessor CreateBeatmapProcessor(IBeatmap beatmap) => new CatchBeatmapProcessor(beatmap);

        public const string SHORT_NAME = "fruits";

        public override IEnumerable<KeyBinding> GetDefaultKeyBindings(int variant = 0) => new[]
        {
            new KeyBinding(InputKey.Z, CatchAction.MoveLeft),
            new KeyBinding(InputKey.Left, CatchAction.MoveLeft),
            new KeyBinding(InputKey.X, CatchAction.MoveRight),
            new KeyBinding(InputKey.Right, CatchAction.MoveRight),
            new KeyBinding(InputKey.Shift, CatchAction.Dash),
            new KeyBinding(InputKey.Shift, CatchAction.Dash),
        };

        public override IEnumerable<Mod> ConvertFromLegacyMods(LegacyMods mods)
        {
            if (mods.HasFlagFast(LegacyMods.Nightcore))
                yield return new CatchModNightcore();
            else if (mods.HasFlagFast(LegacyMods.DoubleTime))
                yield return new CatchModDoubleTime();

            if (mods.HasFlagFast(LegacyMods.Perfect))
                yield return new CatchModPerfect();
            else if (mods.HasFlagFast(LegacyMods.SuddenDeath))
                yield return new CatchModSuddenDeath();

            if (mods.HasFlagFast(LegacyMods.Cinema))
                yield return new CatchModCinema();
            else if (mods.HasFlagFast(LegacyMods.Autoplay))
                yield return new CatchModAutoplay();

            if (mods.HasFlagFast(LegacyMods.Easy))
                yield return new CatchModEasy();

            if (mods.HasFlagFast(LegacyMods.Flashlight))
                yield return new CatchModFlashlight();

            if (mods.HasFlagFast(LegacyMods.HalfTime))
                yield return new CatchModHalfTime();

            if (mods.HasFlagFast(LegacyMods.HardRock))
                yield return new CatchModHardRock();

            if (mods.HasFlagFast(LegacyMods.Hidden))
                yield return new CatchModHidden();

            if (mods.HasFlagFast(LegacyMods.NoFail))
                yield return new CatchModNoFail();

            if (mods.HasFlagFast(LegacyMods.Relax))
                yield return new CatchModRelax();
        }

        public override IEnumerable<Mod> GetModsFor(ModType type)
        {
            switch (type)
            {
                case ModType.DifficultyReduction:
                    return new Mod[]
                    {
                        new CatchModEasy(),
                        new CatchModNoFail(),
                        new MultiMod(new CatchModHalfTime(), new CatchModDaycore())
                    };

                case ModType.DifficultyIncrease:
                    return new Mod[]
                    {
                        new CatchModHardRock(),
                        new MultiMod(new CatchModSuddenDeath(), new CatchModPerfect()),
                        new MultiMod(new CatchModDoubleTime(), new CatchModNightcore()),
                        new CatchModHidden(),
                        new CatchModFlashlight(),
                    };

                case ModType.Conversion:
                    return new Mod[]
                    {
                        new CatchModDifficultyAdjust(),
                        new CatchModClassic(),
                        new CatchModMirror(),
                    };

                case ModType.Automation:
                    return new Mod[]
                    {
                        new MultiMod(new CatchModAutoplay(), new CatchModCinema()),
                        new CatchModRelax(),
                    };

                case ModType.Fun:
                    return new Mod[]
                    {
                        new MultiMod(new ModWindUp(), new ModWindDown()),
                        new CatchModFloatingFruits(),
                        new CatchModMuted(),
                        new CatchModNoScope(),
                    };

                default:
                    return Array.Empty<Mod>();
            }
        }

        public override string Description => "osu!catch";

        public override string ShortName => SHORT_NAME;

        public override string PlayingVerb => "Catching fruit";

        public override Drawable CreateIcon() => new SpriteIcon { Icon = OsuIcon.RulesetCatch };

        protected override IEnumerable<HitResult> GetValidHitResults()
        {
            return new[]
            {
                HitResult.Great,

                HitResult.LargeTickHit,
                HitResult.SmallTickHit,
                HitResult.LargeBonus,
            };
        }

        public override string GetDisplayNameForHitResult(HitResult result)
        {
            switch (result)
            {
                case HitResult.LargeTickHit:
                    return "Large droplet";

                case HitResult.SmallTickHit:
                    return "Small droplet";

                case HitResult.LargeBonus:
                    return "Banana";
            }

            return base.GetDisplayNameForHitResult(result);
        }

        public override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) => new CatchDifficultyCalculator(RulesetInfo, beatmap);

        public override ISkin CreateLegacySkinProvider(ISkin skin, IBeatmap beatmap) => new CatchLegacySkinTransformer(skin);

        public override PerformanceCalculator CreatePerformanceCalculator(DifficultyAttributes attributes, ScoreInfo score) => new CatchPerformanceCalculator(this, attributes, score);

        public int LegacyID => 2;

        public override IConvertibleReplayFrame CreateConvertibleReplayFrame() => new CatchReplayFrame();

        public override HitObjectComposer CreateHitObjectComposer() => new CatchHitObjectComposer(this);

        public override IBeatmapVerifier CreateBeatmapVerifier() => new CatchBeatmapVerifier();
    }
}
