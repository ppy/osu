// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using System.Collections.Generic;

namespace osu.Desktop.Tests.Visual
{
    internal class TestCaseBreakPeriods : TestCasePlayer
    {
        public override string Description => @"Testing break-in/break-out behavior.";

        protected override Beatmap TestBeatmap()
        {
            var objects = new List<HitObject>
            {
                new HitCircle
                {
                    StartTime = 7000,
                    Position = new Vector2(OsuPlayfield.BASE_SIZE.X / 2, OsuPlayfield.BASE_SIZE.Y / 2),
                },
                new HitCircle
                {
                    StartTime = 30000,
                    Position = new Vector2(OsuPlayfield.BASE_SIZE.X / 2, OsuPlayfield.BASE_SIZE.Y / 2),
                },
            };

            var breaks = new List<BreakPeriod>
            {
                //Long break
                new BreakPeriod
                {
                    StartTime = 8000,
                    EndTime = 14000,
                },
                //Short break
                new BreakPeriod
                {
                    StartTime = 17000,
                    EndTime = 18000,
                },
            };

            return new Beatmap
            {
                HitObjects = objects,
                Breaks = breaks,
                BeatmapInfo = new BeatmapInfo
                {
                    Difficulty = new BeatmapDifficulty(),
                    Ruleset = Rulesets.Query<RulesetInfo>().First(),
                    Metadata = new BeatmapMetadata
                    {
                        Artist = @"Unknown",
                        Title = @"Sample Beatmap",
                        Author = @"peppy",
                    }
                }
            };
        }
    }
}
