// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
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
        public void TestStackingEdgeCaseOne()
        {
            using (var stream = new MemoryStream(@"
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
"u8.ToArray()))
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

        [Test]
        public void TestStackingEdgeCaseTwo()
        {
            using (var stream = new MemoryStream(@"
osu file format v14
// extracted from https://osu.ppy.sh/beatmapsets/365006#osu/801165

[General]
StackLeniency: 0.2

[Difficulty]
HPDrainRate:6
CircleSize:4
OverallDifficulty:8
ApproachRate:9.3
SliderMultiplier:2
SliderTickRate:1

[TimingPoints]
5338,444.444444444444,4,2,0,50,1,0
82893,-76.9230769230769,4,2,8,50,0,0
85115,-76.9230769230769,4,2,0,50,0,0
85337,-100,4,2,8,60,0,0
85893,-100,4,2,7,60,0,0
86226,-100,4,2,8,60,0,0
88893,-58.8235294117647,4,1,8,70,0,1

[HitObjects]
427,124,84226,1,0,3:0:0:0:
427,124,84337,1,0,3:0:0:0:
427,124,84449,1,8,0:0:0:0:
"u8.ToArray()))
            using (var reader = new LineBufferedReader(stream))
            {
                var beatmap = Decoder.GetDecoder<Beatmap>(reader).Decode(reader);
                var converted = new TestWorkingBeatmap(beatmap).GetPlayableBeatmap(new OsuRuleset().RulesetInfo, Array.Empty<Mod>());

                var objects = converted.HitObjects.ToList();

                Assert.That(objects, Has.Count.EqualTo(3));

                // The last hitobject triggers the stacking
                for (int i = 0; i < objects.Count - 1; i++)
                    Assert.AreEqual(0, ((OsuHitObject)objects[i]).StackHeight);
            }
        }
    }
}
