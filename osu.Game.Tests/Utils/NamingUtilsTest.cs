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
        public void TestNextBestNameEmptySet()
        {
            string nextBestName = NamingUtils.GetNextBestName(Enumerable.Empty<string>(), "New Difficulty");

            Assert.AreEqual("New Difficulty", nextBestName);
        }

        [Test]
        public void TestNextBestNameNotTaken()
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
        public void TestNextBestNameNotTakenButClose()
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
        public void TestNextBestNameAlreadyTaken()
        {
            string[] existingNames =
            {
                "New Difficulty"
            };

            string nextBestName = NamingUtils.GetNextBestName(existingNames, "New Difficulty");

            Assert.AreEqual("New Difficulty (1)", nextBestName);
        }

        [Test]
        public void TestNextBestNameAlreadyTakenWithDifferentCase()
        {
            string[] existingNames =
            {
                "new difficulty"
            };

            string nextBestName = NamingUtils.GetNextBestName(existingNames, "New Difficulty");

            Assert.AreEqual("New Difficulty (1)", nextBestName);
        }

        [Test]
        public void TestNextBestNameAlreadyTakenWithBrackets()
        {
            string[] existingNames =
            {
                "new difficulty (copy)"
            };

            string nextBestName = NamingUtils.GetNextBestName(existingNames, "New Difficulty (copy)");

            Assert.AreEqual("New Difficulty (copy) (1)", nextBestName);
        }

        [Test]
        public void TestNextBestNameMultipleAlreadyTaken()
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
        public void TestNextBestNameEvenMoreAlreadyTaken()
        {
            string[] existingNames = Enumerable.Range(1, 30).Select(i => $"New Difficulty ({i})").Append("New Difficulty").ToArray();

            string nextBestName = NamingUtils.GetNextBestName(existingNames, "New Difficulty");

            Assert.AreEqual("New Difficulty (31)", nextBestName);
        }

        [Test]
        public void TestNextBestNameMultipleAlreadyTakenWithGaps()
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

        [Test]
        public void TestNextBestFilenameEmptySet()
        {
            string nextBestFilename = NamingUtils.GetNextBestFilename(Enumerable.Empty<string>(), "test_file.osr");

            Assert.AreEqual("test_file.osr", nextBestFilename);
        }

        [Test]
        public void TestNextBestFilenameNotTaken()
        {
            string[] existingFiles =
            {
                "this file exists.zip",
                "that file exists.too",
                "three.4",
            };

            string nextBestFilename = NamingUtils.GetNextBestFilename(existingFiles, "test_file.osr");

            Assert.AreEqual("test_file.osr", nextBestFilename);
        }

        [Test]
        public void TestNextBestFilenameNotTakenButClose()
        {
            string[] existingFiles =
            {
                "replay_file(1).osr",
                "replay_file (not a number).zip",
                "replay_file (1 <- now THAT is a number right here).lol",
            };

            string nextBestFilename = NamingUtils.GetNextBestFilename(existingFiles, "replay_file.osr");

            Assert.AreEqual("replay_file.osr", nextBestFilename);
        }

        [Test]
        public void TestNextBestFilenameAlreadyTaken()
        {
            string[] existingFiles =
            {
                "replay_file.osr",
            };

            string nextBestFilename = NamingUtils.GetNextBestFilename(existingFiles, "replay_file.osr");

            Assert.AreEqual("replay_file (1).osr", nextBestFilename);
        }

        [Test]
        public void TestNextBestFilenameAlreadyTakenDifferentCase()
        {
            string[] existingFiles =
            {
                "replay_file.osr",
                "RePlAy_FiLe (1).OsR",
                "REPLAY_FILE (2).OSR",
            };

            string nextBestFilename = NamingUtils.GetNextBestFilename(existingFiles, "replay_file.osr");
            Assert.AreEqual("replay_file (3).osr", nextBestFilename);
        }

        [Test]
        public void TestNextBestFilenameAlreadyTakenWithBrackets()
        {
            string[] existingFiles =
            {
                "replay_file.osr",
                "replay_file (copy).osr",
            };

            string nextBestFilename = NamingUtils.GetNextBestFilename(existingFiles, "replay_file.osr");
            Assert.AreEqual("replay_file (1).osr", nextBestFilename);

            nextBestFilename = NamingUtils.GetNextBestFilename(existingFiles, "replay_file (copy).osr");
            Assert.AreEqual("replay_file (copy) (1).osr", nextBestFilename);
        }

        [Test]
        public void TestNextBestFilenameMultipleAlreadyTaken()
        {
            string[] existingFiles =
            {
                "replay_file.osr",
                "replay_file (1).osr",
                "replay_file (2).osr",
                "replay_file (3).osr",
            };

            string nextBestFilename = NamingUtils.GetNextBestFilename(existingFiles, "replay_file.osr");

            Assert.AreEqual("replay_file (4).osr", nextBestFilename);
        }

        [Test]
        public void TestNextBestFilenameMultipleAlreadyTakenWithGaps()
        {
            string[] existingFiles =
            {
                "replay_file.osr",
                "replay_file (1).osr",
                "replay_file (2).osr",
                "replay_file (4).osr",
                "replay_file (5).osr",
            };

            string nextBestFilename = NamingUtils.GetNextBestFilename(existingFiles, "replay_file.osr");

            Assert.AreEqual("replay_file (3).osr", nextBestFilename);
        }

        [Test]
        public void TestNextBestFilenameNoExtensions()
        {
            string[] existingFiles =
            {
                "those",
                "are definitely",
                "files",
            };

            string nextBestFilename = NamingUtils.GetNextBestFilename(existingFiles, "surely");
            Assert.AreEqual("surely", nextBestFilename);

            nextBestFilename = NamingUtils.GetNextBestFilename(existingFiles, "those");
            Assert.AreEqual("those (1)", nextBestFilename);
        }

        [Test]
        public void TestNextBestFilenameDifferentExtensions()
        {
            string[] existingFiles =
            {
                "replay_file.osr",
                "replay_file (1).osr",
                "replay_file.txt",
            };

            string nextBestFilename = NamingUtils.GetNextBestFilename(existingFiles, "replay_file.osr");
            Assert.AreEqual("replay_file (2).osr", nextBestFilename);

            nextBestFilename = NamingUtils.GetNextBestFilename(existingFiles, "replay_file.txt");
            Assert.AreEqual("replay_file (1).txt", nextBestFilename);
        }
    }
}
