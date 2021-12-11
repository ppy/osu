// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Scoring;

namespace osu.Game.Tests.Scores.IO
{
    [TestFixture]
    public class TestScoreEquality
    {
        [Test]
        public void TestNonMatchingByReference()
        {
            ScoreInfo score1 = new ScoreInfo();
            ScoreInfo score2 = new ScoreInfo();

            Assert.That(score1, Is.Not.EqualTo(score2));
        }

        [Test]
        public void TestMatchingByReference()
        {
            ScoreInfo score = new ScoreInfo();

            Assert.That(score, Is.EqualTo(score));
        }

        [Test]
        public void TestNonMatchingByPrimaryKey()
        {
            ScoreInfo score1 = new ScoreInfo { ID = 1 };
            ScoreInfo score2 = new ScoreInfo { ID = 2 };

            Assert.That(score1, Is.Not.EqualTo(score2));
        }

        [Test]
        public void TestMatchingByPrimaryKey()
        {
            ScoreInfo score1 = new ScoreInfo { ID = 1 };
            ScoreInfo score2 = new ScoreInfo { ID = 1 };

            Assert.That(score1, Is.EqualTo(score2));
        }

        [Test]
        public void TestNonMatchingByNull()
        {
            ScoreInfo score = new ScoreInfo();

            Assert.That(score, Is.Not.EqualTo(null));
        }
    }
}
