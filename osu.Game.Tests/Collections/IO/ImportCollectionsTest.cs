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
using osu.Game.Beatmaps;
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
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost("TestImportEmptyDatabase"))
            {
                try
                {
                    var osu = await loadOsu(host);

                    var collectionManager = osu.Dependencies.Get<BeatmapCollectionManager>();
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
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost("TestImportWithNoBeatmaps"))
            {
                try
                {
                    var osu = await loadOsu(host);

                    var collectionManager = osu.Dependencies.Get<BeatmapCollectionManager>();
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
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost("TestImportWithBeatmaps"))
            {
                try
                {
                    var osu = await loadOsu(host, true);

                    var collectionManager = osu.Dependencies.Get<BeatmapCollectionManager>();
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

            using (HeadlessGameHost host = new CleanRunHeadlessGameHost("TestImportMalformedDatabase"))
            {
                try
                {
                    AppDomain.CurrentDomain.UnhandledException += setException;

                    var osu = await loadOsu(host, true);

                    var collectionManager = osu.Dependencies.Get<BeatmapCollectionManager>();

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
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost("TestSaveAndReload"))
            {
                try
                {
                    var osu = await loadOsu(host, true);

                    var collectionManager = osu.Dependencies.Get<BeatmapCollectionManager>();
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
                    var osu = await loadOsu(host, true);

                    var collectionManager = osu.Dependencies.Get<BeatmapCollectionManager>();

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

        private async Task<OsuGameBase> loadOsu(GameHost host, bool withBeatmap = false)
        {
            var osu = new OsuGameBase();

#pragma warning disable 4014
            Task.Run(() => host.Run(osu));
#pragma warning restore 4014

            waitForOrAssert(() => osu.IsLoaded, @"osu! failed to start in a reasonable amount of time");

            if (withBeatmap)
            {
                var beatmapFile = TestResources.GetTestBeatmapForImport();
                var beatmapManager = osu.Dependencies.Get<BeatmapManager>();
                await beatmapManager.Import(beatmapFile);
            }

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
    }
}
