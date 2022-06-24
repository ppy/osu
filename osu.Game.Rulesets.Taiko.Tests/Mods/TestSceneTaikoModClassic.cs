// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Taiko.Beatmaps;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Replays;

namespace osu.Game.Rulesets.Taiko.Tests.Mods
{
    public class TestSceneTaikoModClassic : TaikoModTestScene
    {
        [Test]
        public void TestHittingDrumRollsIsOptional() => CreateModTest(new ModTestData
        {
            Mod = new TaikoModClassic(),
            Autoplay = false,
            Beatmap = new TaikoBeatmap
            {
                BeatmapInfo = { Ruleset = CreatePlayerRuleset().RulesetInfo },
                HitObjects = new List<TaikoHitObject>
                {
                    new Hit
                    {
                        StartTime = 1000,
                        Type = HitType.Centre
                    },
                    new DrumRoll
                    {
                        StartTime = 3000,
                        EndTime = 6000
                    }
                }
            },
            ReplayFrames = new List<ReplayFrame>
            {
                new TaikoReplayFrame(1000, TaikoAction.LeftCentre),
                new TaikoReplayFrame(1001)
            },
            PassCondition = () => Player.ScoreProcessor.HasCompleted.Value
                                  && Player.ScoreProcessor.Combo.Value == 1
                                  && Player.ScoreProcessor.Accuracy.Value == 1
        });

        [Test]
        public void TestHittingSwellsIsOptional() => CreateModTest(new ModTestData
        {
            Mod = new TaikoModClassic(),
            Autoplay = false,
            Beatmap = new TaikoBeatmap
            {
                BeatmapInfo = { Ruleset = CreatePlayerRuleset().RulesetInfo },
                HitObjects = new List<TaikoHitObject>
                {
                    new Hit
                    {
                        StartTime = 1000,
                        Type = HitType.Centre
                    },
                    new Swell
                    {
                        StartTime = 3000,
                        EndTime = 6000
                    }
                }
            },
            ReplayFrames = new List<ReplayFrame>
            {
                new TaikoReplayFrame(1000, TaikoAction.LeftCentre),
                new TaikoReplayFrame(1001)
            },
            PassCondition = () => Player.ScoreProcessor.HasCompleted.Value
                                  && Player.ScoreProcessor.Combo.Value == 1
                                  && Player.ScoreProcessor.Accuracy.Value == 1
        });
    }
}
