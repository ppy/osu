// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
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

                    await osu.CollectionManager.Import(new MemoryStream());

                    Assert.That(osu.CollectionManager.Collections.Count, Is.Zero);
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

                    await osu.CollectionManager.Import(TestResources.OpenResource("Collections/collections.db"));

                    Assert.That(osu.CollectionManager.Collections.Count, Is.EqualTo(2));

                    Assert.That(osu.CollectionManager.Collections[0].Name.Value, Is.EqualTo("First"));
                    Assert.That(osu.CollectionManager.Collections[0].Beatmaps.Count, Is.Zero);

                    Assert.That(osu.CollectionManager.Collections[1].Name.Value, Is.EqualTo("Second"));
                    Assert.That(osu.CollectionManager.Collections[1].Beatmaps.Count, Is.Zero);
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

                    await osu.CollectionManager.Import(TestResources.OpenResource("Collections/collections.db"));

                    Assert.That(osu.CollectionManager.Collections.Count, Is.EqualTo(2));

                    Assert.That(osu.CollectionManager.Collections[0].Name.Value, Is.EqualTo("First"));
                    Assert.That(osu.CollectionManager.Collections[0].Beatmaps.Count, Is.EqualTo(1));

                    Assert.That(osu.CollectionManager.Collections[1].Name.Value, Is.EqualTo("Second"));
                    Assert.That(osu.CollectionManager.Collections[1].Beatmaps.Count, Is.EqualTo(12));
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

                    using (var ms = new MemoryStream())
                    {
                        using (var bw = new BinaryWriter(ms, Encoding.UTF8, true))
                        {
                            for (int i = 0; i < 10000; i++)
                                bw.Write((byte)i);
                        }

                        ms.Seek(0, SeekOrigin.Begin);

                        await osu.CollectionManager.Import(ms);
                    }

                    Assert.That(host.UpdateThread.Running, Is.True);
                    Assert.That(exceptionThrown, Is.False);
                    Assert.That(osu.CollectionManager.Collections.Count, Is.EqualTo(0));
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

                    await osu.CollectionManager.Import(TestResources.OpenResource("Collections/collections.db"));

                    // Move first beatmap from second collection into the first.
                    osu.CollectionManager.Collections[0].Beatmaps.Add(osu.CollectionManager.Collections[1].Beatmaps[0]);
                    osu.CollectionManager.Collections[1].Beatmaps.RemoveAt(0);

                    // Rename the second collecction.
                    osu.CollectionManager.Collections[1].Name.Value = "Another";
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

                    Assert.That(osu.CollectionManager.Collections.Count, Is.EqualTo(2));

                    Assert.That(osu.CollectionManager.Collections[0].Name.Value, Is.EqualTo("First"));
                    Assert.That(osu.CollectionManager.Collections[0].Beatmaps.Count, Is.EqualTo(2));

                    Assert.That(osu.CollectionManager.Collections[1].Name.Value, Is.EqualTo("Another"));
                    Assert.That(osu.CollectionManager.Collections[1].Beatmaps.Count, Is.EqualTo(11));
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        private TestOsuGameBase loadOsu(GameHost host, bool withBeatmap = false)
        {
            var osu = new TestOsuGameBase(withBeatmap);

#pragma warning disable 4014
            Task.Run(() => host.Run(osu));
#pragma warning restore 4014

            waitForOrAssert(() => osu.IsLoaded, @"osu! failed to start in a reasonable amount of time");

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
            public CollectionManager CollectionManager { get; private set; }

            private readonly bool withBeatmap;

            public TestOsuGameBase(bool withBeatmap)
            {
                this.withBeatmap = withBeatmap;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                // Beatmap must be imported before the collection manager is loaded.
                if (withBeatmap)
                    BeatmapManager.Import(TestResources.GetTestBeatmapForImport()).Wait();

                AddInternal(CollectionManager = new CollectionManager(Storage));
            }
        }
    }
}
