// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Database;
using osu.Game.Models;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Tests.Resources;
using Realms;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Writers.Zip;

namespace osu.Game.Tests.Database
{
    /// <summary>
    /// Tests the flow where a beatmap is already loaded and an update is applied.
    /// </summary>
    [TestFixture]
    public class BeatmapImporterUpdateTests : RealmTest
    {
        private const int count_beatmaps = 12;

        [Test]
        public void TestNewDifficultyAdded()
        {
            RunTestWithRealmAsync(async (realm, storage) =>
            {
                var importer = new BeatmapImporter(storage, realm);
                using var rulesets = new RealmRulesetStore(realm, storage);

                using var __ = getBeatmapArchive(out string pathOriginal);
                using var _ = getBeatmapArchiveWithModifications(out string pathMissingOneBeatmap, directory =>
                {
                    // remove one difficulty before first import
                    directory.GetFiles("*.osu").First().Delete();
                });

                var importBeforeUpdate = await importer.Import(new ImportTask(pathMissingOneBeatmap));

                Assert.That(importBeforeUpdate, Is.Not.Null);
                Debug.Assert(importBeforeUpdate != null);

                realm.Run(r => r.Refresh());

                checkCount<BeatmapSetInfo>(realm, 1, s => !s.DeletePending);
                Assert.That(importBeforeUpdate.Value.Beatmaps, Has.Count.EqualTo(count_beatmaps - 1));

                // Second import matches first but contains one extra .osu file.
                var importAfterUpdate = await importer.ImportAsUpdate(new ProgressNotification(), new ImportTask(pathOriginal), importBeforeUpdate.Value);

                Assert.That(importAfterUpdate, Is.Not.Null);
                Debug.Assert(importAfterUpdate != null);

                realm.Run(r => r.Refresh());

                checkCount<BeatmapInfo>(realm, count_beatmaps);
                checkCount<BeatmapMetadata>(realm, count_beatmaps);
                checkCount<BeatmapSetInfo>(realm, 1);

                // check the newly "imported" beatmap is not the original.
                Assert.That(importBeforeUpdate.ID, Is.Not.EqualTo(importAfterUpdate.ID));

                // Previous beatmap set has no beatmaps so will be completely purged on the spot.
                Assert.That(importBeforeUpdate.Value.IsValid, Is.False);
            });
        }

        /// <summary>
        /// Regression test covering https://github.com/ppy/osu/issues/19369 (import potentially duplicating if original has no <see cref="BeatmapInfo.OnlineID"/>).
        /// </summary>
        [Test]
        public void TestNewDifficultyAddedNoOnlineID()
        {
            RunTestWithRealmAsync(async (realm, storage) =>
            {
                var importer = new BeatmapImporter(storage, realm);
                using var rulesets = new RealmRulesetStore(realm, storage);

                using var __ = getBeatmapArchive(out string pathOriginal);
                using var _ = getBeatmapArchiveWithModifications(out string pathMissingOneBeatmap, directory =>
                {
                    // remove one difficulty before first import
                    directory.GetFiles("*.osu").First().Delete();
                });

                var importBeforeUpdate = await importer.Import(new ImportTask(pathMissingOneBeatmap));

                Assert.That(importBeforeUpdate, Is.Not.Null);
                Debug.Assert(importBeforeUpdate != null);

                // This test is the same as TestNewDifficultyAdded except for this block.
                importBeforeUpdate.PerformWrite(s =>
                {
                    s.OnlineID = -1;
                    foreach (var beatmap in s.Beatmaps)
                        beatmap.ResetOnlineInfo();
                });

                realm.Run(r => r.Refresh());

                checkCount<BeatmapSetInfo>(realm, 1, s => !s.DeletePending);
                Assert.That(importBeforeUpdate.Value.Beatmaps, Has.Count.EqualTo(count_beatmaps - 1));

                // Second import matches first but contains one extra .osu file.
                var importAfterUpdate = await importer.ImportAsUpdate(new ProgressNotification(), new ImportTask(pathOriginal), importBeforeUpdate.Value);

                Assert.That(importAfterUpdate, Is.Not.Null);
                Debug.Assert(importAfterUpdate != null);

                realm.Run(r => r.Refresh());

                checkCount<BeatmapInfo>(realm, count_beatmaps);
                checkCount<BeatmapMetadata>(realm, count_beatmaps);
                checkCount<BeatmapSetInfo>(realm, 1);

                // check the newly "imported" beatmap is not the original.
                Assert.That(importBeforeUpdate.ID, Is.Not.EqualTo(importAfterUpdate.ID));

                // Previous beatmap set has no beatmaps so will be completely purged on the spot.
                Assert.That(importBeforeUpdate.Value.IsValid, Is.False);
            });
        }

