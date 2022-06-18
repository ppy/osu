// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Mods;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects;
using osuTK;

namespace osu.Game.Rulesets.Catch.Tests.Mods
{
    [TestFixture]
    public class CatchModMirrorTest
    {
        [Test]
        public void TestModMirror()
        {
            IBeatmap original = createBeatmap(false);
            IBeatmap mirrored = createBeatmap(true);

            assertEffectivePositionsMirrored(original, mirrored);
        }

        private static IBeatmap createBeatmap(bool withMirrorMod)
        {
            var beatmap = createRawBeatmap();
            var mirrorMod = new CatchModMirror();

            var beatmapProcessor = new CatchBeatmapProcessor(beatmap);
            beatmapProcessor.PreProcess();

            foreach (var hitObject in beatmap.HitObjects)
                hitObject.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            beatmapProcessor.PostProcess();

            if (withMirrorMod)
                mirrorMod.ApplyToBeatmap(beatmap);

            return beatmap;
        }

        private static IBeatmap createRawBeatmap() => new Beatmap
        {
            HitObjects = new List<HitObject>
            {
                new Fruit
                {
                    OriginalX = 150,
                    StartTime = 0
                },
                new Fruit
                {
                    OriginalX = 450,
                    StartTime = 500
                },
                new JuiceStream
                {
                    OriginalX = 250,
                    Path = new SliderPath
                    {
                        ControlPoints =
                        {
                            new PathControlPoint(new Vector2(-100, 1)),
                            new PathControlPoint(new Vector2(0, 2)),
                            new PathControlPoint(new Vector2(100, 3)),
                            new PathControlPoint(new Vector2(0, 4))
                        }
                    },
                    StartTime = 1000,
                },
                new BananaShower
                {
                    StartTime = 5000,
                    Duration = 5000
                }
            }
        };

        private static void assertEffectivePositionsMirrored(IBeatmap original, IBeatmap mirrored)
        {
            if (original.HitObjects.Count != mirrored.HitObjects.Count)
                Assert.Fail($"Top-level object count mismatch (original: {original.HitObjects.Count}, mirrored: {mirrored.HitObjects.Count})");

            for (int i = 0; i < original.HitObjects.Count; ++i)
            {
                var originalObject = (CatchHitObject)original.HitObjects[i];
                var mirroredObject = (CatchHitObject)mirrored.HitObjects[i];

                // banana showers themselves are exempt, as we only really care about their nested bananas' positions.
                if (!effectivePositionMirrored(originalObject, mirroredObject) && !(originalObject is BananaShower))
                    Assert.Fail($"{originalObject.GetType().Name} at time {originalObject.StartTime} is not mirrored ({printEffectivePositions(originalObject, mirroredObject)})");

                if (originalObject.NestedHitObjects.Count != mirroredObject.NestedHitObjects.Count)
                    Assert.Fail($"{originalObject.GetType().Name} nested object count mismatch (original: {originalObject.NestedHitObjects.Count}, mirrored: {mirroredObject.NestedHitObjects.Count})");

                for (int j = 0; j < originalObject.NestedHitObjects.Count; ++j)
                {
                    var originalNested = (CatchHitObject)originalObject.NestedHitObjects[j];
                    var mirroredNested = (CatchHitObject)mirroredObject.NestedHitObjects[j];

                    if (!effectivePositionMirrored(originalNested, mirroredNested))
                        Assert.Fail($"{originalObject.GetType().Name}'s nested {originalNested.GetType().Name} at time {originalObject.StartTime} is not mirrored ({printEffectivePositions(originalNested, mirroredNested)})");
                }
            }
        }

        private static string printEffectivePositions(CatchHitObject original, CatchHitObject mirrored)
            => $"original X: {original.EffectiveX}, mirrored X is: {mirrored.EffectiveX}, mirrored X should be: {CatchPlayfield.WIDTH - original.EffectiveX}";

        private static bool effectivePositionMirrored(CatchHitObject original, CatchHitObject mirrored)
            => Precision.AlmostEquals(original.EffectiveX, CatchPlayfield.WIDTH - mirrored.EffectiveX);
    }
}
