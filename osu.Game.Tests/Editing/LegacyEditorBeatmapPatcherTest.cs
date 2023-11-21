// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.IO;
using System.Text;
using NUnit.Framework;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osuTK;
using Decoder = osu.Game.Beatmaps.Formats.Decoder;

namespace osu.Game.Tests.Editing
{
    [TestFixture]
    public class LegacyEditorBeatmapPatcherTest
    {
        private LegacyEditorBeatmapPatcher patcher;
        private EditorBeatmap current;

        [SetUp]
        public void Setup()
        {
            patcher = new LegacyEditorBeatmapPatcher(current = new EditorBeatmap(new OsuBeatmap
            {
                BeatmapInfo =
                {
                    Ruleset = new OsuRuleset().RulesetInfo
                }
            }));
        }

        [Test]
        public void TestPatchNoObjectChanges()
        {
            runTest(new OsuBeatmap());
        }

        [Test]
        public void TestAddHitObject()
        {
            var patch = new OsuBeatmap
            {
                HitObjects =
                {
                    new HitCircle { StartTime = 1000, NewCombo = true }
                }
            };

            runTest(patch);
        }

        [Test]
        public void TestInsertHitObject()
        {
            current.AddRange(new[]
            {
                new HitCircle { StartTime = 1000, NewCombo = true },
                new HitCircle { StartTime = 3000 },
            });

            var patch = new OsuBeatmap
            {
                HitObjects =
                {
                    (OsuHitObject)current.HitObjects[0],
                    new HitCircle { StartTime = 2000 },
                    (OsuHitObject)current.HitObjects[1],
                }
            };

            runTest(patch);
        }

        [Test]
        public void TestDeleteHitObject()
        {
            current.AddRange(new[]
            {
                new HitCircle { StartTime = 1000, NewCombo = true },
                new HitCircle { StartTime = 2000 },
                new HitCircle { StartTime = 3000 },
            });

            var patch = new OsuBeatmap
            {
                HitObjects =
                {
                    (OsuHitObject)current.HitObjects[0],
                    (OsuHitObject)current.HitObjects[2],
                }
            };

            runTest(patch);
        }

        [Test]
        public void TestChangeStartTime()
        {
            current.AddRange(new[]
            {
                new HitCircle { StartTime = 1000, NewCombo = true },
                new HitCircle { StartTime = 2000 },
                new HitCircle { StartTime = 3000 },
            });

            var patch = new OsuBeatmap
            {
                HitObjects =
                {
                    new HitCircle { StartTime = 500, NewCombo = true },
                    (OsuHitObject)current.HitObjects[1],
                    (OsuHitObject)current.HitObjects[2],
                }
            };

            runTest(patch);
        }

        [Test]
        public void TestChangeSample()
        {
            current.AddRange(new[]
            {
                new HitCircle { StartTime = 1000, NewCombo = true },
                new HitCircle { StartTime = 2000 },
                new HitCircle { StartTime = 3000 },
            });

            var patch = new OsuBeatmap
            {
                HitObjects =
                {
                    (OsuHitObject)current.HitObjects[0],
                    new HitCircle { StartTime = 2000, Samples = { new HitSampleInfo(HitSampleInfo.HIT_FINISH) } },
                    (OsuHitObject)current.HitObjects[2],
                }
            };

            runTest(patch);
        }

        [Test]
        public void TestChangeSliderPath()
        {
            current.AddRange(new OsuHitObject[]
            {
                new HitCircle { StartTime = 1000, NewCombo = true },
                new Slider
                {
                    StartTime = 2000,
                    Path = new SliderPath(new[]
                    {
                        new PathControlPoint(Vector2.Zero),
                        new PathControlPoint(Vector2.One),
                        new PathControlPoint(new Vector2(2), PathType.BEZIER),
                        new PathControlPoint(new Vector2(3)),
                    }, 50)
                },
                new HitCircle { StartTime = 3000 },
            });

            var patch = new OsuBeatmap
            {
                HitObjects =
                {
                    (OsuHitObject)current.HitObjects[0],
                    new Slider
                    {
                        StartTime = 2000,
                        Path = new SliderPath(new[]
                        {
                            new PathControlPoint(Vector2.Zero, PathType.BEZIER),
                            new PathControlPoint(new Vector2(4)),
                            new PathControlPoint(new Vector2(5)),
                        }, 100)
                    },
                    (OsuHitObject)current.HitObjects[2],
                }
            };

            runTest(patch);
        }

        [Test]
        public void TestAddMultipleHitObjects()
        {
            current.AddRange(new[]
            {
                new HitCircle { StartTime = 1000, NewCombo = true },
                new HitCircle { StartTime = 2000 },
                new HitCircle { StartTime = 3000 },
            });

            var patch = new OsuBeatmap
            {
                HitObjects =
                {
                    new HitCircle { StartTime = 500, NewCombo = true },
                    (OsuHitObject)current.HitObjects[0],
                    new HitCircle { StartTime = 1500 },
                    (OsuHitObject)current.HitObjects[1],
                    new HitCircle { StartTime = 2250 },
                    new HitCircle { StartTime = 2500 },
                    (OsuHitObject)current.HitObjects[2],
                    new HitCircle { StartTime = 3500 },
                }
            };

            runTest(patch);
        }

