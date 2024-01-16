// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Tests.Mods
{
    public partial class TestSceneTaikoModHidden : TaikoModTestScene
    {
        private Func<bool> checkAllMaxResultJudgements(int count) => ()
            => Player.ScoreProcessor.JudgedHits >= count
               && Player.Results.All(result => result.Type == result.JudgementCriteria.MaxResult);

        [Test]
        public void TestDefaultBeatmapTest() => CreateModTest(new ModTestData
        {
            Mod = new TaikoModHidden(),
            Autoplay = true,
            PassCondition = checkAllMaxResultJudgements(4),
        });

        [Test]
        public void TestHitTwoNotesWithinShortPeriod()
        {
            const double hit_time = 1;

            var beatmap = new Beatmap<TaikoHitObject>
            {
                HitObjects = new List<TaikoHitObject>
                {
                    new Hit
                    {
                        Type = HitType.Rim,
                        StartTime = hit_time,
                    },
                    new Hit
                    {
                        Type = HitType.Centre,
                        StartTime = hit_time * 2,
                    },
                },
                BeatmapInfo =
                {
                    Difficulty = new BeatmapDifficulty
                    {
                        SliderTickRate = 4,
                        OverallDifficulty = 0,
                    },
                    Ruleset = new TaikoRuleset().RulesetInfo
                },
            };

            beatmap.ControlPointInfo.Add(0, new EffectControlPoint { ScrollSpeed = 0.1f });

            CreateModTest(new ModTestData
            {
                Mod = new TaikoModHidden(),
                Autoplay = true,
                PassCondition = checkAllMaxResultJudgements(2),
                Beatmap = beatmap,
            });
        }
    }
}
