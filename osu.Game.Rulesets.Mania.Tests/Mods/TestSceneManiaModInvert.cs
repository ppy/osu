// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests.Mods
{
    public partial class TestSceneManiaModInvert : ModTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new ManiaRuleset();

        [Test]
        public void TestInversion() => CreateModTest(new ModTestData
        {
            Mod = new ManiaModInvert(),
            PassCondition = () => Player.ScoreProcessor.JudgedHits >= 2
        });

        [Test]
        public void TestBreaksPreservedOnOriginalBeatmap()
        {
            var beatmap = CreateBeatmap(new ManiaRuleset().RulesetInfo);
            beatmap.Breaks.Clear();
            beatmap.Breaks.Add(new BreakPeriod(0, 1000));

            var workingBeatmap = new FlatWorkingBeatmap(beatmap);

            var playableWithInvert = workingBeatmap.GetPlayableBeatmap(new ManiaRuleset().RulesetInfo, new[] { new ManiaModInvert() });
            Assert.That(playableWithInvert.Breaks.Count, Is.Zero);

            var playableWithoutInvert = workingBeatmap.GetPlayableBeatmap(new ManiaRuleset().RulesetInfo);
            Assert.That(playableWithoutInvert.Breaks.Count, Is.Not.Zero);
            Assert.That(playableWithoutInvert.Breaks[0], Is.EqualTo(new BreakPeriod(0, 1000)));
        }
    }
}