        [Test]
        public void TestDeleteMultipleHitObjects()
        {
            current.AddRange(new[]
            {
                new HitCircle { StartTime = 500, NewCombo = true },
                new HitCircle { StartTime = 1000 },
                new HitCircle { StartTime = 1500 },
                new HitCircle { StartTime = 2000 },
                new HitCircle { StartTime = 2250 },
                new HitCircle { StartTime = 2500 },
                new HitCircle { StartTime = 3000 },
                new HitCircle { StartTime = 3500 },
            });

            var patchedFirst = (HitCircle)current.HitObjects[1];
            patchedFirst.NewCombo = true;

            var patch = new OsuBeatmap
            {
                HitObjects =
                {
                    (OsuHitObject)current.HitObjects[1],
                    (OsuHitObject)current.HitObjects[3],
                    (OsuHitObject)current.HitObjects[6],
                }
            };

            runTest(patch);
        }

        [Test]
        public void TestChangeSamplesOfMultipleHitObjects()
        {
            current.AddRange(new[]
            {
                new HitCircle { StartTime = 500, NewCombo = true },
                new HitCircle { StartTime = 1000 },
                new HitCircle { StartTime = 1500 },
                new HitCircle { StartTime = 2000 },
                new HitCircle { StartTime = 2250 },
                new HitCircle { StartTime = 2500 },
                new HitCircle { StartTime = 3000 },
                new HitCircle { StartTime = 3500 },
            });

            var patch = new OsuBeatmap
            {
                HitObjects =
                {
                    (OsuHitObject)current.HitObjects[0],
                    new HitCircle { StartTime = 1000, Samples = { new HitSampleInfo(HitSampleInfo.HIT_FINISH) } },
                    (OsuHitObject)current.HitObjects[2],
                    (OsuHitObject)current.HitObjects[3],
                    new HitCircle { StartTime = 2250, Samples = { new HitSampleInfo(HitSampleInfo.HIT_WHISTLE) } },
                    (OsuHitObject)current.HitObjects[5],
                    new HitCircle { StartTime = 3000, Samples = { new HitSampleInfo(HitSampleInfo.HIT_CLAP) } },
                    (OsuHitObject)current.HitObjects[7],
                }
            };

            runTest(patch);
        }

        [Test]
        public void TestAddAndDeleteHitObjects()
        {
            current.AddRange(new[]
            {
                new HitCircle { StartTime = 500, NewCombo = true },
                new HitCircle { StartTime = 1000 },
                new HitCircle { StartTime = 1500 },
                new HitCircle { StartTime = 2000 },
                new HitCircle { StartTime = 2250 },
                new HitCircle { StartTime = 2500 },
                new HitCircle { StartTime = 3000 },
                new HitCircle { StartTime = 3500 },
            });

            var patch = new OsuBeatmap
            {
                HitObjects =
                {
                    new HitCircle { StartTime = 750, NewCombo = true },
                    (OsuHitObject)current.HitObjects[1],
                    (OsuHitObject)current.HitObjects[4],
                    (OsuHitObject)current.HitObjects[5],
                    new HitCircle { StartTime = 2650 },
                    new HitCircle { StartTime = 2750 },
                    new HitCircle { StartTime = 4000 },
                }
            };

            runTest(patch);
        }

        [Test]
        public void TestChangeHitObjectAtSameTime()
        {
            current.AddRange(new[]
            {
                new HitCircle { StartTime = 500, Position = new Vector2(50), NewCombo = true },
                new HitCircle { StartTime = 500, Position = new Vector2(100), NewCombo = true },
                new HitCircle { StartTime = 500, Position = new Vector2(150), NewCombo = true },
                new HitCircle { StartTime = 500, Position = new Vector2(200), NewCombo = true },
            });

            var patch = new OsuBeatmap
            {
                HitObjects =
                {
                    new HitCircle { StartTime = 500, Position = new Vector2(150), NewCombo = true },
                    new HitCircle { StartTime = 500, Position = new Vector2(100), NewCombo = true },
                    new HitCircle { StartTime = 500, Position = new Vector2(50), NewCombo = true },
                    new HitCircle { StartTime = 500, Position = new Vector2(200), NewCombo = true },
                }
            };

            runTest(patch);
        }

        private void runTest(IBeatmap patch)
        {
            // Due to the method of testing, "patch" comes in without having been decoded via a beatmap decoder.
            // This causes issues because the decoder adds various default properties (e.g. new combo on first object, default samples).
            // To resolve "patch" into a sane state it is encoded and then re-decoded.
            patch = decode(encode(patch));

            // Apply the patch.
            patcher.Patch(encode(current), encode(patch));

            // Convert beatmaps to strings for assertion purposes.
            string currentStr = Encoding.ASCII.GetString(encode(current));
            string patchStr = Encoding.ASCII.GetString(encode(patch));

            Assert.That(currentStr, Is.EqualTo(patchStr));
        }

        private byte[] encode(IBeatmap beatmap)
        {
            using (var encoded = new MemoryStream())
            {
                using (var sw = new StreamWriter(encoded))
                    new LegacyBeatmapEncoder(beatmap, null).Encode(sw);

                return encoded.ToArray();
            }
        }

        private IBeatmap decode(byte[] state)
        {
            using (var stream = new MemoryStream(state))
            using (var reader = new LineBufferedReader(stream))
                return Decoder.GetDecoder<Beatmap>(reader).Decode(reader);
        }
    }
}