        [Test]
        public void TestExistingDifficultyModified()
        {
            RunTestWithRealmAsync(async (realm, storage) =>
            {
                var importer = new BeatmapImporter(storage, realm);
                using var rulesets = new RealmRulesetStore(realm, storage);

                using var __ = getBeatmapArchive(out string pathOriginal);
                using var _ = getBeatmapArchiveWithModifications(out string pathModified, directory =>
                {
                    // Modify one .osu file with different content.
                    var firstOsuFile = directory.GetFiles("*.osu").First();

                    string existingContent = File.ReadAllText(firstOsuFile.FullName);

                    File.WriteAllText(firstOsuFile.FullName, existingContent + "\n# I am new content");
                });

                var importBeforeUpdate = await importer.Import(new ImportTask(pathOriginal));

                Assert.That(importBeforeUpdate, Is.Not.Null);
                Debug.Assert(importBeforeUpdate != null);

                realm.Run(r => r.Refresh());

                checkCount<BeatmapSetInfo>(realm, 1, s => !s.DeletePending);
                Assert.That(importBeforeUpdate.Value.Beatmaps, Has.Count.EqualTo(count_beatmaps));

                // Second import matches first but contains one extra .osu file.
                var importAfterUpdate = await importer.ImportAsUpdate(new ProgressNotification(), new ImportTask(pathModified), importBeforeUpdate.Value);

                Assert.That(importAfterUpdate, Is.Not.Null);
                Debug.Assert(importAfterUpdate != null);

                // should only contain the modified beatmap (others purged).
                Assert.That(importBeforeUpdate.Value.Beatmaps, Has.Count.EqualTo(1));
                Assert.That(importAfterUpdate.Value.Beatmaps, Has.Count.EqualTo(count_beatmaps));

                realm.Run(r => r.Refresh());

                checkCount<BeatmapInfo>(realm, count_beatmaps + 1);
                checkCount<BeatmapMetadata>(realm, count_beatmaps + 1);

                checkCount<BeatmapSetInfo>(realm, 1, s => !s.DeletePending);
                checkCount<BeatmapSetInfo>(realm, 1, s => s.DeletePending);
            });
        }

        [Test]
        public void TestExistingDifficultyRemoved()
        {
            RunTestWithRealmAsync(async (realm, storage) =>
            {
                var importer = new BeatmapImporter(storage, realm);
                using var rulesets = new RealmRulesetStore(realm, storage);

                using var __ = getBeatmapArchive(out string pathOriginal);
                using var _ = getBeatmapArchiveWithModifications(out string pathMissingOneBeatmap, directory =>
                {
                    // remove one difficulty before first import
                    directory.GetFiles("*.osu").First().Delete();
                });

                var importBeforeUpdate = await importer.Import(new ImportTask(pathOriginal));

                Assert.That(importBeforeUpdate, Is.Not.Null);
                Debug.Assert(importBeforeUpdate != null);

                Assert.That(importBeforeUpdate.Value.Beatmaps, Has.Count.EqualTo(count_beatmaps));
                Assert.That(importBeforeUpdate.Value.Beatmaps.First().OnlineID, Is.GreaterThan(-1));

                // Second import matches first but contains one extra .osu file.
                var importAfterUpdate = await importer.ImportAsUpdate(new ProgressNotification(), new ImportTask(pathMissingOneBeatmap), importBeforeUpdate.Value);

                Assert.That(importAfterUpdate, Is.Not.Null);
                Debug.Assert(importAfterUpdate != null);

                realm.Run(r => r.Refresh());

                checkCount<BeatmapInfo>(realm, count_beatmaps);
                checkCount<BeatmapMetadata>(realm, count_beatmaps);
                checkCount<BeatmapSetInfo>(realm, 2);

                // previous set should contain the removed beatmap still.
                Assert.That(importBeforeUpdate.Value.Beatmaps, Has.Count.EqualTo(1));
                Assert.That(importBeforeUpdate.Value.Beatmaps.First().OnlineID, Is.EqualTo(-1));

                // Previous beatmap set has no beatmaps so will be completely purged on the spot.
                Assert.That(importAfterUpdate.Value.Beatmaps, Has.Count.EqualTo(count_beatmaps - 1));
            });
        }

