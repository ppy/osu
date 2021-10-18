// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Database;
using osu.Game.Models;

#nullable enable

namespace osu.Game.Tests.Database
{
    [TestFixture]
    public abstract class RealmTest
    {
        private static readonly TemporaryNativeStorage storage;

        static RealmTest()
        {
            storage = new TemporaryNativeStorage("realm-test");
            storage.DeleteDirectory(string.Empty);
        }

        protected void RunTestWithRealm(Action<RealmContextFactory, Storage> testAction, [CallerMemberName] string caller = "")
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost(caller))
            {
                host.Run(new RealmTestGame(() =>
                {
                    var testStorage = storage.GetStorageForDirectory(caller);

                    using (var realmFactory = new RealmContextFactory(testStorage, caller))
                    {
                        Logger.Log($"Running test using realm file {testStorage.GetFullPath(realmFactory.Filename)}");
                        testAction(realmFactory, testStorage);

                        realmFactory.Dispose();

                        Logger.Log($"Final database size: {getFileSize(testStorage, realmFactory)}");
                        realmFactory.Compact();
                        Logger.Log($"Final database size after compact: {getFileSize(testStorage, realmFactory)}");
                    }
                }));
            }
        }

        protected void RunTestWithRealmAsync(Func<RealmContextFactory, Storage, Task> testAction, [CallerMemberName] string caller = "")
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost(caller))
            {
                host.Run(new RealmTestGame(async () =>
                {
                    var testStorage = storage.GetStorageForDirectory(caller);

                    using (var realmFactory = new RealmContextFactory(testStorage, caller))
                    {
                        Logger.Log($"Running test using realm file {testStorage.GetFullPath(realmFactory.Filename)}");
                        await testAction(realmFactory, testStorage);

                        realmFactory.Dispose();

                        Logger.Log($"Final database size: {getFileSize(testStorage, realmFactory)}");
                        realmFactory.Compact();
                    }
                }));
            }
        }

        protected static RealmBeatmapSet CreateBeatmapSet(RealmRuleset ruleset)
        {
            RealmFile createRealmFile() => new RealmFile { Hash = Guid.NewGuid().ToString().ComputeSHA2Hash() };

            var metadata = new RealmBeatmapMetadata
            {
                Title = "My Love",
                Artist = "Kuba Oms"
            };

            var beatmapSet = new RealmBeatmapSet
            {
                Beatmaps =
                {
                    new RealmBeatmap(ruleset, new RealmBeatmapDifficulty(), metadata) { DifficultyName = "Easy", },
                    new RealmBeatmap(ruleset, new RealmBeatmapDifficulty(), metadata) { DifficultyName = "Normal", },
                    new RealmBeatmap(ruleset, new RealmBeatmapDifficulty(), metadata) { DifficultyName = "Hard", },
                    new RealmBeatmap(ruleset, new RealmBeatmapDifficulty(), metadata) { DifficultyName = "Insane", }
                },
                Files =
                {
                    new RealmNamedFileUsage(createRealmFile(), "test [easy].osu"),
                    new RealmNamedFileUsage(createRealmFile(), "test [normal].osu"),
                    new RealmNamedFileUsage(createRealmFile(), "test [hard].osu"),
                    new RealmNamedFileUsage(createRealmFile(), "test [insane].osu"),
                }
            };

            for (int i = 0; i < 8; i++)
                beatmapSet.Files.Add(new RealmNamedFileUsage(createRealmFile(), $"hitsound{i}.mp3"));

            foreach (var b in beatmapSet.Beatmaps)
                b.BeatmapSet = beatmapSet;

            return beatmapSet;
        }

        protected static RealmRuleset CreateRuleset() =>
            new RealmRuleset(0, "osu!", "osu", true);

        private class RealmTestGame : Framework.Game
        {
            public RealmTestGame(Func<Task> work)
            {
                // ReSharper disable once AsyncVoidLambda
                Scheduler.Add(async () =>
                {
                    await work().ConfigureAwait(true);
                    Exit();
                });
            }

            public RealmTestGame(Action work)
            {
                Scheduler.Add(() =>
                {
                    work();
                    Exit();
                });
            }
        }

        private static long getFileSize(Storage testStorage, RealmContextFactory realmFactory)
        {
            try
            {
                using (var stream = testStorage.GetStream(realmFactory.Filename))
                    return stream?.Length ?? 0;
            }
            catch
            {
                // windows runs may error due to file still being open.
                return 0;
            }
        }
    }
}
