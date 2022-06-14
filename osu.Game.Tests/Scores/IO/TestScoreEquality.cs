// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
            ScoreInfo score1 = new ScoreInfo { ID = Guid.NewGuid() };
            ScoreInfo score2 = new ScoreInfo { ID = Guid.NewGuid() };

            Assert.That(score1, Is.Not.EqualTo(score2));
        }

        [Test]
        public void TestMatchingByPrimaryKey()
        {
            Guid id = Guid.NewGuid();

            ScoreInfo score1 = new ScoreInfo { ID = id };
            ScoreInfo score2 = new ScoreInfo { ID = id };

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
