// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Replays;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests
{
    public class TestCaseAutoGeneration : OsuTestCase
    {
        [Test]
        public void TestSingleNote()
        {
            // |   |
            // | - |
            // |   |

            var beatmap = new Beatmap<ManiaHitObject>();
            beatmap.HitObjects.Add(new Note { StartTime = 1000 });

            var generated = new ManiaAutoGenerator(beatmap).Generate();

            Assert.IsTrue(generated.Frames.Count == 3, "Replay must have 3 frames");
            Assert.AreEqual(1000, generated.Frames[1].Time, "Incorrect hit time");
            Assert.AreEqual(1000 + ManiaAutoGenerator.RELEASE_DELAY, generated.Frames[2].Time, "Incorrect release time");
            Assert.AreEqual(1, generated.Frames[1].MouseX, "Key 0 has not been pressed");
            Assert.AreEqual(0, generated.Frames[2].MouseX, "Key 0 has not been released");
        }

        [Test]
        public void TestSingleHoldNote()
        {
            // |   |
            // | * |
            // | * |
            // | * |
            // |   |

            var beatmap = new Beatmap<ManiaHitObject>();
            beatmap.HitObjects.Add(new HoldNote { StartTime = 1000, Duration = 2000 });

            var generated = new ManiaAutoGenerator(beatmap).Generate();

            Assert.IsTrue(generated.Frames.Count == 3, "Replay must have 3 frames");
            Assert.AreEqual(1000, generated.Frames[1].Time, "Incorrect hit time");
            Assert.AreEqual(3000 + ManiaAutoGenerator.RELEASE_DELAY, generated.Frames[2].Time, "Incorrect release time");
            Assert.AreEqual(1, generated.Frames[1].MouseX, "Key 0 has not been pressed");
            Assert.AreEqual(0, generated.Frames[2].MouseX, "Key 0 has not been released");
        }

        [Test]
        public void TestSingleNoteChord()
        {
            // |   |   |
            // | - | - |
            // |   |   |

            var beatmap = new Beatmap<ManiaHitObject>();
            beatmap.HitObjects.Add(new Note { StartTime = 1000 });
            beatmap.HitObjects.Add(new Note { StartTime = 1000, Column = 1 });

            var generated = new ManiaAutoGenerator(beatmap).Generate();

            Assert.IsTrue(generated.Frames.Count == 3, "Replay must have 3 frames");
            Assert.AreEqual(1000, generated.Frames[1].Time, "Incorrect hit time");
            Assert.AreEqual(1000 + ManiaAutoGenerator.RELEASE_DELAY, generated.Frames[2].Time, "Incorrect release time");
            Assert.AreEqual(3, generated.Frames[1].MouseX, "Keys 1 and 2 have not been pressed");
            Assert.AreEqual(0, generated.Frames[2].MouseX, "Keys 1 and 2 have not been released");
        }

        [Test]
        public void TestHoldNoteChord()
        {
            // |   |   |
            // | * | * |
            // | * | * |
            // | * | * |
            // |   |   |

            var beatmap = new Beatmap<ManiaHitObject>();
            beatmap.HitObjects.Add(new HoldNote { StartTime = 1000, Duration = 2000 });
            beatmap.HitObjects.Add(new HoldNote { StartTime = 1000, Duration = 2000, Column = 1 });

            var generated = new ManiaAutoGenerator(beatmap).Generate();

            Assert.IsTrue(generated.Frames.Count == 3, "Replay must have 3 frames");
            Assert.AreEqual(1000, generated.Frames[1].Time, "Incorrect hit time");
            Assert.AreEqual(3000 + ManiaAutoGenerator.RELEASE_DELAY, generated.Frames[2].Time, "Incorrect release time");
            Assert.AreEqual(3, generated.Frames[1].MouseX, "Keys 1 and 2 have not been pressed");
            Assert.AreEqual(0, generated.Frames[2].MouseX, "Keys 1 and 2 have not been released");
        }

        [Test]
        public void TestSingleNoteStair()
        {
            // |   |   |
            // |   | - |
            // | - |   |
            // |   |   |

            var beatmap = new Beatmap<ManiaHitObject>();
            beatmap.HitObjects.Add(new Note { StartTime = 1000 });
            beatmap.HitObjects.Add(new Note { StartTime = 2000, Column = 1 });

            var generated = new ManiaAutoGenerator(beatmap).Generate();

            Assert.IsTrue(generated.Frames.Count == 5, "Replay must have 5 frames");
            Assert.AreEqual(1000, generated.Frames[1].Time, "Incorrect first note hit time");
            Assert.AreEqual(1000 + ManiaAutoGenerator.RELEASE_DELAY, generated.Frames[2].Time, "Incorrect first note release time");
            Assert.AreEqual(2000, generated.Frames[3].Time, "Incorrect second note hit time");
            Assert.AreEqual(2000 + ManiaAutoGenerator.RELEASE_DELAY, generated.Frames[4].Time, "Incorrect second note release time");
            Assert.AreEqual(1, generated.Frames[1].MouseX, "Key 1 has not been pressed");
            Assert.AreEqual(0, generated.Frames[2].MouseX, "Key 1 has not been released");
            Assert.AreEqual(2, generated.Frames[3].MouseX, "Key 2 has not been pressed");
            Assert.AreEqual(0, generated.Frames[4].MouseX, "Key 2 has not been released");
        }

        [Test]
        public void TestHoldNoteStair()
        {
            // |   |   |
            // |   | * |
            // | * | * |
            // | * | * |
            // | * |   |
            // |   |   |

            var beatmap = new Beatmap<ManiaHitObject>();
            beatmap.HitObjects.Add(new HoldNote { StartTime = 1000, Duration = 2000 });
            beatmap.HitObjects.Add(new HoldNote { StartTime = 2000, Duration = 2000, Column = 1 });

            var generated = new ManiaAutoGenerator(beatmap).Generate();

            Assert.IsTrue(generated.Frames.Count == 5, "Replay must have 5 frames");
            Assert.AreEqual(1000, generated.Frames[1].Time, "Incorrect first note hit time");
            Assert.AreEqual(3000 + ManiaAutoGenerator.RELEASE_DELAY, generated.Frames[3].Time, "Incorrect first note release time");
            Assert.AreEqual(2000, generated.Frames[2].Time, "Incorrect second note hit time");
            Assert.AreEqual(4000 + ManiaAutoGenerator.RELEASE_DELAY, generated.Frames[4].Time, "Incorrect second note release time");
            Assert.AreEqual(1, generated.Frames[1].MouseX, "Key 1 has not been pressed");
            Assert.AreEqual(3, generated.Frames[2].MouseX, "Keys 1 and 2 have not been pressed");
            Assert.AreEqual(2, generated.Frames[3].MouseX, "Key 1 has not been released");
            Assert.AreEqual(0, generated.Frames[4].MouseX, "Key 2 has not been released");
        }

        [Test]
        public void TestHoldNoteWithReleasePress()
        {
            // |   |   |
            // | * | - |
            // | * |   |
            // | * |   |
            // |   |   |

            var beatmap = new Beatmap<ManiaHitObject>();
            beatmap.HitObjects.Add(new HoldNote { StartTime = 1000, Duration = 2000 - ManiaAutoGenerator.RELEASE_DELAY });
            beatmap.HitObjects.Add(new Note { StartTime = 3000, Column = 1 });

            var generated = new ManiaAutoGenerator(beatmap).Generate();

            Assert.IsTrue(generated.Frames.Count == 4, "Replay must have 4 frames");
            Assert.AreEqual(1000, generated.Frames[1].Time, "Incorrect first note hit time");
            Assert.AreEqual(3000, generated.Frames[2].Time, "Incorrect second note press time + first note release time");
            Assert.AreEqual(3000 + ManiaAutoGenerator.RELEASE_DELAY, generated.Frames[3].Time, "Incorrect second note release time");
            Assert.AreEqual(1, generated.Frames[1].MouseX, "Key 1 has not been pressed");
            Assert.AreEqual(2, generated.Frames[2].MouseX, "Key 1 has not been released or key 2 has not been pressed");
            Assert.AreEqual(0, generated.Frames[3].MouseX, "Keys 1 and 2 have not been released");
        }
    }
}
