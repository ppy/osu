// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Utils;

namespace osu.Game.Tests.Utils
{
    [TestFixture]
    public class NamingUtilsTest
    {
        [Test]
        public void TestEmptySet()
        {
            string nextBestName = NamingUtils.GetNextBestName(Enumerable.Empty<string>(), "New Difficulty");

            Assert.AreEqual("New Difficulty", nextBestName);
        }

        [Test]
        public void TestNotTaken()
        {
            string[] existingNames =
            {
                "Something",
                "Entirely",
                "Different"
            };

            string nextBestName = NamingUtils.GetNextBestName(existingNames, "New Difficulty");

            Assert.AreEqual("New Difficulty", nextBestName);
        }

        [Test]
        public void TestNotTakenButClose()
        {
            string[] existingNames =
            {
                "New Difficulty(1)",
                "New Difficulty (abcd)",
                "New Difficulty but not really"
            };

            string nextBestName = NamingUtils.GetNextBestName(existingNames, "New Difficulty");

            Assert.AreEqual("New Difficulty", nextBestName);
        }

        [Test]
        public void TestAlreadyTaken()
        {
            string[] existingNames =
            {
                "New Difficulty"
            };

            string nextBestName = NamingUtils.GetNextBestName(existingNames, "New Difficulty");

            Assert.AreEqual("New Difficulty (1)", nextBestName);
        }

        [Test]
        public void TestAlreadyTakenWithDifferentCase()
        {
            string[] existingNames =
            {
                "new difficulty"
            };

            string nextBestName = NamingUtils.GetNextBestName(existingNames, "New Difficulty");

            Assert.AreEqual("New Difficulty (1)", nextBestName);
        }

        [Test]
        public void TestAlreadyTakenWithBrackets()
        {
            string[] existingNames =
            {
                "new difficulty (copy)"
            };

            string nextBestName = NamingUtils.GetNextBestName(existingNames, "New Difficulty (copy)");

            Assert.AreEqual("New Difficulty (copy) (1)", nextBestName);
        }

        [Test]
        public void TestMultipleAlreadyTaken()
        {
            string[] existingNames =
            {
                "New Difficulty",
                "New difficulty (1)",
                "new Difficulty (2)",
                "New DIFFICULTY (3)"
            };

            string nextBestName = NamingUtils.GetNextBestName(existingNames, "New Difficulty");

            Assert.AreEqual("New Difficulty (4)", nextBestName);
        }

        [Test]
        public void TestEvenMoreAlreadyTaken()
        {
            string[] existingNames = Enumerable.Range(1, 30).Select(i => $"New Difficulty ({i})").Append("New Difficulty").ToArray();

            string nextBestName = NamingUtils.GetNextBestName(existingNames, "New Difficulty");

            Assert.AreEqual("New Difficulty (31)", nextBestName);
        }

        [Test]
        public void TestMultipleAlreadyTakenWithGaps()
        {
            string[] existingNames =
            {
                "New Difficulty",
                "New Difficulty (1)",
                "New Difficulty (4)",
                "New Difficulty (9)"
            };

            string nextBestName = NamingUtils.GetNextBestName(existingNames, "New Difficulty");

            Assert.AreEqual("New Difficulty (2)", nextBestName);
        }
    }
}