        [Test]
        public void TestUpdatedImportContainsNothing()
        {
            RunTestWithRealmAsync(async (realm, storage) =>
            {
                var importer = new BeatmapImporter(storage, realm);
                using var rulesets = new RealmRulesetStore(realm, storage);

                using var __ = getBeatmapArchive(out string pathOriginal);
                using var _ = getBeatmapArchiveWithModifications(out string pathEmpty, directory =>
                {
                    foreach (var file in directory.GetFiles())
                        file.Delete();
                });

                var importBeforeUpdate = await importer.Import(new ImportTask(pathOriginal));

                Assert.That(importBeforeUpdate, Is.Not.Null);
                Debug.Assert(importBeforeUpdate != null);

                var importAfterUpdate = await importer.ImportAsUpdate(new ProgressNotification(), new ImportTask(pathEmpty), importBeforeUpdate.Value);
                Assert.That(importAfterUpdate, Is.Null);

                realm.Run(r => r.Refresh());

                checkCount<BeatmapSetInfo>(realm, 1);
                checkCount<BeatmapInfo>(realm, count_beatmaps);
                checkCount<BeatmapMetadata>(realm, count_beatmaps);

                Assert.That(importBeforeUpdate.Value.IsValid, Is.True);
            });
        }

        [Test]
        public void TestNoChanges()
        {
            RunTestWithRealmAsync(async (realm, storage) =>
            {
                var importer = new BeatmapImporter(storage, realm);
                using var rulesets = new RealmRulesetStore(realm, storage);

                using var __ = getBeatmapArchive(out string pathOriginal);
                using var _ = getBeatmapArchive(out string pathOriginalSecond);

                var importBeforeUpdate = await importer.Import(new ImportTask(pathOriginal));

                Assert.That(importBeforeUpdate, Is.Not.Null);
                Debug.Assert(importBeforeUpdate != null);

                var importAfterUpdate = await importer.ImportAsUpdate(new ProgressNotification(), new ImportTask(pathOriginalSecond), importBeforeUpdate.Value);

                Assert.That(importAfterUpdate, Is.Not.Null);
                Debug.Assert(importAfterUpdate != null);

                realm.Run(r => r.Refresh());

                checkCount<BeatmapSetInfo>(realm, 1);
                checkCount<BeatmapInfo>(realm, count_beatmaps);
                checkCount<BeatmapMetadata>(realm, count_beatmaps);

                Assert.That(importBeforeUpdate.Value.Beatmaps.First().OnlineID, Is.GreaterThan(-1));
                Assert.That(importBeforeUpdate.ID, Is.EqualTo(importAfterUpdate.ID));
            });
        }

        [Test]
        public void TestScoreTransferredOnUnchanged()
        {
            RunTestWithRealmAsync(async (realm, storage) =>
            {
                var importer = new BeatmapImporter(storage, realm);
                using var rulesets = new RealmRulesetStore(realm, storage);
                string removedFilename = null!;

                using var __ = getBeatmapArchive(out string pathOriginal);
                using var _ = getBeatmapArchiveWithModifications(out string pathMissingOneBeatmap, directory =>
                {
                    // arbitrary beatmap removal
                    var fileToRemove = directory.GetFiles("*.osu").First();

                    removedFilename = fileToRemove.Name;
                    fileToRemove.Delete();
                });

                var importBeforeUpdate = await importer.Import(new ImportTask(pathOriginal));

                Assert.That(importBeforeUpdate, Is.Not.Null);
                Debug.Assert(importBeforeUpdate != null);

                string scoreTargetBeatmapHash = string.Empty;

                importBeforeUpdate.PerformWrite(s =>
                {
                    // make sure not to add scores to the same beatmap that is removed in the update.
                    var beatmapInfo = s.Beatmaps.First(b => b.File?.Filename != removedFilename);

                    scoreTargetBeatmapHash = beatmapInfo.Hash;
                    s.Realm!.Add(new ScoreInfo(beatmapInfo, s.Realm.All<RulesetInfo>().First(), new RealmUser()));
                });

                realm.Run(r => r.Refresh());

                checkCount<ScoreInfo>(realm, 1);

                var importAfterUpdate = await importer.ImportAsUpdate(new ProgressNotification(), new ImportTask(pathMissingOneBeatmap), importBeforeUpdate.Value);

                Assert.That(importAfterUpdate, Is.Not.Null);
                Debug.Assert(importAfterUpdate != null);

                realm.Run(r => r.Refresh());

                checkCount<BeatmapInfo>(realm, count_beatmaps);
                checkCount<BeatmapMetadata>(realm, count_beatmaps);
                checkCount<BeatmapSetInfo>(realm, 2);

                // score is transferred across to the new set
                checkCount<ScoreInfo>(realm, 1);
                Assert.That(importAfterUpdate.Value.Beatmaps.First(b => b.Hash == scoreTargetBeatmapHash).Scores, Has.Count.EqualTo(1));
            });
        }

