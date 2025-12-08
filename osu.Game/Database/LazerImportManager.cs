// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Models;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Skinning;
using Realms;

namespace osu.Game.Database
{
    /// <summary>
    /// Handles importing files and database from another osu!lazer install
    /// </summary>
    public class LazerImportManager
    {
        private readonly RealmAccess realmAccess;
        private readonly INotificationOverlay notifications;
        private readonly Storage storage;

        public LazerImportManager(RealmAccess realmAccess, INotificationOverlay notifications, Storage storage)
        {
            this.realmAccess = realmAccess;
            this.notifications = notifications;
            this.storage = storage;
        }

        /// <summary>
        /// Imports content from a separate osu!lazer data directory.
        /// </summary>
        /// <param name="sourcePath">The root directory of the source installation (containing 'client.realm' and 'files').</param>
        public async Task ImportFrom(string sourcePath)
        {
            var filesNotification = new ProgressNotification
            {
                Text = "Importing files (physical)...",
                State = ProgressNotificationState.Active
            };
            notifications.Post(filesNotification);

            await copyFiles(filesNotification, sourcePath).ConfigureAwait(false);

            filesNotification.CompletionText = "Files copied!";
            filesNotification.State = ProgressNotificationState.Completed;

            if (filesNotification.State == ProgressNotificationState.Cancelled) return;

            realmAccess.Write(destRealm =>
            {
                var sourceConfig = new RealmConfiguration(Path.Combine(sourcePath, "client.realm"))
                {
                    IsReadOnly = true,
                    SchemaVersion = RealmAccess.schema_version
                };

                using (var sourceRealm = Realm.GetInstance(sourceConfig))
                {
                    importRealmFiles(sourceRealm, destRealm);

                    importBeatmaps(sourceRealm, destRealm);

                    importScores(sourceRealm, destRealm);

                    importSkins(sourceRealm, destRealm);

                    importCollections(sourceRealm, destRealm);
                }
            });
        }

