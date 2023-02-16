// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Database;

namespace osu.Game.Tests.Database
{
    public class LegacyExporterTest
    {
        private TestLegacyExporter? legacyExporter;
        private TemporaryNativeStorage? storage;

        [SetUp]
        public void SetupLegacyExporter()
        {
            storage = new TemporaryNativeStorage("export-storage");
            legacyExporter = new TestLegacyExporter(storage);
        }

        [Test]
        public void ExportFileWithNormalName()
        {
            var exportStorage = storage?.GetStorageForDirectory(@"exports");

            const string filename = "normal file name";
            var item = new TestPathInfo(filename);

            Assert.That(item.FileName.Length < TestLegacyExporter.GetMaxPath(), Is.True);
            Assert.DoesNotThrow(() => legacyExporter?.Export(item));
            Assert.That(exportStorage?.Exists($"{filename}{legacyExporter?.GetExtension()}"), Is.True);
        }

        [Test]
        public void ExportFileWithSuperLongName()
        {
            var exportStorage = storage?.GetStorageForDirectory(@"exports");

            const string fullname = "some file with super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name";

            int capacity = TestLegacyExporter.GetMaxPath() - (legacyExporter?.GetExtension().Length ?? 0);
            string expectedName = fullname.Remove(capacity);

            var item = new TestPathInfo(fullname);

            Assert.That(item.FileName.Length > TestLegacyExporter.GetMaxPath(), Is.True);
            Assert.DoesNotThrow(() => legacyExporter?.Export(item));
            Assert.That(exportStorage?.Exists($"{expectedName}{legacyExporter?.GetExtension()}"), Is.True);
        }

        [TearDown]
        public void CleanupAfterTest()
        {
            storage?.Dispose();
        }

        private class TestPathInfo : IHasNamedFiles
        {
            public string FileName { get; set; }

            public TestPathInfo(string fileName) => FileName = fileName;

            public IEnumerable<INamedFileUsage> Files { get; set; } = new List<INamedFileUsage>();

            public override string ToString() => FileName;
        }

        private class TestLegacyExporter : LegacyExporter<IHasNamedFiles>
        {
            public TestLegacyExporter(Storage storage)
                : base(storage)
            {
            }

            public static int GetMaxPath() => MAX_PATH;

            public string GetExtension() => FileExtension;

            protected override string FileExtension => ".ots";
        }
    }
}