        [Test]
        public void TestDanglingScoreTransferred()
        {
            RunTestWithRealmAsync(async (realm, storage) =>
            {
                var importer = new BeatmapImporter(storage, realm);
                using var rulesets = new RealmRulesetStore(realm, storage);

                using var __ = getBeatmapArchive(out string pathOriginal);
                using var _ = getBeatmapArchive(out string pathOnlineCopy);

                var importBeforeUpdate = await importer.Import(new ImportTask(pathOriginal));

                Assert.That(importBeforeUpdate, Is.Not.Null);
                Debug.Assert(importBeforeUpdate != null);

                string scoreTargetBeatmapHash = string.Empty;

                // set a score on the beatmap
                importBeforeUpdate.PerformWrite(s =>
                {
                    var beatmapInfo = s.Beatmaps.First();

                    scoreTargetBeatmapHash = beatmapInfo.Hash;

                    s.Realm!.Add(new ScoreInfo(beatmapInfo, s.Realm.All<RulesetInfo>().First(), new RealmUser()));
                });

                // locally modify beatmap
                const string new_beatmap_hash = "new_hash";
                importBeforeUpdate.PerformWrite(s =>
                {
                    var beatmapInfo = s.Beatmaps.First(b => b.Hash == scoreTargetBeatmapHash);

                    beatmapInfo.Hash = new_beatmap_hash;
                    beatmapInfo.ResetOnlineInfo();
                    beatmapInfo.UpdateLocalScores(s.Realm!);
                });

                realm.Run(r => r.Refresh());

                // making changes to a beatmap doesn't remove the score from realm, but should disassociate the beatmap.
                checkCount<ScoreInfo>(realm, 1);
                Assert.That(realm.Run(r => r.All<ScoreInfo>().First().BeatmapInfo), Is.Null);

                // reimport the original beatmap before local modifications
                var importAfterUpdate = await importer.ImportAsUpdate(new ProgressNotification(), new ImportTask(pathOnlineCopy), importBeforeUpdate.Value);

                Assert.That(importAfterUpdate, Is.Not.Null);
                Debug.Assert(importAfterUpdate != null);

                realm.Run(r => r.Refresh());

                // both original and locally modified versions present
                checkCount<BeatmapInfo>(realm, count_beatmaps + 1);
                checkCount<BeatmapMetadata>(realm, count_beatmaps + 1);
                checkCount<BeatmapSetInfo>(realm, 2);

                // score is preserved
                checkCount<ScoreInfo>(realm, 1);

                // score is transferred to new beatmap
                Assert.That(importBeforeUpdate.Value.Beatmaps.First(b => b.Hash == new_beatmap_hash).Scores, Has.Count.EqualTo(0));
                Assert.That(importAfterUpdate.Value.Beatmaps.First(b => b.Hash == scoreTargetBeatmapHash).Scores, Has.Count.EqualTo(1));
            });
        }

