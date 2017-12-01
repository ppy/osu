using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    [Ignore("getting CI working")]
    public class TestCaseHyperdash : Game.Tests.Visual.TestCasePlayer
    {
        public TestCaseHyperdash()
            : base(typeof(CatchRuleset))
        {
        }

        protected override Beatmap CreateBeatmap()
        {
            var beatmap = new Beatmap();

            for (int i = 0; i < 512; i++)
                beatmap.HitObjects.Add(new Fruit { X = i % 8 < 4 ? 0.02f : 0.98f, StartTime = i * 100, NewCombo = i % 8 == 0 });

            return beatmap;
        }
    }
}
