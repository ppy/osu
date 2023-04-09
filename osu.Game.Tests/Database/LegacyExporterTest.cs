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
            var item = new TestRealmObject(short_filename);

            Assert.That(item.Filename.Length, Is.LessThan(TestLegacyModelExporter.MAX_FILENAME_LENGTH));

            exportItemAndAssert(item, short_filename);
        }

        [Test]
        public void ExportFileWithNormalNameMultipleTimesTest()
        {
            var item = new TestRealmObject(short_filename);

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

            var item = new TestRealmObject(long_filename);

            Assert.That(item.Filename.Length, Is.GreaterThan(TestLegacyModelExporter.MAX_FILENAME_LENGTH));
            exportItemAndAssert(item, expectedName);
        }

        [Test]
        public void ExportFileWithSuperLongNameMultipleTimesTest()
        {
            int expectedLength = TestLegacyModelExporter.MAX_FILENAME_LENGTH - (legacyExporter.GetExtension().Length);
            string expectedName = long_filename.Remove(expectedLength);

            var item = new TestRealmObject(long_filename);

            Assert.That(item.Filename.Length, Is.GreaterThan(TestLegacyModelExporter.MAX_FILENAME_LENGTH));

            //Export multiple times
            for (int i = 0; i < 100; i++)
            {
                string expectedFilename = i == 0 ? expectedName : $"{expectedName} ({i})";
                exportItemAndAssert(item, expectedFilename);
            }
        }

        private void exportItemAndAssert(TestRealmObject item, string expectedName)
        {
            // ReSharper disable once AsyncVoidLambda
            Assert.DoesNotThrow(() =>
            {
                Task t = Task.Run(() => legacyExporter.ExportAsync(new TestRealmLive(item)));
                t.WaitSafely();
            });
            Assert.That(storage.Exists($"exports/{expectedName}{legacyExporter.GetExtension()}"), Is.True);
        }

        [TearDown]
        public void TearDown()
        {
            if (storage.IsNotNull())
                storage.Dispose();
        }

        private class TestLegacyModelExporter : LegacyModelExporter<TestRealmObject>
        {
            public TestLegacyModelExporter(Storage storage)
                : base(storage)
            {
            }

            public string GetExtension() => FileExtension;

            protected override void ExportToStream(TestRealmObject model, Stream outputStream, ProgressNotification? notification, CancellationToken cancellationToken = default)
            {
            }

            protected override string FileExtension => ".test";
        }

        private class TestRealmObject : RealmObject, IHasNamedFiles, IHasGuidPrimaryKey
        {
            public Guid ID => throw new NotImplementedException();
            public string Filename { get; }

            public IEnumerable<INamedFileUsage> Files { get; } = new List<INamedFileUsage>();

            public TestRealmObject(string filename)
            {
                Filename = filename;
            }

            public override string ToString() => Filename;
        }

        private class TestRealmLive : Live<TestRealmObject>
        {
            public override void PerformRead(Action<TestRealmObject> perform) => perform(Value);

            public override TReturn PerformRead<TReturn>(Func<TestRealmObject, TReturn> perform) => perform(Value);

            public override void PerformWrite(Action<TestRealmObject> perform) => throw new NotImplementedException();

            public override bool IsManaged => throw new NotImplementedException();

            public override TestRealmObject Value { get; }

            public TestRealmLive(TestRealmObject model)
                : base(Guid.Empty)
            {
                Value = model;
            }
        }
    }
}