        [Test]
        public void TestScoreLostOnModification()
        {
            RunTestWithRealmAsync(async (realm, storage) =>
            {
                var importer = new BeatmapImporter(storage, realm);
                using var rulesets = new RealmRulesetStore(realm, storage);

                using var __ = getBeatmapArchive(out string pathOriginal);

                var importBeforeUpdate = await importer.Import(new ImportTask(pathOriginal));

                Assert.That(importBeforeUpdate, Is.Not.Null);
                Debug.Assert(importBeforeUpdate != null);

                string? scoreTargetFilename = string.Empty;

                importBeforeUpdate.PerformWrite(s =>
                {
                    var beatmapInfo = s.Beatmaps.Last();
                    scoreTargetFilename = beatmapInfo.File?.Filename;
                    s.Realm!.Add(new ScoreInfo(beatmapInfo, s.Realm.All<RulesetInfo>().First(), new RealmUser()));
                });

                realm.Run(r => r.Refresh());

                checkCount<ScoreInfo>(realm, 1);

                using var _ = getBeatmapArchiveWithModifications(out string pathModified, directory =>
                {
                    // Modify one .osu file with different content.
                    var firstOsuFile = directory.GetFiles(scoreTargetFilename).First();

                    string existingContent = File.ReadAllText(firstOsuFile.FullName);

                    File.WriteAllText(firstOsuFile.FullName, existingContent + "\n# I am new content");
                });

                var importAfterUpdate = await importer.ImportAsUpdate(new ProgressNotification(), new ImportTask(pathModified), importBeforeUpdate.Value);

                Assert.That(importAfterUpdate, Is.Not.Null);
                Debug.Assert(importAfterUpdate != null);

                realm.Run(r => r.Refresh());

                checkCount<BeatmapInfo>(realm, count_beatmaps + 1);
                checkCount<BeatmapMetadata>(realm, count_beatmaps + 1);
                checkCount<BeatmapSetInfo>(realm, 2);

                // score is not transferred due to modifications.
                checkCount<ScoreInfo>(realm, 1);
                Assert.That(importBeforeUpdate.Value.Beatmaps.AsEnumerable().First(b => b.File?.Filename == scoreTargetFilename).Scores, Has.Count.EqualTo(1));
                Assert.That(importAfterUpdate.Value.Beatmaps.AsEnumerable().First(b => b.File?.Filename == scoreTargetFilename).Scores, Has.Count.EqualTo(0));
            });
        }

        [Test]
        public void TestMetadataTransferred()
        {
            RunTestWithRealmAsync(async (realm, storage) =>
            {
                var importer = new BeatmapImporter(storage, realm);
                using var rulesets = new RealmRulesetStore(realm, storage);

                using var __ = getBeatmapArchive(out string pathOriginal);
                using var _ = getBeatmapArchiveWithModifications(out string pathMissingOneBeatmap, directory =>
                {
                    // arbitrary beatmap removal
                    directory.GetFiles("*.osu").First().Delete();
                });

                var importBeforeUpdate = await importer.Import(new ImportTask(pathOriginal));

                Assert.That(importBeforeUpdate, Is.Not.Null);
                Debug.Assert(importBeforeUpdate != null);

                var importAfterUpdate = await importer.ImportAsUpdate(new ProgressNotification(), new ImportTask(pathMissingOneBeatmap), importBeforeUpdate.Value);

                Assert.That(importAfterUpdate, Is.Not.Null);
                Debug.Assert(importAfterUpdate != null);

                Assert.That(importBeforeUpdate.ID, Is.Not.EqualTo(importAfterUpdate.ID));
                Assert.That(importBeforeUpdate.Value.DateAdded, Is.EqualTo(importAfterUpdate.Value.DateAdded));
            });
        }

        /// <summary>
        /// If all difficulties in the original beatmap set are in a collection, presume the user also wants new difficulties added.
        /// </summary>
        [TestCase(false)]
        [TestCase(true)]
        public void TestCollectionTransferNewBeatmap(bool allOriginalBeatmapsInCollection)
        {
            RunTestWithRealmAsync(async (realm, storage) =>
            {
                var importer = new BeatmapImporter(storage, realm);
                using var rulesets = new RealmRulesetStore(realm, storage);

                using var __ = getBeatmapArchive(out string pathOriginal);
                using var _ = getBeatmapArchiveWithModifications(out string pathMissingOneBeatmap, directory =>
                {
                    // remove one difficulty before first import
                    directory.GetFiles("*.osu").First().Delete();
                });

                var importBeforeUpdate = await importer.Import(new ImportTask(pathMissingOneBeatmap));

                Assert.That(importBeforeUpdate, Is.Not.Null);
                Debug.Assert(importBeforeUpdate != null);

                int beatmapsToAddToCollection = 0;

                importBeforeUpdate.PerformWrite(s =>
                {
                    var beatmapCollection = s.Realm!.Add(new BeatmapCollection("test collection"));
                    beatmapsToAddToCollection = s.Beatmaps.Count - (allOriginalBeatmapsInCollection ? 0 : 1);

                    for (int i = 0; i < beatmapsToAddToCollection; i++)
                        beatmapCollection.BeatmapMD5Hashes.Add(s.Beatmaps[i].MD5Hash);
                });

                // Second import matches first but contains one extra .osu file.
                var importAfterUpdate = await importer.ImportAsUpdate(new ProgressNotification(), new ImportTask(pathOriginal), importBeforeUpdate.Value);

                Assert.That(importAfterUpdate, Is.Not.Null);
                Debug.Assert(importAfterUpdate != null);

                importAfterUpdate.PerformRead(updated =>
                {
                    updated.Realm!.Refresh();

                    string[] hashes = updated.Realm.All<BeatmapCollection>().Single().BeatmapMD5Hashes.ToArray();

                    if (allOriginalBeatmapsInCollection)
                    {
                        Assert.That(updated.Beatmaps.Count, Is.EqualTo(beatmapsToAddToCollection + 1));
                        Assert.That(hashes, Has.Length.EqualTo(updated.Beatmaps.Count));
                    }
                    else
                    {
                        // Collection contains one less than the original beatmap, and two less after update (new difficulty included).
                        Assert.That(updated.Beatmaps.Count, Is.EqualTo(beatmapsToAddToCollection + 2));
                        Assert.That(hashes, Has.Length.EqualTo(beatmapsToAddToCollection));
                    }
                });
            });
        }

