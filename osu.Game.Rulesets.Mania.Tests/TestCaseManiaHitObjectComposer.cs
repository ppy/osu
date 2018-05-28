// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Edit;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Screens.Edit.Screens.Compose;
using System;
using System.Collections.Generic;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests
{
    [TestFixture]
    public class TestCaseManiaHitObjectComposer : HitObjectComposerTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(HitObjectComposer),
            typeof(ManiaHitObjectComposer),
        };

        protected override HitObjectComposer CreateComposer()
        {
            var m = new ManiaHitObjectComposer(new ManiaRuleset(), new BindableBeatDivisor());

            AddStep("change beat snap divisor to 1/1", () => m.BeatDivisor.Value = 1);
            AddStep("change beat snap divisor to 1/2", () => m.BeatDivisor.Value = 2);
            AddStep("change beat snap divisor to 1/4", () => m.BeatDivisor.Value = 4);
            AddStep("change beat snap divisor to 1/8", () => m.BeatDivisor.Value = 8);
            AddStep("change beat snap divisor to 1/16", () => m.BeatDivisor.Value = 16);
            AddStep("change beat snap divisor to 1/3", () => m.BeatDivisor.Value = 3);
            AddStep("change beat snap divisor to 1/6", () => m.BeatDivisor.Value = 6);
            AddStep("change beat snap divisor to 1/12", () => m.BeatDivisor.Value = 12);

            return m;
        }

        protected override IBeatmap CreateBeatmap()
        {
            return new ManiaBeatmap(new StageDefinition { Columns = 4 })
            {
                HitObjects = new List<ManiaHitObject>
                {
                    new Note { Column = 2, StartTime = 100 },
                    new Note { Column = 3, StartTime = 150 },
                    new HoldNote
                    {
                        Column = 1,
                        StartTime = 200,
                        Duration = 150,
                    }
                },
                ControlPointInfo =
                {
                    TimingPoints =
                    {
                        new TimingControlPoint { Time = 0, BeatLength = 400 }
                    }
                },
                BeatmapInfo = { Ruleset = new ManiaRuleset().RulesetInfo }
            };
        }
    }
}
