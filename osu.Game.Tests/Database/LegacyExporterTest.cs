// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Database;

namespace osu.Game.Tests.Database
{
    [TestFixture]
    public class LegacyExporterTest
    {
        private TestLegacyExporter legacyExporter = null!;
        private TemporaryNativeStorage storage = null!;

        private const string long_filename =
            "some file with super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name";

        [SetUp]
        public void SetUp()
        {
            storage = new TemporaryNativeStorage("export-storage");
            legacyExporter = new TestLegacyExporter(storage);
        }

        [Test]
        public void ExportFileWithNormalNameTest()
        {
            const string filename = "normal file name";
            var item = new TestPathInfo(filename);

            Assert.That(item.Filename.Length, Is.LessThan(TestLegacyExporter.MAX_FILENAME_LENGTH));
            exportItemAndAssert(item, filename);
        }

        [Test]
        public void ExportFileWithNormalNameMultipleTimesTest()
        {
            const string filename = "normal file name";
            var item = new TestPathInfo(filename);

            Assert.That(item.Filename.Length < TestLegacyExporter.MAX_FILENAME_LENGTH, Is.True);

            //Export multiple times
            for (int i = 0; i < 10; i++)
            {
                string expectedFileName = i == 0 ? filename : $"{filename} ({i})";
                exportItemAndAssert(item, expectedFileName);
            }
        }

        [Test]
        public void ExportFileWithSuperLongNameTest()
        {
            int expectedLength = TestLegacyExporter.MAX_FILENAME_LENGTH - (legacyExporter.GetExtension().Length);
            string expectedName = long_filename.Remove(expectedLength);

            var item = new TestPathInfo(long_filename);

            Assert.That(item.Filename.Length > TestLegacyExporter.MAX_FILENAME_LENGTH, Is.True);
            exportItemAndAssert(item, expectedName);
        }

        [Test]
        public void ExportFileWithSuperLongNameMultipleTimesTest()
        {
            int expectedLength = TestLegacyExporter.MAX_FILENAME_LENGTH - (legacyExporter.GetExtension().Length);
            string expectedName = long_filename.Remove(expectedLength);

            var item = new TestPathInfo(long_filename);

            Assert.That(item.Filename.Length > TestLegacyExporter.MAX_FILENAME_LENGTH, Is.True);

            //Export multiple times
            for (int i = 0; i < 10; i++)
                exportItemAndAssert(item, expectedName);
        }

        private void exportItemAndAssert(IHasNamedFiles item, string expectedName)
        {
            Assert.DoesNotThrow(() => legacyExporter.Export(item));
            Assert.That(storage.Exists($"exports/{expectedName}{legacyExporter.GetExtension()}"), Is.True);
        }

        [TearDown]
        public void TearDown()
        {
            if (storage.IsNotNull())
                storage.Dispose();
        }

        private class TestPathInfo : IHasNamedFiles
        {
            public string Filename { get; }

            public IEnumerable<INamedFileUsage> Files { get; } = new List<INamedFileUsage>();

            public TestPathInfo(string filename)
            {
                Filename = filename;
            }

            public override string ToString() => Filename;
        }

        private class TestLegacyExporter : LegacyExporter<IHasNamedFiles>
        {
            public TestLegacyExporter(Storage storage)
                : base(storage)
            {
            }

            public string GetExtension() => FileExtension;

            protected override string FileExtension => ".ots";
        }
    }
}
