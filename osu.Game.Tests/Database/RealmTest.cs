// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NUnit.Framework;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Database;

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
            AsyncContext.Run(() =>
            {
                var testStorage = storage.GetStorageForDirectory(caller);

                using (var realmFactory = new RealmContextFactory(testStorage, caller))
                {
                    Logger.Log($"Running test using realm file {testStorage.GetFullPath(realmFactory.Filename)}");
                    testAction(realmFactory, testStorage);

                    realmFactory.Dispose();
                    Logger.Log($"Final database size: {testStorage.GetStream(realmFactory.Filename)?.Length ?? 0}");

                    realmFactory.Compact();
                    Logger.Log($"Final database size after compact: {testStorage.GetStream(realmFactory.Filename)?.Length ?? 0}");
                }
            });
        }

        protected void RunTestWithRealmAsync(Func<RealmContextFactory, Storage, Task> testAction, [CallerMemberName] string caller = "")
        {
            AsyncContext.Run(async () =>
            {
                var testStorage = storage.GetStorageForDirectory(caller);

                using (var realmFactory = new RealmContextFactory(testStorage, caller))
                {
                    Logger.Log($"Running test using realm file {testStorage.GetFullPath(realmFactory.Filename)}");

                    await testAction(realmFactory, testStorage);

                    realmFactory.Dispose();
                    Logger.Log($"Final database size: {testStorage.GetStream(realmFactory.Filename)?.Length ?? 0}");

                    realmFactory.Compact();
                    Logger.Log($"Final database size after compact: {testStorage.GetStream(realmFactory.Filename)?.Length ?? 0}");
                }
            });
        }
    }
}
