// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Collections;
using osu.Game.Database;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Collections.IO
{
    [TestFixture]
    public class ImportCollectionsTest : ImportTest
    {
        [Test]
        public async Task TestImportEmptyDatabase()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost())
            {
                try
                {
                    var osu = LoadOsuIntoHost(host);

                    await importCollectionsFromStream(osu, new MemoryStream());

                    osu.Realm.Run(realm =>
                    {
                        var collections = realm.All<BeatmapCollection>().ToList();
                        Assert.That(collections.Count, Is.Zero);
                    });
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
                    var osu = LoadOsuIntoHost(host);

                    await importCollectionsFromStream(osu, TestResources.OpenResource("Collections/collections.db"));

                    osu.Realm.Run(realm =>
                    {
                        var collections = realm.All<BeatmapCollection>().ToList();
                        Assert.That(collections.Count, Is.EqualTo(2));

                        // Even with no beatmaps imported, collections are tracking the hashes and will continue to.
                        // In the future this whole mechanism will be replaced with having the collections in realm,
                        // but until that happens it makes rough sense that we want to track not-yet-imported beatmaps
                        // and have them associate with collections if/when they become available.

                        Assert.That(collections[0].Name, Is.EqualTo("First"));
                        Assert.That(collections[0].BeatmapMD5Hashes.Count, Is.EqualTo(1));

                        Assert.That(collections[1].Name, Is.EqualTo("Second"));
                        Assert.That(collections[1].BeatmapMD5Hashes.Count, Is.EqualTo(12));
                    });
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
                    var osu = LoadOsuIntoHost(host, true);

                    await importCollectionsFromStream(osu, TestResources.OpenResource("Collections/collections.db"));

                    osu.Realm.Run(realm =>
                    {
                        var collections = realm.All<BeatmapCollection>().ToList();

                        Assert.That(collections.Count, Is.EqualTo(2));

                        Assert.That(collections[0].Name, Is.EqualTo("First"));
                        Assert.That(collections[0].BeatmapMD5Hashes.Count, Is.EqualTo(1));

                        Assert.That(collections[1].Name, Is.EqualTo("Second"));
                        Assert.That(collections[1].BeatmapMD5Hashes.Count, Is.EqualTo(12));
                    });
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
            UnhandledExceptionEventHandler setException = (_, _) => exceptionThrown = true;

            using (HeadlessGameHost host = new CleanRunHeadlessGameHost())
            {
                try
                {
                    AppDomain.CurrentDomain.UnhandledException += setException;

                    var osu = LoadOsuIntoHost(host, true);

                    using (var ms = new MemoryStream())
                    {
                        using (var bw = new BinaryWriter(ms, Encoding.UTF8, true))
                        {
                            for (int i = 0; i < 10000; i++)
                                bw.Write((byte)i);
                        }

                        ms.Seek(0, SeekOrigin.Begin);

                        await importCollectionsFromStream(osu, ms);
                    }

                    Assert.That(exceptionThrown, Is.False);
                    osu.Realm.Run(realm =>
                    {
                        var collections = realm.All<BeatmapCollection>().ToList();
                        Assert.That(collections.Count, Is.EqualTo(0));
                    });
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
            string firstRunName;

            using (var host = new CleanRunHeadlessGameHost(bypassCleanupOnDispose: true))
            {
                firstRunName = host.Name;

                try
                {
                    var osu = LoadOsuIntoHost(host, true);

                    await importCollectionsFromStream(osu, TestResources.OpenResource("Collections/collections.db"));

                    // ReSharper disable once MethodHasAsyncOverload
                    osu.Realm.Write(realm =>
                    {
                        var collections = realm.All<BeatmapCollection>().ToList();

                        // Move first beatmap from second collection into the first.
                        collections[0].BeatmapMD5Hashes.Add(collections[1].BeatmapMD5Hashes[0]);
                        collections[1].BeatmapMD5Hashes.RemoveAt(0);

                        // Rename the second collecction.
                        collections[1].Name = "Another";
                    });
                }
                finally
                {
                    host.Exit();
                }
            }

            // Name matches the automatically chosen name from `CleanRunHeadlessGameHost` above, so we end up using the same storage location.
            using (HeadlessGameHost host = new TestRunHeadlessGameHost(firstRunName))
            {
                try
                {
                    var osu = LoadOsuIntoHost(host, true);

                    osu.Realm.Run(realm =>
                    {
                        var collections = realm.All<BeatmapCollection>().ToList();
                        Assert.That(collections.Count, Is.EqualTo(2));

                        Assert.That(collections[0].Name, Is.EqualTo("First"));
                        Assert.That(collections[0].BeatmapMD5Hashes.Count, Is.EqualTo(2));

                        Assert.That(collections[1].Name, Is.EqualTo("Another"));
                        Assert.That(collections[1].BeatmapMD5Hashes.Count, Is.EqualTo(11));
                    });
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        private static async Task importCollectionsFromStream(TestOsuGameBase osu, Stream stream)
        {
            // intentionally spin this up on a separate task to avoid disposal deadlocks.
            // see https://github.com/EventStore/EventStore/issues/1179
            await Task.Factory.StartNew(() => new LegacyCollectionImporter(osu.Realm).Import(stream).WaitSafely(), TaskCreationOptions.LongRunning);
        }
    }
}
