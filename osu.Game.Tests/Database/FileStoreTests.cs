// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using osu.Framework.Logging;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.Models;

namespace osu.Game.Tests.Database
{
    public class FileStoreTests : RealmTest
    {
        [Test]
        public void TestImportFile()
        {
            RunTestWithRealm((realmAccess, storage) =>
            {
                var realm = realmAccess.Realm;
                var files = new RealmFileStore(realmAccess, storage);

                var testData = new MemoryStream(new byte[] { 0, 1, 2, 3 });

                realm.Write(() => files.Add(testData, realm));

                ClassicAssert.True(files.Storage.Exists("0/05/054edec1d0211f624fed0cbca9d4f9400b0e491c43742af2c5b0abebf0c990d8"));
                ClassicAssert.True(files.Storage.Exists(realm.All<RealmFile>().First().GetStoragePath()));
            });
        }

        [Test]
        public void TestImportSameFileTwice()
        {
            RunTestWithRealm((realmAccess, storage) =>
            {
                var realm = realmAccess.Realm;
                var files = new RealmFileStore(realmAccess, storage);

                var testData = new MemoryStream(new byte[] { 0, 1, 2, 3 });

                realm.Write(() => files.Add(testData, realm));
                realm.Write(() => files.Add(testData, realm));

                ClassicAssert.AreEqual(1, realm.All<RealmFile>().Count());
            });
        }

        [Test]
        public void TestDontPurgeReferenced()
        {
            RunTestWithRealm((realmAccess, storage) =>
            {
                var realm = realmAccess.Realm;
                var files = new RealmFileStore(realmAccess, storage);

                var file = realm.Write(() => files.Add(new MemoryStream(new byte[] { 0, 1, 2, 3 }), realm));

                var timer = new Stopwatch();
                timer.Start();

                realm.Write(() =>
                {
                    // attach the file to an arbitrary beatmap
                    var beatmapSet = CreateBeatmapSet(CreateRuleset());

                    beatmapSet.Files.Add(new RealmNamedFileUsage(file, "arbitrary.resource"));

                    realm.Add(beatmapSet);
                });

                Logger.Log($"Import complete at {timer.ElapsedMilliseconds}");

                string path = file.GetStoragePath();

                ClassicAssert.True(realm.All<RealmFile>().Any());
                ClassicAssert.True(files.Storage.Exists(path));

                files.Cleanup();
                Logger.Log($"Cleanup complete at {timer.ElapsedMilliseconds}");

                ClassicAssert.True(realm.All<RealmFile>().Any());
                ClassicAssert.True(file.IsValid);
                ClassicAssert.True(files.Storage.Exists(path));
            });
        }

        [Test]
        public void TestPurgeUnreferenced()
        {
            RunTestWithRealm((realmAccess, storage) =>
            {
                var realm = realmAccess.Realm;
                var files = new RealmFileStore(realmAccess, storage);

                var file = realm.Write(() => files.Add(new MemoryStream(new byte[] { 0, 1, 2, 3 }), realm));

                string path = file.GetStoragePath();

                ClassicAssert.True(realm.All<RealmFile>().Any());
                ClassicAssert.True(files.Storage.Exists(path));

                files.Cleanup();

                ClassicAssert.False(realm.All<RealmFile>().Any());
                ClassicAssert.False(file.IsValid);
                ClassicAssert.False(files.Storage.Exists(path));
            });
        }
    }
}
