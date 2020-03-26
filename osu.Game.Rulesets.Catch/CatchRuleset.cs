﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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
using osu.Game.Rulesets.Catch.Skinning;
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
            if (mods.HasFlag(LegacyMods.Nightcore))
                yield return new CatchModNightcore();
            else if (mods.HasFlag(LegacyMods.DoubleTime))
                yield return new CatchModDoubleTime();

            if (mods.HasFlag(LegacyMods.Perfect))
                yield return new CatchModPerfect();
            else if (mods.HasFlag(LegacyMods.SuddenDeath))
                yield return new CatchModSuddenDeath();

            if (mods.HasFlag(LegacyMods.Cinema))
                yield return new CatchModCinema();
            else if (mods.HasFlag(LegacyMods.Autoplay))
                yield return new CatchModAutoplay();

            if (mods.HasFlag(LegacyMods.Easy))
                yield return new CatchModEasy();

            if (mods.HasFlag(LegacyMods.Flashlight))
                yield return new CatchModFlashlight();

            if (mods.HasFlag(LegacyMods.HalfTime))
                yield return new CatchModHalfTime();

            if (mods.HasFlag(LegacyMods.HardRock))
                yield return new CatchModHardRock();

            if (mods.HasFlag(LegacyMods.Hidden))
                yield return new CatchModHidden();

            if (mods.HasFlag(LegacyMods.NoFail))
                yield return new CatchModNoFail();

            if (mods.HasFlag(LegacyMods.Relax))
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
                        new MultiMod(new ModWindUp(), new ModWindDown())
                    };

                default:
                    return Array.Empty<Mod>();
            }
        }

        public override string Description => "osu!catch";

        public override string ShortName => SHORT_NAME;

        public override string PlayingVerb => "Catching fruit";

        public override Drawable CreateIcon() => new SpriteIcon { Icon = OsuIcon.RulesetCatch };

        public override DifficultyCalculator CreateDifficultyCalculator(WorkingBeatmap beatmap) => new CatchDifficultyCalculator(this, beatmap);

        public override ISkin CreateLegacySkinProvider(ISkinSource source) => new CatchLegacySkinTransformer(source);

        public override PerformanceCalculator CreatePerformanceCalculator(WorkingBeatmap beatmap, ScoreInfo score) => new CatchPerformanceCalculator(this, beatmap, score);

        public int LegacyID => 2;

        public override IConvertibleReplayFrame CreateConvertibleReplayFrame() => new CatchReplayFrame();
    }
}
