using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class TestCaseCatchPlayer : Game.Tests.Visual.TestCasePlayer
    {
        protected override Beatmap CreateBeatmap()
        {
            var beatmap = new Beatmap();

            for (int i = 0; i < 256; i++)
                beatmap.HitObjects.Add(new Fruit { X = 0.5f, StartTime = i * 100, NewCombo = i % 8 == 0 });

            return beatmap;
        }
    }
}
