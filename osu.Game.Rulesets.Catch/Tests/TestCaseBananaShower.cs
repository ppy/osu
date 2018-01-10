// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    [Ignore("getting CI working")]
    public class TestCaseBananaShower : Game.Tests.Visual.TestCasePlayer
    {
        public TestCaseBananaShower()
            : base(typeof(CatchRuleset))
        {
        }

        protected override Beatmap CreateBeatmap()
        {
            var beatmap = new Beatmap
            {
                BeatmapInfo = new BeatmapInfo
                {
                    BaseDifficulty = new BeatmapDifficulty
                    {
                        CircleSize = 6,
                    }
                }
            };

            for (int i = 0; i < 10; i++)
                beatmap.HitObjects.Add(new BananaShower { StartTime = i * 1200, Duration = 1000, NewCombo = i % 2 == 0 });

            return beatmap;
        }
    }
}