        /// <summary>
        /// If a difficulty in the original beatmap set is modified, the updated version should remain in any collections it was in.
        /// </summary>
        [Test]
        public void TestCollectionTransferModifiedBeatmap()
        {
            RunTestWithRealmAsync(async (realm, storage) =>
            {
                var importer = new BeatmapImporter(storage, realm);
                using var rulesets = new RealmRulesetStore(realm, storage);

                using var __ = getBeatmapArchive(out string pathOriginal);
                using var _ = getBeatmapArchiveWithModifications(out string pathModified, directory =>
                {
                    // Modify one .osu file with different content.
                    var firstOsuFile = directory.GetFiles("*[Hard]*.osu").First();

                    string existingContent = File.ReadAllText(firstOsuFile.FullName);

                    File.WriteAllText(firstOsuFile.FullName, existingContent + "\n# I am new content");
                });

                var importBeforeUpdate = await importer.Import(new ImportTask(pathOriginal));

                Assert.That(importBeforeUpdate, Is.Not.Null);
                Debug.Assert(importBeforeUpdate != null);

                string originalHash = string.Empty;

                importBeforeUpdate.PerformWrite(s =>
                {
                    var beatmapCollection = s.Realm!.Add(new BeatmapCollection("test collection"));
                    originalHash = s.Beatmaps.Single(b => b.DifficultyName == "Hard").MD5Hash;

                    beatmapCollection.BeatmapMD5Hashes.Add(originalHash);
                });

                // Second import matches first but contains a modified .osu file.
                var importAfterUpdate = await importer.ImportAsUpdate(new ProgressNotification(), new ImportTask(pathModified), importBeforeUpdate.Value);

                Assert.That(importAfterUpdate, Is.Not.Null);
                Debug.Assert(importAfterUpdate != null);

                importAfterUpdate.PerformRead(updated =>
                {
                    updated.Realm!.Refresh();

                    string[] hashes = updated.Realm.All<BeatmapCollection>().Single().BeatmapMD5Hashes.ToArray();
                    string updatedHash = updated.Beatmaps.Single(b => b.DifficultyName == "Hard").MD5Hash;

                    Assert.That(hashes, Has.Length.EqualTo(1));
                    Assert.That(hashes.First(), Is.EqualTo(updatedHash));

                    Assert.That(updatedHash, Is.Not.EqualTo(originalHash));
                });
            });
        }

        private static void checkCount<T>(RealmAccess realm, int expected, Expression<Func<T, bool>>? condition = null) where T : RealmObject
        {
            var query = realm.Realm.All<T>();

            if (condition != null)
                query = query.Where(condition);

            Assert.That(query, Has.Count.EqualTo(expected));
        }

        private static IDisposable getBeatmapArchiveWithModifications(out string path, Action<DirectoryInfo> applyModifications)
        {
            var cleanup = getBeatmapArchive(out path);

            string extractedFolder = $"{path}_extracted";
            Directory.CreateDirectory(extractedFolder);

            using (var zip = ZipArchive.Open(path))
                zip.WriteToDirectory(extractedFolder);

            applyModifications(new DirectoryInfo(extractedFolder));

            File.Delete(path);

            using (var zip = ZipArchive.Create())
            {
                zip.AddAllFromDirectory(extractedFolder);
                zip.SaveTo(path, new ZipWriterOptions(CompressionType.Deflate));
            }

            Directory.Delete(extractedFolder, true);

            return cleanup;
        }

        private static IDisposable getBeatmapArchive(out string path, bool quick = true)
        {
            string beatmapPath = TestResources.GetTestBeatmapForImport(quick);

            path = beatmapPath;

            return new InvokeOnDisposal(() => File.Delete(beatmapPath));
        }
    }
}
