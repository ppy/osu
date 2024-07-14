// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Legacy;

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

            cpi.Add(1000, new TimingControlPoint()); // is redundant

            Assert.That(cpi.Groups.Count, Is.EqualTo(2));
            Assert.That(cpi.TimingPoints.Count, Is.EqualTo(2));
            Assert.That(cpi.AllControlPoints.Count(), Is.EqualTo(2));

            cpi.Add(1200, new TimingControlPoint { OmitFirstBarLine = true }); // is not redundant

            Assert.That(cpi.Groups.Count, Is.EqualTo(3));
            Assert.That(cpi.TimingPoints.Count, Is.EqualTo(3));
            Assert.That(cpi.AllControlPoints.Count(), Is.EqualTo(3));

            cpi.Add(1500, new TimingControlPoint { OmitFirstBarLine = true }); // is not redundant

            Assert.That(cpi.Groups.Count, Is.EqualTo(4));
            Assert.That(cpi.TimingPoints.Count, Is.EqualTo(4));
            Assert.That(cpi.AllControlPoints.Count(), Is.EqualTo(4));
        }

        [Test]
        public void TestAddRedundantDifficulty()
        {
            var cpi = new LegacyControlPointInfo();

            cpi.Add(0, new DifficultyControlPoint()); // is redundant
            cpi.Add(1000, new DifficultyControlPoint()); // is redundant

            Assert.That(cpi.Groups.Count, Is.EqualTo(0));
            Assert.That(cpi.DifficultyPoints.Count, Is.EqualTo(0));
            Assert.That(cpi.AllControlPoints.Count(), Is.EqualTo(0));

            cpi.Add(1000, new DifficultyControlPoint { SliderVelocity = 2 }); // is not redundant

            Assert.That(cpi.Groups.Count, Is.EqualTo(1));
            Assert.That(cpi.DifficultyPoints.Count, Is.EqualTo(1));
            Assert.That(cpi.AllControlPoints.Count(), Is.EqualTo(1));
        }

        [Test]
        public void TestAddRedundantSample()
        {
            var cpi = new LegacyControlPointInfo();

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

            cpi.Add(1000, new EffectControlPoint { KiaiMode = true }); // is not redundant
            cpi.Add(1400, new EffectControlPoint { KiaiMode = true }); // is redundant

            Assert.That(cpi.Groups.Count, Is.EqualTo(1));
            Assert.That(cpi.EffectPoints.Count, Is.EqualTo(1));
            Assert.That(cpi.AllControlPoints.Count(), Is.EqualTo(1));
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
        public void TestRemoveGroupAlsoRemovedControlPoints()
        {
            var cpi = new LegacyControlPointInfo();

            var group = cpi.GroupAt(1000, true);

            group.Add(new SampleControlPoint());

            Assert.That(cpi.SamplePoints.Count, Is.EqualTo(1));

            cpi.RemoveGroup(group);

            Assert.That(cpi.SamplePoints.Count, Is.EqualTo(0));
        }

        [Test]
        public void TestAddControlPointToGroup()
        {
            var cpi = new LegacyControlPointInfo();

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
            var cpi = new LegacyControlPointInfo();

            var group = cpi.GroupAt(1000, true);
            Assert.That(cpi.Groups.Count, Is.EqualTo(1));

            group.Add(new DifficultyControlPoint());
            group.Add(new DifficultyControlPoint { SliderVelocity = 2 });

            Assert.That(group.ControlPoints.Count, Is.EqualTo(1));
            Assert.That(cpi.DifficultyPoints.Count, Is.EqualTo(1));
            Assert.That(cpi.DifficultyPoints.First().SliderVelocity, Is.EqualTo(2));
        }

        [Test]
        public void TestRemoveControlPointFromGroup()
        {
            var cpi = new LegacyControlPointInfo();

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
            var cpi = new LegacyControlPointInfo();

            cpi.Add(0, new TimingControlPoint());
            cpi.Add(1000, new TimingControlPoint { BeatLength = 500 });
            cpi.Add(10000, new TimingControlPoint { BeatLength = 200 });
            cpi.Add(5000, new TimingControlPoint { BeatLength = 100 });
            cpi.Add(3000, new DifficultyControlPoint { SliderVelocity = 2 });
            cpi.GroupAt(7000, true).Add(new DifficultyControlPoint { SliderVelocity = 4 });
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
            var cpi = new LegacyControlPointInfo();

            cpi.Add(0, new TimingControlPoint());
            cpi.Add(1000, new TimingControlPoint { BeatLength = 500 });
            cpi.Add(10000, new TimingControlPoint { BeatLength = 200 });
            cpi.Add(5000, new TimingControlPoint { BeatLength = 100 });
            cpi.Add(3000, new DifficultyControlPoint { SliderVelocity = 2 });
            cpi.GroupAt(7000, true).Add(new DifficultyControlPoint { SliderVelocity = 4 });
            cpi.GroupAt(1000).Add(new SampleControlPoint { SampleVolume = 0 });
            cpi.GroupAt(8000, true).Add(new EffectControlPoint { KiaiMode = true });

            cpi.Clear();

            Assert.That(cpi.Groups.Count, Is.EqualTo(0));
            Assert.That(cpi.DifficultyPoints.Count, Is.EqualTo(0));
            Assert.That(cpi.AllControlPoints.Count, Is.EqualTo(0));
        }

        [Test]
        public void TestDeepClone()
        {
            var cpi = new ControlPointInfo();

            cpi.Add(1000, new TimingControlPoint { BeatLength = 500 });

            var cpiCopy = cpi.DeepClone();

            cpiCopy.Add(2000, new TimingControlPoint { BeatLength = 500 });

            Assert.That(cpi.Groups.Count, Is.EqualTo(1));
            Assert.That(cpiCopy.Groups.Count, Is.EqualTo(2));

            Assert.That(cpi.TimingPoints.Count, Is.EqualTo(1));
            Assert.That(cpiCopy.TimingPoints.Count, Is.EqualTo(2));

            Assert.That(cpi.TimingPoints[0], Is.Not.SameAs(cpiCopy.TimingPoints[0]));
            Assert.That(cpi.TimingPoints[0].BeatLengthBindable, Is.Not.SameAs(cpiCopy.TimingPoints[0].BeatLengthBindable));

            Assert.That(cpi.TimingPoints[0].BeatLength, Is.EqualTo(cpiCopy.TimingPoints[0].BeatLength));

            cpi.TimingPoints[0].BeatLength = 800;

            Assert.That(cpi.TimingPoints[0].BeatLength, Is.Not.EqualTo(cpiCopy.TimingPoints[0].BeatLength));
        }
    }
}
