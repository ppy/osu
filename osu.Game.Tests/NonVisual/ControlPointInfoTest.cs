// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Tests.NonVisual
{
    [TestFixture]
    public class ControlPointInfoTest
    {
        [Test]
        public void TestAdd()
        {
            var cpi = new ControlPointInfo();

            cpi.Add(0, new TimingControlPoint());
            cpi.Add(1000, new TimingControlPoint { BeatLength = 500 });

            Assert.That(cpi.Groups.Count, Is.EqualTo(2));
            Assert.That(cpi.TimingPoints.Count, Is.EqualTo(2));
            Assert.That(cpi.AllControlPoints.Count(), Is.EqualTo(2));
        }

        [Test]
        public void TestAddRedundantTiming()
        {
            var cpi = new ControlPointInfo();

            cpi.Add(0, new TimingControlPoint()); // is *not* redundant, special exception for first timing point.
            cpi.Add(1000, new TimingControlPoint()); // is also not redundant, due to change of offset

            Assert.That(cpi.Groups.Count, Is.EqualTo(2));
            Assert.That(cpi.TimingPoints.Count, Is.EqualTo(2));
            Assert.That(cpi.AllControlPoints.Count(), Is.EqualTo(2));

            cpi.Add(1000, new TimingControlPoint()); //is redundant

            Assert.That(cpi.Groups.Count, Is.EqualTo(2));
            Assert.That(cpi.TimingPoints.Count, Is.EqualTo(2));
            Assert.That(cpi.AllControlPoints.Count(), Is.EqualTo(2));
        }

        [Test]
        public void TestAddRedundantDifficulty()
        {
            var cpi = new ControlPointInfo();

            cpi.Add(0, new DifficultyControlPoint()); // is redundant
            cpi.Add(1000, new DifficultyControlPoint()); // is redundant

            Assert.That(cpi.Groups.Count, Is.EqualTo(0));
            Assert.That(cpi.DifficultyPoints.Count, Is.EqualTo(0));
            Assert.That(cpi.AllControlPoints.Count(), Is.EqualTo(0));

            cpi.Add(1000, new DifficultyControlPoint { SpeedMultiplier = 2 }); // is not redundant

            Assert.That(cpi.Groups.Count, Is.EqualTo(1));
            Assert.That(cpi.DifficultyPoints.Count, Is.EqualTo(1));
            Assert.That(cpi.AllControlPoints.Count(), Is.EqualTo(1));
        }

        [Test]
        public void TestAddRedundantSample()
        {
            var cpi = new ControlPointInfo();

            cpi.Add(0, new SampleControlPoint()); // is *not* redundant, special exception for first sample point
            cpi.Add(1000, new SampleControlPoint()); // is redundant

            Assert.That(cpi.Groups.Count, Is.EqualTo(1));
            Assert.That(cpi.SamplePoints.Count, Is.EqualTo(1));
            Assert.That(cpi.AllControlPoints.Count(), Is.EqualTo(1));

            cpi.Add(1000, new SampleControlPoint { SampleVolume = 50 }); // is not redundant

            Assert.That(cpi.Groups.Count, Is.EqualTo(2));
            Assert.That(cpi.SamplePoints.Count, Is.EqualTo(2));
            Assert.That(cpi.AllControlPoints.Count(), Is.EqualTo(2));
        }

        [Test]
        public void TestAddRedundantEffect()
        {
            var cpi = new ControlPointInfo();

            cpi.Add(0, new EffectControlPoint()); // is redundant
            cpi.Add(1000, new EffectControlPoint()); // is redundant

            Assert.That(cpi.Groups.Count, Is.EqualTo(0));
            Assert.That(cpi.EffectPoints.Count, Is.EqualTo(0));
            Assert.That(cpi.AllControlPoints.Count(), Is.EqualTo(0));

            cpi.Add(1000, new EffectControlPoint { KiaiMode = true, OmitFirstBarLine = true }); // is not redundant
            cpi.Add(1400, new EffectControlPoint { KiaiMode = true, OmitFirstBarLine = true }); // same settings, but is not redundant

            Assert.That(cpi.Groups.Count, Is.EqualTo(2));
            Assert.That(cpi.EffectPoints.Count, Is.EqualTo(2));
            Assert.That(cpi.AllControlPoints.Count(), Is.EqualTo(2));
        }

        [Test]
        public void TestAddGroup()
        {
            var cpi = new ControlPointInfo();

            var group = cpi.GroupAt(1000, true);
            var group2 = cpi.GroupAt(1000, true);

            Assert.That(group, Is.EqualTo(group2));
            Assert.That(cpi.Groups.Count, Is.EqualTo(1));
        }

        [Test]
        public void TestGroupAtLookupOnly()
        {
            var cpi = new ControlPointInfo();

            var group = cpi.GroupAt(5000, true);
            Assert.That(group, Is.Not.Null);

            Assert.That(cpi.Groups.Count, Is.EqualTo(1));
            Assert.That(cpi.GroupAt(1000), Is.Null);
            Assert.That(cpi.GroupAt(5000), Is.Not.Null);
        }

        [Test]
        public void TestAddRemoveGroup()
        {
            var cpi = new ControlPointInfo();

            var group = cpi.GroupAt(1000, true);

            Assert.That(cpi.Groups.Count, Is.EqualTo(1));

            cpi.RemoveGroup(group);

            Assert.That(cpi.Groups.Count, Is.EqualTo(0));
        }

        [Test]
        public void TestAddControlPointToGroup()
        {
            var cpi = new ControlPointInfo();

            var group = cpi.GroupAt(1000, true);
            Assert.That(cpi.Groups.Count, Is.EqualTo(1));

            // usually redundant, but adding to group forces it to be added
            group.Add(new DifficultyControlPoint());

            Assert.That(group.ControlPoints.Count, Is.EqualTo(1));
            Assert.That(cpi.DifficultyPoints.Count, Is.EqualTo(1));
        }

        [Test]
        public void TestAddDuplicateControlPointToGroup()
        {
            var cpi = new ControlPointInfo();

            var group = cpi.GroupAt(1000, true);
            Assert.That(cpi.Groups.Count, Is.EqualTo(1));

            group.Add(new DifficultyControlPoint());
            group.Add(new DifficultyControlPoint { SpeedMultiplier = 2 });

            Assert.That(group.ControlPoints.Count, Is.EqualTo(1));
            Assert.That(cpi.DifficultyPoints.Count, Is.EqualTo(1));
            Assert.That(cpi.DifficultyPoints.First().SpeedMultiplier, Is.EqualTo(2));
        }

        [Test]
        public void TestRemoveControlPointFromGroup()
        {
            var cpi = new ControlPointInfo();

            var group = cpi.GroupAt(1000, true);
            Assert.That(cpi.Groups.Count, Is.EqualTo(1));

            var difficultyPoint = new DifficultyControlPoint();

            group.Add(difficultyPoint);
            group.Remove(difficultyPoint);

            Assert.That(group.ControlPoints.Count, Is.EqualTo(0));
            Assert.That(cpi.DifficultyPoints.Count, Is.EqualTo(0));
            Assert.That(cpi.AllControlPoints.Count, Is.EqualTo(0));
        }

        [Test]
        public void TestOrdering()
        {
            var cpi = new ControlPointInfo();

            cpi.Add(0, new TimingControlPoint());
            cpi.Add(1000, new TimingControlPoint { BeatLength = 500 });
            cpi.Add(10000, new TimingControlPoint { BeatLength = 200 });
            cpi.Add(5000, new TimingControlPoint { BeatLength = 100 });
            cpi.Add(3000, new DifficultyControlPoint { SpeedMultiplier = 2 });
            cpi.GroupAt(7000, true).Add(new DifficultyControlPoint { SpeedMultiplier = 4 });
            cpi.GroupAt(1000).Add(new SampleControlPoint { SampleVolume = 0 });
            cpi.GroupAt(8000, true).Add(new EffectControlPoint { KiaiMode = true });

            Assert.That(cpi.AllControlPoints.Count, Is.EqualTo(8));

            Assert.That(cpi.Groups, Is.Ordered.Ascending.By(nameof(ControlPointGroup.Time)));

            Assert.That(cpi.AllControlPoints, Is.Ordered.Ascending.By(nameof(ControlPoint.Time)));
            Assert.That(cpi.TimingPoints, Is.Ordered.Ascending.By(nameof(ControlPoint.Time)));
        }

        [Test]
        public void TestClear()
        {
            var cpi = new ControlPointInfo();

            cpi.Add(0, new TimingControlPoint());
            cpi.Add(1000, new TimingControlPoint { BeatLength = 500 });
            cpi.Add(10000, new TimingControlPoint { BeatLength = 200 });
            cpi.Add(5000, new TimingControlPoint { BeatLength = 100 });
            cpi.Add(3000, new DifficultyControlPoint { SpeedMultiplier = 2 });
            cpi.GroupAt(7000, true).Add(new DifficultyControlPoint { SpeedMultiplier = 4 });
            cpi.GroupAt(1000).Add(new SampleControlPoint { SampleVolume = 0 });
            cpi.GroupAt(8000, true).Add(new EffectControlPoint { KiaiMode = true });

            cpi.Clear();

            Assert.That(cpi.Groups.Count, Is.EqualTo(0));
            Assert.That(cpi.DifficultyPoints.Count, Is.EqualTo(0));
            Assert.That(cpi.AllControlPoints.Count, Is.EqualTo(0));
        }
    }
}
