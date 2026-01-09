// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Game.IO;

namespace osu.Game.Tests.Database
{
    [TestFixture]
    public class DiskUsageTests
    {
        private string tempDir = null!;

        [SetUp]
        public void SetUp()
        {
            // Create a temporary directory to ensure we are testing against a valid location
            tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }

        [Test]
        public void TestSufficientSpace()
        {
            // Asking for 0 bytes should always succeed (unless the drive is 100% full)
            Assert.DoesNotThrow(() => DiskUsage.EnsureSufficientSpace(tempDir, 0));
        }

        [Test]
        public void TestInsufficientSpace()
        {
            // Asking for the maximum possible long value should always exceed available space
            Assert.Throws<IOException>(() => DiskUsage.EnsureSufficientSpace(tempDir, long.MaxValue));
        }

        [Test]
        public void TestNonExistentDirectory()
        {
            string nonExistentPath = Path.Combine(tempDir, "does_not_exist");
            Assert.Throws<DirectoryNotFoundException>(() => DiskUsage.EnsureSufficientSpace(nonExistentPath));
        }

        [Test]
        public async Task TestAsyncWrapper()
        {
            await DiskUsage.EnsureSufficientSpaceAsync(tempDir, 0);
        }
    }
}
