// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Database;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Skins
{
    [HeadlessTest]
    public partial class TestSceneSkinPinning : OsuTestScene
    {
        [Resolved]
        private SkinManager skins { get; set; } = null!;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        private List<Guid> seededIds = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            // Realm persists across tests in this fixture, so reset pin state up-front so
            // ordering / cycle assertions never pick up stale items.
            AddStep("reset pin state", () => realm.Write(r =>
            {
                foreach (var s in r.All<SkinInfo>())
                    s.Pinned = false;
            }));

            AddStep("seed test skins", () =>
            {
                string tag = $"pinning-{Guid.NewGuid():N}";
                seededIds = new List<Guid>();

                realm.Write(r =>
                {
                    foreach (string suffix in new[] { "alpha", "bravo", "charlie" })
                    {
                        var info = new SkinInfo(name: $"{tag}-{suffix}", creator: "test", instantiationInfo: typeof(TrianglesSkin).AssemblyQualifiedName);
                        r.Add(info);
                        seededIds.Add(info.ID);
                    }
                });
            });
        }

        [Test]
        public void TestNewSkinIsUnpinnedByDefault()
        {
            AddAssert("seeded skins start unpinned", () => realm.Run(r => seededIds.All(id => !r.Find<SkinInfo>(id)!.Pinned)));
        }

        [Test]
        public void TestTogglePinnedFlipsState()
        {
            AddStep("toggle pin on bravo", () => skins.TogglePinned(liveSkin(1)));
            AddAssert("bravo is pinned", () => realm.Run(r => r.Find<SkinInfo>(seededIds[1])!.Pinned));

            AddStep("toggle pin on bravo again", () => skins.TogglePinned(liveSkin(1)));
            AddAssert("bravo is unpinned", () => realm.Run(r => !r.Find<SkinInfo>(seededIds[1])!.Pinned));
        }

        [Test]
        public void TestPinnedSkinsSurfaceFirst()
        {
            AddStep("pin charlie", () => realm.Write(r => r.Find<SkinInfo>(seededIds[2])!.Pinned = true));

            AddAssert("charlie precedes other seeded skins", () =>
            {
                var ids = seededIdsInDropdownOrder();
                int charlie = ids.IndexOf(seededIds[2]);
                return charlie >= 0 && ids.IndexOf(seededIds[0]) > charlie && ids.IndexOf(seededIds[1]) > charlie;
            });
        }

        [Test]
        public void TestPinnedBucketKeepsAlphabeticalOrder()
        {
            AddStep("pin alpha and charlie", () => realm.Write(r =>
            {
                r.Find<SkinInfo>(seededIds[0])!.Pinned = true;
                r.Find<SkinInfo>(seededIds[2])!.Pinned = true;
            }));

            AddAssert("alpha precedes charlie ahead of bravo", () =>
            {
                var ids = seededIdsInDropdownOrder();
                int alpha = ids.IndexOf(seededIds[0]);
                int charlie = ids.IndexOf(seededIds[2]);
                int bravo = ids.IndexOf(seededIds[1]);
                return alpha >= 0 && charlie > alpha && bravo > charlie;
            });
        }

        [Test]
        public void TestCycleVisitsEverySkinByDefault()
        {
            assertCycleVisitsAllSeededSkins(favouritesOnly: false);
        }

        [Test]
        public void TestCycleRestrictsToFavouritesWhenEnoughArePinned()
        {
            AddStep("pin alpha and charlie", () => realm.Write(r =>
            {
                r.Find<SkinInfo>(seededIds[0])!.Pinned = true;
                r.Find<SkinInfo>(seededIds[2])!.Pinned = true;
            }));
            AddStep("select alpha", () => skins.CurrentSkinInfo.Value = liveSkin(0));

            AddStep("cycle next", () => skins.SelectNextSkin(favouritesOnly: true));
            AddAssert("now on a pinned skin", () => skins.CurrentSkinInfo.Value.PerformRead(s => s.Pinned));

            AddStep("cycle next again", () => skins.SelectNextSkin(favouritesOnly: true));
            AddAssert("still on a pinned skin", () => skins.CurrentSkinInfo.Value.PerformRead(s => s.Pinned));
        }

        [Test]
        public void TestCycleFallsBackToAllSkinsWithFewerThanTwoPinned()
        {
            AddStep("pin only alpha", () => realm.Write(r => r.Find<SkinInfo>(seededIds[0])!.Pinned = true));

            assertCycleVisitsAllSeededSkins(favouritesOnly: true);
        }

        private void assertCycleVisitsAllSeededSkins(bool favouritesOnly)
        {
            HashSet<Guid> visited = null!;

            AddStep("select alpha", () => skins.CurrentSkinInfo.Value = liveSkin(0));
            AddStep("walk the cycle", () =>
            {
                visited = new HashSet<Guid> { skins.CurrentSkinInfo.Value.ID };

                int totalSkins = skins.GetAllUsableSkins().Count;

                for (int i = 0; i < totalSkins * 2; i++)
                {
                    skins.SelectNextSkin(favouritesOnly);
                    visited.Add(skins.CurrentSkinInfo.Value.ID);
                }
            });
            AddAssert("every seeded skin was reached", () => seededIds.All(id => visited.Contains(id)));
        }

        private List<Guid> seededIdsInDropdownOrder()
            => skins.GetAllUsableSkins()
                    .Select(s => s.Value.ID)
                    .Where(id => seededIds.Contains(id))
                    .ToList();

        private Live<SkinInfo> liveSkin(int index)
            => realm.Run(r => r.Find<SkinInfo>(seededIds[index])!.ToLive(realm));
    }
}
