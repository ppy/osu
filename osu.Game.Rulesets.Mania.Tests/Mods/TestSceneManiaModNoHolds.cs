// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Tests.Visual;
using osu.Game.Rulesets.Mania.Tests;
using System.Collections.Generic;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Mania.Beatmaps;

namespace osu.Game.Rulesets.Mania.Tests.Mods
{
    public class TestSceneManiaModNoHolds : ModTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new ManiaRuleset();

        [Test]
        public void TestMapHasNoHeldNotes()
        {
            var testBeatmap = createBeatmap();
            Assert.That(!testBeatmap.HitObjects.OfType<HoldNote>().Any());
        }


        private static IBeatmap createBeatmap()
        {
            var beatmap = createRawBeatmap();
            var noHoldsMod = new ManiaModNoHolds();

            foreach (var hitObject in beatmap.HitObjects)
                hitObject.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            noHoldsMod.ApplyToBeatmap(beatmap);

            return beatmap;
        }
        private static IBeatmap createRawBeatmap()
        {
            var beatmap = new ManiaBeatmap(new StageDefinition { Columns = 1 });
            beatmap.HitObjects.Add(new Note { StartTime = 1000 });
            beatmap.HitObjects.Add(new HoldNote { StartTime = 1000, EndTime = 3000 });
            return beatmap;
        }
    }
}
