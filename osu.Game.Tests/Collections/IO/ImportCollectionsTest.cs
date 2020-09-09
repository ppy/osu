// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Game.Collections;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Collections.IO
{
    [TestFixture]
    public class ImportCollectionsTest
    {
        [Test]
        public async Task TestImportEmptyDatabase()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost())
            {
                try
                {
                    var osu = loadOsu(host);

                    var collectionManager = osu.Dependencies.Get<CollectionManager>();
                    await collectionManager.Import(new MemoryStream());

                    Assert.That(collectionManager.Collections.Count, Is.Zero);
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public async Task TestImportWithNoBeatmaps()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost())
            {
                try
                {
                    var osu = loadOsu(host);

                    var collectionManager = osu.Dependencies.Get<CollectionManager>();
                    await collectionManager.Import(TestResources.OpenResource("Collections/collections.db"));

                    Assert.That(collectionManager.Collections.Count, Is.EqualTo(2));

                    Assert.That(collectionManager.Collections[0].Name.Value, Is.EqualTo("First"));
                    Assert.That(collectionManager.Collections[0].Beatmaps.Count, Is.Zero);

                    Assert.That(collectionManager.Collections[1].Name.Value, Is.EqualTo("Second"));
                    Assert.That(collectionManager.Collections[1].Beatmaps.Count, Is.Zero);
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public async Task TestImportWithBeatmaps()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost())
            {
                try
                {
                    var osu = loadOsu(host, true);

                    var collectionManager = osu.Dependencies.Get<CollectionManager>();
                    await collectionManager.Import(TestResources.OpenResource("Collections/collections.db"));

                    Assert.That(collectionManager.Collections.Count, Is.EqualTo(2));

                    Assert.That(collectionManager.Collections[0].Name.Value, Is.EqualTo("First"));
                    Assert.That(collectionManager.Collections[0].Beatmaps.Count, Is.EqualTo(1));

                    Assert.That(collectionManager.Collections[1].Name.Value, Is.EqualTo("Second"));
                    Assert.That(collectionManager.Collections[1].Beatmaps.Count, Is.EqualTo(12));
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public async Task TestImportMalformedDatabase()
        {
            bool exceptionThrown = false;
            UnhandledExceptionEventHandler setException = (_, __) => exceptionThrown = true;

            using (HeadlessGameHost host = new CleanRunHeadlessGameHost())
            {
                try
                {
                    AppDomain.CurrentDomain.UnhandledException += setException;

                    var osu = loadOsu(host, true);

                    var collectionManager = osu.Dependencies.Get<CollectionManager>();

                    using (var ms = new MemoryStream())
                    {
                        using (var bw = new BinaryWriter(ms, Encoding.UTF8, true))
                        {
                            for (int i = 0; i < 10000; i++)
                                bw.Write((byte)i);
                        }

                        ms.Seek(0, SeekOrigin.Begin);

                        await collectionManager.Import(ms);
                    }

                    Assert.That(host.UpdateThread.Running, Is.True);
                    Assert.That(exceptionThrown, Is.False);
                    Assert.That(collectionManager.Collections.Count, Is.EqualTo(0));
                }
                finally
                {
                    host.Exit();
                    AppDomain.CurrentDomain.UnhandledException -= setException;
                }
            }
        }

        [Test]
        public async Task TestSaveAndReload()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost())
            {
                try
                {
                    var osu = loadOsu(host, true);

                    var collectionManager = osu.Dependencies.Get<CollectionManager>();
                    await collectionManager.Import(TestResources.OpenResource("Collections/collections.db"));

                    // Move first beatmap from second collection into the first.
                    collectionManager.Collections[0].Beatmaps.Add(collectionManager.Collections[1].Beatmaps[0]);
                    collectionManager.Collections[1].Beatmaps.RemoveAt(0);

                    // Rename the second collecction.
                    collectionManager.Collections[1].Name.Value = "Another";
                }
                finally
                {
                    host.Exit();
                }
            }

            using (HeadlessGameHost host = new HeadlessGameHost("TestSaveAndReload"))
            {
                try
                {
                    var osu = loadOsu(host, true);

                    var collectionManager = osu.Dependencies.Get<CollectionManager>();

                    Assert.That(collectionManager.Collections.Count, Is.EqualTo(2));

                    Assert.That(collectionManager.Collections[0].Name.Value, Is.EqualTo("First"));
                    Assert.That(collectionManager.Collections[0].Beatmaps.Count, Is.EqualTo(2));

                    Assert.That(collectionManager.Collections[1].Name.Value, Is.EqualTo("Another"));
                    Assert.That(collectionManager.Collections[1].Beatmaps.Count, Is.EqualTo(11));
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        private OsuGameBase loadOsu(GameHost host, bool withBeatmap = false)
        {
            var osu = new TestOsuGameBase(withBeatmap);

#pragma warning disable 4014
            Task.Run(() => host.Run(osu));
#pragma warning restore 4014

            waitForOrAssert(() => osu.IsLoaded, @"osu! failed to start in a reasonable amount of time");

            var collectionManager = osu.Dependencies.Get<CollectionManager>();
            waitForOrAssert(() => collectionManager.DatabaseLoaded, "Collection database did not load in a reasonable amount of time");

            return osu;
        }

        private void waitForOrAssert(Func<bool> result, string failureMessage, int timeout = 60000)
        {
            Task task = Task.Run(() =>
            {
                while (!result()) Thread.Sleep(200);
            });

            Assert.IsTrue(task.Wait(timeout), failureMessage);
        }

        private class TestOsuGameBase : OsuGameBase
        {
            private readonly bool withBeatmap;

            public TestOsuGameBase(bool withBeatmap)
            {
                this.withBeatmap = withBeatmap;
            }

            protected override void AddInternal(Drawable drawable)
            {
                // The beatmap must be imported just before the collection manager is loaded.
                if (drawable is CollectionManager && withBeatmap)
                    BeatmapManager.Import(TestResources.GetTestBeatmapForImport()).Wait();

                base.AddInternal(drawable);
            }
        }
    }
}
