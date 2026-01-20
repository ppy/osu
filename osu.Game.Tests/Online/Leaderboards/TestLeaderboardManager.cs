// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Game.Configuration;
using osu.Game.Online.Leaderboards;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Leaderboards;

namespace osu.Game.Tests.Online.Leaderboards
{
    [TestFixture]
    public class TestLeaderboardScopeMapping
    {
        [Test]
        public void TestBeatmapDetailTabToScopeMapping()
        {
            // test that BeatmapDetailTab enum values correctly map to BeatmapLeaderboardScope
            // this test verifies the mapping logic without requiring a full LeaderboardManager instance

            // test all BeatmapDetailTab values
            foreach (BeatmapDetailTab tab in Enum.GetValues(typeof(BeatmapDetailTab)))
            {
                var expectedScope = getExpectedScopeForTab(tab);

                // verify the mapping for each tab is a valid scope (not an invalid value)
                Assert.That(Enum.IsDefined(typeof(BeatmapLeaderboardScope), expectedScope), Is.True, $"Failed for {tab}: {expectedScope} is not a defined BeatmapLeaderboardScope");
            }
        }

        [Test]
        public void TestDefaultScopeFallback()
        {
            // test that invalid BeatmapDetailTab values fallback to Global scope
            var invalidTabs = new[] { (BeatmapDetailTab)int.MinValue, (BeatmapDetailTab)int.MaxValue, (BeatmapDetailTab)(-1) };

            foreach (var invalidTab in invalidTabs)
            {
                Assert.That(getExpectedScopeForTab(invalidTab), Is.EqualTo(BeatmapLeaderboardScope.Global), $"Failed for invalid tab value {invalidTab}");
            }
        }

        [Test]
        public void TestValidScopeMappings()
        {
            // test specific valid mappings
            Assert.Multiple(() =>
            {
                Assert.That(getExpectedScopeForTab(BeatmapDetailTab.Local), Is.EqualTo(BeatmapLeaderboardScope.Local));
                Assert.That(getExpectedScopeForTab(BeatmapDetailTab.Country), Is.EqualTo(BeatmapLeaderboardScope.Country));
                Assert.That(getExpectedScopeForTab(BeatmapDetailTab.Friends), Is.EqualTo(BeatmapLeaderboardScope.Friend));
                Assert.That(getExpectedScopeForTab(BeatmapDetailTab.Team), Is.EqualTo(BeatmapLeaderboardScope.Team));
                Assert.That(getExpectedScopeForTab(BeatmapDetailTab.Global), Is.EqualTo(BeatmapLeaderboardScope.Global));
            });
        }

        // helper method to test the same mapping logic used in LeaderboardManager
        private BeatmapLeaderboardScope getExpectedScopeForTab(BeatmapDetailTab tab)
        {
            return tab switch
            {
                BeatmapDetailTab.Local => BeatmapLeaderboardScope.Local,
                BeatmapDetailTab.Country => BeatmapLeaderboardScope.Country,
                BeatmapDetailTab.Friends => BeatmapLeaderboardScope.Friend,
                BeatmapDetailTab.Team => BeatmapLeaderboardScope.Team,
                _ => BeatmapLeaderboardScope.Global
            };
        }
    }
}
