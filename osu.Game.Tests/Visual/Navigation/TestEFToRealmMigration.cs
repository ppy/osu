// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Runtime.InteropServices;
using NUnit.Framework;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Models;
using osu.Game.Scoring;
using osu.Game.Skinning;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.Navigation
{
    public class TestEFToRealmMigration : OsuGameTestScene
    {
        public override void RecycleLocalStorage(bool isDisposing)
        {
            base.RecycleLocalStorage(isDisposing);

            if (isDisposing)
                return;

            using (var outStream = LocalStorage.CreateFileSafely(DatabaseContextFactory.DATABASE_NAME))
            using (var stream = TestResources.OpenResource(DatabaseContextFactory.DATABASE_NAME))
                stream.CopyTo(outStream);
        }

        [SetUp]
        public void SetUp()
        {
            if (RuntimeInfo.OS == RuntimeInfo.Platform.macOS && RuntimeInformation.OSArchitecture == Architecture.Arm64)
                Assert.Ignore("EF-to-realm migrations are not supported on M1 ARM architectures.");
        }

        public override void SetUpSteps()
        {
            // base SetUpSteps are executed before the above SetUp, therefore early-return to allow ignoring test properly.
            // attempting to ignore here would yield a TargetInvocationException instead.
            if (RuntimeInfo.OS == RuntimeInfo.Platform.macOS && RuntimeInformation.OSArchitecture == Architecture.Arm64)
                return;

            base.SetUpSteps();
        }

        [Test]
        public void TestMigration()
        {
            // Numbers are taken from the test database (see commit f03de16ee5a46deac3b5f2ca1edfba5c4c5dca7d).
            AddAssert("Check beatmaps", () => Game.Dependencies.Get<RealmAccess>().Run(r => r.All<BeatmapSetInfo>().Count(s => !s.Protected) == 1));
            AddAssert("Check skins", () => Game.Dependencies.Get<RealmAccess>().Run(r => r.All<SkinInfo>().Count(s => !s.Protected) == 1));
            AddAssert("Check scores", () => Game.Dependencies.Get<RealmAccess>().Run(r => r.All<ScoreInfo>().Count() == 1));

            // One extra file is created during realm migration / startup due to the circles intro import.
            AddAssert("Check files", () => Game.Dependencies.Get<RealmAccess>().Run(r => r.All<RealmFile>().Count() == 271));
        }
    }
}
