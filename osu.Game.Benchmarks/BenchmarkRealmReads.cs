// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading;
using BenchmarkDotNet.Attributes;
using osu.Framework.Testing;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Rulesets.Osu;
using osu.Game.Tests.Resources;

namespace osu.Game.Benchmarks
{
    public class BenchmarkRealmReads : BenchmarkTest
    {
        private TemporaryNativeStorage storage;
        private RealmAccess realm;
        private UpdateThread updateThread;

        [Params(1, 100, 1000)]
        public int ReadsPerFetch { get; set; }

        public override void SetUp()
        {
            storage = new TemporaryNativeStorage("realm-benchmark");
            storage.DeleteDirectory(string.Empty);

            realm = new RealmAccess(storage, "client");

            realm.Run(r =>
            {
                realm.Write(c => c.Add(TestResources.CreateTestBeatmapSetInfo(rulesets: new[] { new OsuRuleset().RulesetInfo })));
            });

            updateThread = new UpdateThread(() => { }, null);
            updateThread.Start();
        }

        [Benchmark]
        public void BenchmarkDirectPropertyRead()
        {
            realm.Run(r =>
            {
                var beatmapSet = r.All<BeatmapSetInfo>().First();

                for (int i = 0; i < ReadsPerFetch; i++)
                {
                    string _ = beatmapSet.Beatmaps.First().Hash;
                }
            });
        }

        [Benchmark]
        public void BenchmarkDirectPropertyReadUpdateThread()
        {
            var done = new ManualResetEventSlim();

            updateThread.Scheduler.Add(() =>
            {
                try
                {
                    var beatmapSet = realm.Realm.All<BeatmapSetInfo>().First();

                    for (int i = 0; i < ReadsPerFetch; i++)
                    {
                        string _ = beatmapSet.Beatmaps.First().Hash;
                    }
                }
                finally
                {
                    done.Set();
                }
            });

            done.Wait();
        }

        [Benchmark]
        public void BenchmarkRealmLivePropertyRead()
        {
            realm.Run(r =>
            {
                var beatmapSet = r.All<BeatmapSetInfo>().First().ToLive(realm);

                for (int i = 0; i < ReadsPerFetch; i++)
                {
                    string _ = beatmapSet.PerformRead(b => b.Beatmaps.First().Hash);
                }
            });
        }

        [Benchmark]
        public void BenchmarkRealmLivePropertyReadUpdateThread()
        {
            var done = new ManualResetEventSlim();

            updateThread.Scheduler.Add(() =>
            {
                try
                {
                    var beatmapSet = realm.Realm.All<BeatmapSetInfo>().First().ToLive(realm);

                    for (int i = 0; i < ReadsPerFetch; i++)
                    {
                        string _ = beatmapSet.PerformRead(b => b.Beatmaps.First().Hash);
                    }
                }
                finally
                {
                    done.Set();
                }
            });

            done.Wait();
        }

        [Benchmark]
        public void BenchmarkDetachedPropertyRead()
        {
            realm.Run(r =>
            {
                var beatmapSet = r.All<BeatmapSetInfo>().First().Detach();

                for (int i = 0; i < ReadsPerFetch; i++)
                {
                    string _ = beatmapSet.Beatmaps.First().Hash;
                }
            });
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            realm?.Dispose();
            storage?.Dispose();
            updateThread?.Exit();
        }
    }
}
