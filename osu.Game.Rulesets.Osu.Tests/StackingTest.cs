// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.IO;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Tests.Beatmaps;
using Decoder = osu.Game.Beatmaps.Formats.Decoder;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class StackingTest
    {
        [Test]
        public void TestStacking()
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(beatmap_data)))
            using (var reader = new LineBufferedReader(stream))
            {
                var beatmap = Decoder.GetDecoder<Beatmap>(reader).Decode(reader);
                var converted = new TestWorkingBeatmap(beatmap).GetPlayableBeatmap(new OsuRuleset().RulesetInfo, Array.Empty<Mod>());

                var objects = converted.HitObjects.ToList();

                // The last hitobject triggers the stacking
                for (int i = 0; i < objects.Count - 1; i++)
                    Assert.AreEqual(0, ((OsuHitObject)objects[i]).StackHeight);
            }
        }

        private const string beatmap_data = @"
osu file format v14

[General]
StackLeniency: 0.2

[Difficulty]
ApproachRate:9.2
SliderMultiplier:1
SliderTickRate:0.5

[TimingPoints]
217871,6400,4,2,1,20,1,0
217871,-800,4,2,1,20,0,0
218071,-787.5,4,2,1,20,0,0
218271,-775,4,2,1,20,0,0
218471,-762.5,4,2,1,20,0,0
218671,-750,4,2,1,20,0,0
240271,-10,4,2,0,5,0,0

[HitObjects]
311,185,217871,6,0,L|318:158,1,25
311,185,218071,2,0,L|335:170,1,25
311,185,218271,2,0,L|338:192,1,25
311,185,218471,2,0,L|325:209,1,25
311,185,218671,2,0,L|304:212,1,25
311,185,240271,5,0,0:0:0:0:
";
    }
}