        private async Task copyFiles(ProgressNotification notification, string sourcePath)
        {
            string sourceFilesPath = Path.Combine(sourcePath, "files");
            var destFilesStorage = storage!.GetStorageForDirectory("files");

            var sourceConfig = new RealmConfiguration(Path.Combine(sourcePath, "client.realm"))
            {
                IsReadOnly = true,
                SchemaVersion = RealmAccess.schema_version
            };

            List<string> filesToCopy;

            using (var realm = Realm.GetInstance(sourceConfig))
            {
                filesToCopy = realm.All<RealmFile>().AsEnumerable().Select(f => f.Hash).ToList();
            }

            int total = filesToCopy.Count;
            int current = 0;
            var uniqueHashes = new HashSet<string>(filesToCopy);

            foreach (string hash in uniqueHashes)
            {
                if (notification.State == ProgressNotificationState.Cancelled) return;

                string folder1 = hash.Substring(0, 1);
                string folder2 = hash.Substring(0, 2);
                string relativePath = Path.Combine(folder1, folder2, hash);

                if (!destFilesStorage.Exists(relativePath))
                {
                    string sourceFilePath = Path.Combine(sourceFilesPath, folder1, folder2, hash);

                    if (File.Exists(sourceFilePath))
                    {
                        using (var src = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
                        using (var dst = destFilesStorage.CreateFileSafely(relativePath))
                        {
                            await src.CopyToAsync(dst).ConfigureAwait(false);
                        }
                    }
                }

                current++;
                notification.Text = $"Copying files ({current}/{uniqueHashes.Count})";
                notification.Progress = (float)current / uniqueHashes.Count;
            }
        }

        private void importRealmFiles(Realm sourceRealm, Realm destRealm)
        {
            var sourceFiles = sourceRealm.All<RealmFile>();
            int total = sourceFiles.Count();
            int current = 0;

            var notification = new ProgressNotification
            {
                Text = "Importing file metadata...",
                State = ProgressNotificationState.Active
            };
            notifications.Post(notification);

            var existingHashes = new HashSet<string>(destRealm.All<RealmFile>().AsEnumerable().Select(f => f.Hash));
            foreach (var file in sourceFiles)
            {
                if (notification.State == ProgressNotificationState.Cancelled) return;

                if (!existingHashes.Contains(file.Hash))
                {
                    destRealm.Add(new RealmFile { Hash = file.Hash });
                    existingHashes.Add(file.Hash);
                }

                current++;
                notification.Text = $"Importing file metadata ({current} of {total})";
                notification.Progress = (float)current / total;
            }

            notification.CompletionText = "File metadata imported!";
            notification.State = ProgressNotificationState.Completed;
        }

        private void importBeatmaps(Realm sourceRealm, Realm destRealm)
        {
            var sourceBeatmaps = sourceRealm.All<BeatmapSetInfo>().Where(b => !b.DeletePending).AsEnumerable();
            int total = sourceBeatmaps.Count();
            int current = 0;

            var notification = new ProgressNotification
            {
                Text = "Importing beatmaps...",
                State = ProgressNotificationState.Active
            };
            notifications.Post(notification);

            var existingRulesets = destRealm.All<RulesetInfo>().ToDictionary(r => r.ShortName);
            var existingIDs = new HashSet<Guid>(destRealm.All<BeatmapSetInfo>().AsEnumerable().Select(b => b.ID));

            foreach (var set in sourceBeatmaps)
            {
                if (notification.State == ProgressNotificationState.Cancelled) return;

                // early return if beatmap (ID) already exists
                if (existingIDs.Contains(set.ID))
                {
                    current++;
                    notification.Text = $"Importing beatmaps ({current} of {total})";
                    notification.Progress = (float)current / total;
                    continue;
                }

                var newSet = set.Detach();

                newSet.Files.Clear();
                foreach (var fileUsage in set.Files)
                {
                    var dbFile = destRealm.Find<RealmFile>(fileUsage.File.Hash);
                    if (dbFile != null)
                    {
                        newSet.Files.Add(new RealmNamedFileUsage(dbFile, fileUsage.Filename));
                    }
                }

                foreach (var beatmap in newSet.Beatmaps)
                {
                    if (existingRulesets.TryGetValue(beatmap.Ruleset.ShortName, out var ruleset))
                        beatmap.Ruleset = ruleset;

                    beatmap.BeatmapSet = newSet;
                }

                destRealm.Add(newSet);
                existingIDs.Add(newSet.ID);

                current++;
                notification.Text = $"Importing beatmaps ({current} of {total})";
                notification.Progress = (float)current / total;
            }

            notification.CompletionText = "Beatmaps imported!";
            notification.State = ProgressNotificationState.Completed;
        }

        private void importScores(Realm sourceRealm, Realm destRealm)
        {
            var sourceScores = sourceRealm.All<ScoreInfo>().Where(s => !s.DeletePending).AsEnumerable();
            int total = sourceScores.Count();
            int current = 0;

            var notification = new ProgressNotification
            {
                Text = "Importing scores...",
                State = ProgressNotificationState.Active
            };
            notifications.Post(notification);

            var existingRulesets = destRealm.All<RulesetInfo>().ToDictionary(r => r.ShortName);
            var existingIDs = new HashSet<Guid>(destRealm.All<ScoreInfo>().AsEnumerable().Select(s => s.ID));

            foreach (var score in sourceScores)
            {
                if (notification.State == ProgressNotificationState.Cancelled) return;

                // early return if score (ID) already exists
                if (existingIDs.Contains(score.ID))
                {
                    current++;
                    notification.Text = $"Importing scores ({current} of {total})";
                    notification.Progress = (float)current / total;
                    continue;
                }

                string mapHash = score.BeatmapInfo?.MD5Hash ?? score.BeatmapHash;
                var targetBeatmap = destRealm.All<BeatmapInfo>().FirstOrDefault(b => b.MD5Hash == mapHash);

                if (targetBeatmap == null)
                {
                    current++;
                    notification.Progress = (float)current / total;
                    continue;
                }

                var newScore = score.Detach();
                newScore.BeatmapInfo = targetBeatmap;

                if (existingRulesets.TryGetValue(newScore.Ruleset.ShortName, out var ruleset))
                    newScore.Ruleset = ruleset;

                newScore.Files.Clear();
                foreach (var fileUsage in score.Files)
                {
                    var dbFile = destRealm.Find<RealmFile>(fileUsage.File.Hash);
                    if (dbFile != null)
                    {
                        newScore.Files.Add(new RealmNamedFileUsage(dbFile, fileUsage.Filename));
                    }
                }

                destRealm.Add(newScore);
                existingIDs.Add(newScore.ID);

                current++;
                notification.Text = $"Importing scores ({current} of {total})";
                notification.Progress = (float)current / total;
            }

            notification.CompletionText = "Scores imported!";
            notification.State = ProgressNotificationState.Completed;
        }

        private void importSkins(Realm sourceRealm, Realm destRealm)
        {
            var sourceSkins = sourceRealm.All<SkinInfo>().Where(s => !s.DeletePending).AsEnumerable();
            int total = sourceSkins.Count();
            int current = 0;

            var notification = new ProgressNotification
            {
                Text = "Importing skins...",
                State = ProgressNotificationState.Active
            };
            notifications.Post(notification);

            var existingIDs = new HashSet<Guid>(destRealm.All<SkinInfo>().AsEnumerable().Select(s => s.ID));

            foreach (var skin in sourceSkins)
            {
                if (notification.State == ProgressNotificationState.Cancelled) return;

                // early return if skin (ID) already exists
                if (existingIDs.Contains(skin.ID))
                {
                    current++;
                    notification.Text = $"Importing skins ({current} of {total})";
                    notification.Progress = (float)current / total;
                    continue;
                }

                var newSkin = skin.Detach();

                newSkin.Files.Clear();
                foreach (var fileUsage in skin.Files)
                {
                    var dbFile = destRealm.Find<RealmFile>(fileUsage.File.Hash);
                    if (dbFile != null)
                    {
                        newSkin.Files.Add(new RealmNamedFileUsage(dbFile, fileUsage.Filename));
                    }
                }

                destRealm.Add(newSkin);
                existingIDs.Add(newSkin.ID);

                current++;
                notification.Text = $"Importing skins ({current} of {total})";
                notification.Progress = (float)current / total;
            }

            notification.CompletionText = "Skins imported!";
            notification.State = ProgressNotificationState.Completed;
        }

        private void importCollections(Realm sourceRealm, Realm destRealm)
        {
            var sourceCollections = sourceRealm.All<BeatmapCollection>().AsEnumerable();
            int total = sourceCollections.Count();
            int current = 0;

            var notification = new ProgressNotification
            {
                Text = "Importing collections...",
                State = ProgressNotificationState.Active
            };
            notifications.Post(notification);

            var existingCollections = destRealm.All<BeatmapCollection>()
                .ToList()
                .GroupBy(c => c.Name)
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var sourceCollection in sourceCollections)
            {
                if (notification.State == ProgressNotificationState.Cancelled) return;

                if (existingCollections.TryGetValue(sourceCollection.Name, out var destCollection))
                {
                    foreach (string hash in sourceCollection.BeatmapMD5Hashes)
                    {
                        if (!destCollection.BeatmapMD5Hashes.Contains(hash))
                        {
                            destCollection.BeatmapMD5Hashes.Add(hash);
                        }
                    }
                }
                else
                {
                    destCollection = new BeatmapCollection
                    {
                        ID = sourceCollection.ID,
                        Name = sourceCollection.Name,
                        LastModified = DateTimeOffset.UtcNow
                    };

                    foreach (string hash in sourceCollection.BeatmapMD5Hashes)
                    {
                        destCollection.BeatmapMD5Hashes.Add(hash);
                    }

                    destRealm.Add(destCollection);
                    existingCollections[destCollection.Name] = destCollection;
                }

                current++;
                notification.Text = $"Importing collections ({current} of {total})";
                notification.Progress = (float)current / total;
            }

            notification.CompletionText = "Collections imported!";
            notification.State = ProgressNotificationState.Completed;
        }


    }
}
