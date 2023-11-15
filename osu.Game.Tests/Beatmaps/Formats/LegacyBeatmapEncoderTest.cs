// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Formats;
using osu.Game.Beatmaps.Legacy;
using osu.Game.IO;
using osu.Game.IO.Serialization;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Taiko;
using osu.Game.Skinning;
using osu.Game.Tests.Resources;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Tests.Beatmaps.Formats
{
    [TestFixture]
    public class LegacyBeatmapEncoderTest
    {
        private static readonly DllResourceStore beatmaps_resource_store = TestResources.GetStore();

        private static IEnumerable<string> allBeatmaps = beatmaps_resource_store.GetAvailableResources().Where(res => res.EndsWith(".osu", StringComparison.Ordinal));

        [TestCaseSource(nameof(allBeatmaps))]
        public void TestEncodeDecodeStability(string name)
        {
            var decoded = decodeFromLegacy(beatmaps_resource_store.GetStream(name), name);
            var decodedAfterEncode = decodeFromLegacy(encodeToLegacy(decoded), name);

            sort(decoded.beatmap);
            sort(decodedAfterEncode.beatmap);

            compareBeatmaps(decoded, decodedAfterEncode);
        }

        [TestCaseSource(nameof(allBeatmaps))]
        public void TestEncodeDecodeStabilityDoubleConvert(string name)
        {
            var decoded = decodeFromLegacy(beatmaps_resource_store.GetStream(name), name);
            var decodedAfterEncode = decodeFromLegacy(encodeToLegacy(decoded), name);

            // run an extra convert. this is expected to be stable.
            decodedAfterEncode.beatmap = convert(decodedAfterEncode.beatmap);

            sort(decoded.beatmap);
            sort(decodedAfterEncode.beatmap);

            compareBeatmaps(decoded, decodedAfterEncode);
        }

        [TestCaseSource(nameof(allBeatmaps))]
        public void TestEncodeDecodeStabilityWithNonLegacyControlPoints(string name)
        {
            var decoded = decodeFromLegacy(beatmaps_resource_store.GetStream(name), name);

            // we are testing that the transfer of relevant data to hitobjects (from legacy control points) sticks through encode/decode.
            // before the encode step, the legacy information is removed here.
            decoded.beatmap.ControlPointInfo = removeLegacyControlPointTypes(decoded.beatmap.ControlPointInfo);

            var decodedAfterEncode = decodeFromLegacy(encodeToLegacy(decoded), name);

            compareBeatmaps(decoded, decodedAfterEncode);

            ControlPointInfo removeLegacyControlPointTypes(ControlPointInfo controlPointInfo)
            {
                // emulate non-legacy control points by cloning the non-legacy portion.
                // the assertion is that the encoder can recreate this losslessly from hitobject data.
                Assert.IsInstanceOf<LegacyControlPointInfo>(controlPointInfo);

                var newControlPoints = new ControlPointInfo();

                foreach (var point in controlPointInfo.AllControlPoints)
                {
                    // completely ignore "legacy" types, which have been moved to HitObjects.
                    // even though these would mostly be ignored by the Add call, they will still be available in groups,
                    // which isn't what we want to be testing here.
                    if (point is SampleControlPoint || point is DifficultyControlPoint)
                        continue;

                    newControlPoints.Add(point.Time, point.DeepClone());
                }

                return newControlPoints;
            }
        }

        private void compareBeatmaps((IBeatmap beatmap, TestLegacySkin skin) expected, (IBeatmap beatmap, TestLegacySkin skin) actual)
        {
            // Check all control points that are still considered to be at a global level.
            Assert.That(expected.beatmap.ControlPointInfo.TimingPoints.Serialize(), Is.EqualTo(actual.beatmap.ControlPointInfo.TimingPoints.Serialize()));
            Assert.That(expected.beatmap.ControlPointInfo.EffectPoints.Serialize(), Is.EqualTo(actual.beatmap.ControlPointInfo.EffectPoints.Serialize()));

            // Check all hitobjects.
            Assert.That(expected.beatmap.HitObjects.Serialize(), Is.EqualTo(actual.beatmap.HitObjects.Serialize()));

            // Check skin.
            Assert.IsTrue(areComboColoursEqual(expected.skin.Configuration, actual.skin.Configuration));
        }

        [Test]
        public void TestEncodeMultiSegmentSliderWithFloatingPointError()
        {
            var beatmap = new Beatmap
            {
                HitObjects =
                {
                    new Slider
                    {
                        Position = new Vector2(0.6f),
                        Path = new SliderPath(new[]
                        {
                            new PathControlPoint(Vector2.Zero, PathType.Bezier),
                            new PathControlPoint(new Vector2(0.5f)),
                            new PathControlPoint(new Vector2(0.51f)), // This is actually on the same position as the previous one in legacy beatmaps (truncated to int).
                            new PathControlPoint(new Vector2(1f), PathType.Bezier),
                            new PathControlPoint(new Vector2(2f))
                        })
                    },
                }
            };

            var decodedAfterEncode = decodeFromLegacy(encodeToLegacy((beatmap, new TestLegacySkin(beatmaps_resource_store, string.Empty))), string.Empty);
            var decodedSlider = (Slider)decodedAfterEncode.beatmap.HitObjects[0];
            Assert.That(decodedSlider.Path.ControlPoints.Count, Is.EqualTo(5));
        }

        private bool areComboColoursEqual(IHasComboColours a, IHasComboColours b)
        {
            // equal to null, no need to SequenceEqual
            if (a.ComboColours == null && b.ComboColours == null)
                return true;

            if (a.ComboColours == null || b.ComboColours == null)
                return false;

            return a.ComboColours.SequenceEqual(b.ComboColours);
        }

        private void sort(IBeatmap beatmap)
        {
            // Sort control points to ensure a sane ordering, as they may be parsed in different orders. This works because each group contains only uniquely-typed control points.
            foreach (var g in beatmap.ControlPointInfo.Groups)
            {
                ArrayList.Adapter((IList)g.ControlPoints).Sort(
                    Comparer<ControlPoint>.Create((c1, c2) => string.Compare(c1.GetType().ToString(), c2.GetType().ToString(), StringComparison.Ordinal)));
            }
        }

        private (IBeatmap beatmap, TestLegacySkin skin) decodeFromLegacy(Stream stream, string name)
        {
            using (var reader = new LineBufferedReader(stream))
            {
                var beatmap = new LegacyBeatmapDecoder { ApplyOffsets = false }.Decode(reader);
                var beatmapSkin = new TestLegacySkin(beatmaps_resource_store, name);
                return (convert(beatmap), beatmapSkin);
            }
        }

        private class TestLegacySkin : LegacySkin
        {
            public TestLegacySkin(IResourceStore<byte[]> storage, string fileName)
                : base(new SkinInfo { Name = "Test Skin", Creator = "Craftplacer" }, null, storage, fileName)
            {
            }
        }

        private MemoryStream encodeToLegacy((IBeatmap beatmap, ISkin skin) fullBeatmap)
        {
            var (beatmap, beatmapSkin) = fullBeatmap;
            var stream = new MemoryStream();

            using (var writer = new StreamWriter(stream, Encoding.UTF8, 1024, true))
                new LegacyBeatmapEncoder(beatmap, beatmapSkin).Encode(writer);

            stream.Position = 0;

            return stream;
        }

        private IBeatmap convert(IBeatmap beatmap)
        {
            switch (beatmap.BeatmapInfo.Ruleset.OnlineID)
            {
                case 0:
                    beatmap.BeatmapInfo.Ruleset = new OsuRuleset().RulesetInfo;
                    break;

                case 1:
                    beatmap.BeatmapInfo.Ruleset = new TaikoRuleset().RulesetInfo;
                    break;

                case 2:
                    beatmap.BeatmapInfo.Ruleset = new CatchRuleset().RulesetInfo;
                    break;

                case 3:
                    beatmap.BeatmapInfo.Ruleset = new ManiaRuleset().RulesetInfo;
                    break;
            }

            return new TestWorkingBeatmap(beatmap).GetPlayableBeatmap(beatmap.BeatmapInfo.Ruleset);
        }

        private class TestWorkingBeatmap : WorkingBeatmap
        {
            private readonly IBeatmap beatmap;

            public TestWorkingBeatmap(IBeatmap beatmap)
                : base(beatmap.BeatmapInfo, null)
            {
                this.beatmap = beatmap;
            }

            protected override IBeatmap GetBeatmap() => beatmap;

            public override Texture GetBackground() => throw new NotImplementedException();

            protected override Track GetBeatmapTrack() => throw new NotImplementedException();

            protected internal override ISkin GetSkin() => throw new NotImplementedException();

            public override Stream GetStream(string storagePath) => throw new NotImplementedException();
        }
    }
}
