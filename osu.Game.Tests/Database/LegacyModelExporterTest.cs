// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Database;
using osu.Game.Overlays.Notifications;
using Realms;

namespace osu.Game.Tests.Database
{
    [TestFixture]
    public class LegacyModelExporterTest
    {
        private TestLegacyModelExporter legacyExporter = null!;
        private TemporaryNativeStorage storage = null!;

        private const string short_filename = "normal file name";

        private const string long_filename =
            "some file with super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name super long name";

        [SetUp]
        public void SetUp()
        {
            storage = new TemporaryNativeStorage("export-storage");
            legacyExporter = new TestLegacyModelExporter(storage);
        }

        [Test]
        public void ExportFileWithNormalNameTest()
        {
            var item = new TestModel(short_filename);

            Assert.That(item.Filename.Length, Is.LessThan(TestLegacyModelExporter.MAX_FILENAME_LENGTH));

            exportItemAndAssert(item, short_filename);
        }

        [Test]
        public void ExportFileWithNormalNameMultipleTimesTest()
        {
            var item = new TestModel(short_filename);

            Assert.That(item.Filename.Length, Is.LessThan(TestLegacyModelExporter.MAX_FILENAME_LENGTH));

            //Export multiple times
            for (int i = 0; i < 100; i++)
            {
                string expectedFileName = i == 0 ? short_filename : $"{short_filename} ({i})";
                exportItemAndAssert(item, expectedFileName);
            }
        }

        [Test]
        public void ExportFileWithSuperLongNameTest()
        {
            int expectedLength = TestLegacyModelExporter.MAX_FILENAME_LENGTH - (legacyExporter.GetExtension().Length);
            string expectedName = long_filename.Remove(expectedLength);

            var item = new TestModel(long_filename);

            Assert.That(item.Filename.Length, Is.GreaterThan(TestLegacyModelExporter.MAX_FILENAME_LENGTH));
            exportItemAndAssert(item, expectedName);
        }

        [Test]
        public void ExportFileWithSuperLongNameMultipleTimesTest()
        {
            int expectedLength = TestLegacyModelExporter.MAX_FILENAME_LENGTH - (legacyExporter.GetExtension().Length);
            string expectedName = long_filename.Remove(expectedLength);

            var item = new TestModel(long_filename);

            Assert.That(item.Filename.Length, Is.GreaterThan(TestLegacyModelExporter.MAX_FILENAME_LENGTH));

            //Export multiple times
            for (int i = 0; i < 100; i++)
            {
                string expectedFilename = i == 0 ? expectedName : $"{expectedName} ({i})";
                exportItemAndAssert(item, expectedFilename);
            }
        }

        [Test]
        public void ExportFileRetryWhenInterruptedByCollectionChange()
        {
            var item = new TestModel("hi mom");

            Assert.That(item.Filename.Length, Is.LessThan(TestLegacyModelExporter.MAX_FILENAME_LENGTH));

            var live = new RealmLiveUnmanaged<TestModel>(item);

            Assert.DoesNotThrow(() =>
            {
                var export = Task.Run(() => legacyExporter.ExportAsync(live));
                var exportInterruptor = Task.Run(() => live.Value.Files = []);

                Task.WhenAll([export, exportInterruptor]).WaitSafely();
            });

            Assert.That(storage.Exists($"exports/{item.Filename}{legacyExporter.GetExtension()}"), Is.True);
        }

        private void exportItemAndAssert(TestModel item, string expectedName)
        {
            Assert.DoesNotThrow(() =>
            {
                Task.Run(() => legacyExporter.ExportAsync(new RealmLiveUnmanaged<TestModel>(item))).WaitSafely();
            });
            Assert.That(storage.Exists($"exports/{expectedName}{legacyExporter.GetExtension()}"), Is.True);
        }

        [TearDown]
        public void TearDown()
        {
            if (storage.IsNotNull())
                storage.Dispose();
        }

        private class TestLegacyModelExporter : LegacyExporter<TestModel>
        {
            public TestLegacyModelExporter(Storage storage)
                : base(storage)
            {
            }

            public string GetExtension() => FileExtension;

            public override void ExportToStream(TestModel model, Stream outputStream, ProgressNotification? notification, CancellationToken cancellationToken = default)
            {
            }

            protected override string FileExtension => ".test";
        }

        private class TestModel : RealmObject, IHasNamedFiles, IHasGuidPrimaryKey
        {
            public Guid ID => Guid.Empty;

            public string Filename { get; }

            public IEnumerable<INamedFileUsage> Files { get; set; } = new List<INamedFileUsage>();

            public TestModel(string filename)
            {
                Filename = filename;
            }

            public override string ToString() => Filename;
        }
    }
}
