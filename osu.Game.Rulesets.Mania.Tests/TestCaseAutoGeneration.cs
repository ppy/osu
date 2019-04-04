// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Replays;
using osu.Game.Rulesets.Replays;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests
{
    [TestFixture]
    public class TestCaseAutoGeneration : OsuTestCase
    {
        [Test]
        public void TestSingleNote()
        {
            // |   |
            // | - |
            // |   |

            var beatmap = new ManiaBeatmap(new StageDefinition { Columns = 1 });
            beatmap.HitObjects.Add(new Note { StartTime = 1000 });

            var generated = new ManiaAutoGenerator(beatmap).Generate();

            Assert.IsTrue(generated.Frames.Count == 3, "Replay must have 3 frames");
            Assert.AreEqual(1000, generated.Frames[1].Time, "Incorrect hit time");
            Assert.AreEqual(1000 + ManiaAutoGenerator.RELEASE_DELAY, generated.Frames[2].Time, "Incorrect release time");
            Assert.IsTrue(checkContains(generated.Frames[1], ManiaAction.Special1), "Special1 has not been pressed");
            Assert.IsFalse(checkContains(generated.Frames[2], ManiaAction.Special1), "Special1 has not been released");
        }

        [Test]
        public void TestSingleHoldNote()
        {
            // |   |
            // | * |
            // | * |
            // | * |
            // |   |

            var beatmap = new ManiaBeatmap(new StageDefinition { Columns = 1 });
            beatmap.HitObjects.Add(new HoldNote { StartTime = 1000, Duration = 2000 });

            var generated = new ManiaAutoGenerator(beatmap).Generate();

            Assert.IsTrue(generated.Frames.Count == 3, "Replay must have 3 frames");
            Assert.AreEqual(1000, generated.Frames[1].Time, "Incorrect hit time");
            Assert.AreEqual(3000 + ManiaAutoGenerator.RELEASE_DELAY, generated.Frames[2].Time, "Incorrect release time");
            Assert.IsTrue(checkContains(generated.Frames[1], ManiaAction.Special1), "Special1 has not been pressed");
            Assert.IsFalse(checkContains(generated.Frames[2], ManiaAction.Special1), "Special1 has not been released");
        }

        [Test]
        public void TestSingleNoteChord()
        {
            // |   |   |
            // | - | - |
            // |   |   |

            var beatmap = new ManiaBeatmap(new StageDefinition { Columns = 2 });
            beatmap.HitObjects.Add(new Note { StartTime = 1000 });
            beatmap.HitObjects.Add(new Note { StartTime = 1000, Column = 1 });

            var generated = new ManiaAutoGenerator(beatmap).Generate();

            Assert.IsTrue(generated.Frames.Count == 3, "Replay must have 3 frames");
            Assert.AreEqual(1000, generated.Frames[1].Time, "Incorrect hit time");
            Assert.AreEqual(1000 + ManiaAutoGenerator.RELEASE_DELAY, generated.Frames[2].Time, "Incorrect release time");
            Assert.IsTrue(checkContains(generated.Frames[1], ManiaAction.Key1, ManiaAction.Key2), "Key1 & Key2 have not been pressed");
            Assert.IsFalse(checkContains(generated.Frames[2], ManiaAction.Key1, ManiaAction.Key2), "Key1 & Key2 have not been released");
        }

        [Test]
        public void TestHoldNoteChord()
        {
            // |   |   |
            // | * | * |
            // | * | * |
            // | * | * |
            // |   |   |

            var beatmap = new ManiaBeatmap(new StageDefinition { Columns = 2 });
            beatmap.HitObjects.Add(new HoldNote { StartTime = 1000, Duration = 2000 });
            beatmap.HitObjects.Add(new HoldNote { StartTime = 1000, Duration = 2000, Column = 1 });

            var generated = new ManiaAutoGenerator(beatmap).Generate();

            Assert.IsTrue(generated.Frames.Count == 3, "Replay must have 3 frames");
            Assert.AreEqual(1000, generated.Frames[1].Time, "Incorrect hit time");
            Assert.AreEqual(3000 + ManiaAutoGenerator.RELEASE_DELAY, generated.Frames[2].Time, "Incorrect release time");
            Assert.IsTrue(checkContains(generated.Frames[1], ManiaAction.Key1, ManiaAction.Key2), "Key1 & Key2 have not been pressed");
            Assert.IsFalse(checkContains(generated.Frames[2], ManiaAction.Key1, ManiaAction.Key2), "Key1 & Key2 have not been released");
        }

        [Test]
        public void TestSingleNoteStair()
        {
            // |   |   |
            // |   | - |
            // | - |   |
            // |   |   |

            var beatmap = new ManiaBeatmap(new StageDefinition { Columns = 2 });
            beatmap.HitObjects.Add(new Note { StartTime = 1000 });
            beatmap.HitObjects.Add(new Note { StartTime = 2000, Column = 1 });

            var generated = new ManiaAutoGenerator(beatmap).Generate();

            Assert.IsTrue(generated.Frames.Count == 5, "Replay must have 5 frames");
            Assert.AreEqual(1000, generated.Frames[1].Time, "Incorrect first note hit time");
            Assert.AreEqual(1000 + ManiaAutoGenerator.RELEASE_DELAY, generated.Frames[2].Time, "Incorrect first note release time");
            Assert.AreEqual(2000, generated.Frames[3].Time, "Incorrect second note hit time");
            Assert.AreEqual(2000 + ManiaAutoGenerator.RELEASE_DELAY, generated.Frames[4].Time, "Incorrect second note release time");
            Assert.IsTrue(checkContains(generated.Frames[1], ManiaAction.Key1), "Key1 has not been pressed");
            Assert.IsFalse(checkContains(generated.Frames[2], ManiaAction.Key1), "Key1 has not been released");
            Assert.IsTrue(checkContains(generated.Frames[3], ManiaAction.Key2), "Key2 has not been pressed");
            Assert.IsFalse(checkContains(generated.Frames[4], ManiaAction.Key2), "Key2 has not been released");
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

            var beatmap = new ManiaBeatmap(new StageDefinition { Columns = 2 });
            beatmap.HitObjects.Add(new HoldNote { StartTime = 1000, Duration = 2000 });
            beatmap.HitObjects.Add(new HoldNote { StartTime = 2000, Duration = 2000, Column = 1 });

            var generated = new ManiaAutoGenerator(beatmap).Generate();

            Assert.IsTrue(generated.Frames.Count == 5, "Replay must have 5 frames");
            Assert.AreEqual(1000, generated.Frames[1].Time, "Incorrect first note hit time");
            Assert.AreEqual(3000 + ManiaAutoGenerator.RELEASE_DELAY, generated.Frames[3].Time, "Incorrect first note release time");
            Assert.AreEqual(2000, generated.Frames[2].Time, "Incorrect second note hit time");
            Assert.AreEqual(4000 + ManiaAutoGenerator.RELEASE_DELAY, generated.Frames[4].Time, "Incorrect second note release time");
            Assert.IsTrue(checkContains(generated.Frames[1], ManiaAction.Key1), "Key1 has not been pressed");
            Assert.IsTrue(checkContains(generated.Frames[2], ManiaAction.Key1, ManiaAction.Key2), "Key1 & Key2 have not been pressed");
            Assert.IsFalse(checkContains(generated.Frames[3], ManiaAction.Key1), "Key1 has not been released");
            Assert.IsTrue(checkContains(generated.Frames[3], ManiaAction.Key2), "Key2 has been released");
            Assert.IsFalse(checkContains(generated.Frames[4], ManiaAction.Key2), "Key2 has not been released");
        }

        [Test]
        public void TestHoldNoteWithReleasePress()
        {
            // |   |   |
            // | * | - |
            // | * |   |
            // | * |   |
            // |   |   |

            var beatmap = new ManiaBeatmap(new StageDefinition { Columns = 2 });
            beatmap.HitObjects.Add(new HoldNote { StartTime = 1000, Duration = 2000 - ManiaAutoGenerator.RELEASE_DELAY });
            beatmap.HitObjects.Add(new Note { StartTime = 3000, Column = 1 });

            var generated = new ManiaAutoGenerator(beatmap).Generate();

            Assert.IsTrue(generated.Frames.Count == 4, "Replay must have 4 frames");
            Assert.AreEqual(1000, generated.Frames[1].Time, "Incorrect first note hit time");
            Assert.AreEqual(3000, generated.Frames[2].Time, "Incorrect second note press time + first note release time");
            Assert.AreEqual(3000 + ManiaAutoGenerator.RELEASE_DELAY, generated.Frames[3].Time, "Incorrect second note release time");
            Assert.IsTrue(checkContains(generated.Frames[1], ManiaAction.Key1), "Key1 has not been pressed");
            Assert.IsFalse(checkContains(generated.Frames[2], ManiaAction.Key1), "Key1 has not been released");
            Assert.IsTrue(checkContains(generated.Frames[2], ManiaAction.Key2), "Key2 has not been pressed");
            Assert.IsFalse(checkContains(generated.Frames[3], ManiaAction.Key2), "Key2 has not been released");
        }

        private bool checkContains(ReplayFrame frame, params ManiaAction[] actions) => actions.All(action => ((ManiaReplayFrame)frame).Actions.Contains(action));
    }
}
