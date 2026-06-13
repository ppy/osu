// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneReplayBookmark : RateAdjustedBeatmapTestScene
    {
        private TestReplayPlayer player = null!;

        private ReplayBookmarkController bookmarkController
            => player.ChildrenOfType<ReplayBookmarkController>().Single();

        private void loadPlayer()
        {
            AddStep("load replay player", () =>
            {
                var ruleset = new OsuRuleset();
                Beatmap.Value = CreateWorkingBeatmap(ruleset.RulesetInfo);
                SelectedMods.Value = new[] { ruleset.GetAutoplayMod() };
                player = new TestReplayPlayer(false);
                LoadScreen(player);
            });

            AddUntilStep("wait for player loaded", () => player.IsLoaded);
        }

        [Test]
        public void TestAddBookmark()
        {
            loadPlayer();

            AddStep("seek to 5000", () => player.Seek(5000));
            AddStep("add bookmark", () => bookmarkController.AddBookmarkAtCurrentTime());

            AddAssert("one bookmark added", () => bookmarkController.Bookmarks.Count, () => Is.EqualTo(1));
            AddAssert("bookmark near 5000", () => bookmarkController.Bookmarks[0], () => Is.EqualTo(5000).Within(200));
        }

        [Test]
        public void TestNoDuplicateBookmark()
        {
            loadPlayer();

            AddStep("seek to 5000", () => player.Seek(5000));
            AddStep("add bookmark twice", () =>
            {
                bookmarkController.AddBookmarkAtCurrentTime();
                bookmarkController.AddBookmarkAtCurrentTime();
            });

            AddAssert("only one bookmark", () => bookmarkController.Bookmarks.Count, () => Is.EqualTo(1));
        }

        [Test]
        public void TestRemoveClosestBookmark()
        {
            loadPlayer();

            AddStep("add bookmark at 5000", () => bookmarkController.Bookmarks.Add(5000));
            AddStep("seek near bookmark", () => player.Seek(4500));
            AddStep("remove closest", () => bookmarkController.RemoveClosestBookmark());

            AddAssert("no bookmarks remain", () => bookmarkController.Bookmarks.Count, () => Is.EqualTo(0));
        }

        [Test]
        public void TestRemoveDoesNothingWhenFar()
        {
            loadPlayer();

            AddStep("add bookmark at 5000", () => bookmarkController.Bookmarks.Add(5000));
            AddStep("seek far from bookmark", () => player.Seek(10000));
            AddStep("remove closest", () => bookmarkController.RemoveClosestBookmark());

            AddAssert("bookmark still present", () => bookmarkController.Bookmarks.Count, () => Is.EqualTo(1));
        }

        [Test]
        public void TestSeekToNextBookmark()
        {
            loadPlayer();

            AddStep("add bookmarks", () =>
            {
                bookmarkController.Bookmarks.Add(3000);
                bookmarkController.Bookmarks.Add(7000);
            });
            AddStep("seek to start", () => player.Seek(0));
            AddStep("seek to next bookmark", () => bookmarkController.SeekBookmark(1));

            AddUntilStep("clock near first bookmark", () => player.GameplayClockContainer.CurrentTime, () => Is.EqualTo(3000).Within(500));
        }

        [Test]
        public void TestSeekToPreviousBookmark()
        {
            loadPlayer();

            AddStep("add bookmarks", () =>
            {
                bookmarkController.Bookmarks.Add(3000);
                bookmarkController.Bookmarks.Add(7000);
            });
            AddStep("seek past second bookmark", () => player.Seek(9000));
            AddStep("seek to previous bookmark", () => bookmarkController.SeekBookmark(-1));

            AddUntilStep("clock near second bookmark", () => player.GameplayClockContainer.CurrentTime, () => Is.EqualTo(7000).Within(500));
        }

        [Test]
        public void TestSeekWithNoBookmarksDoesNotThrow()
        {
            loadPlayer();

            AddAssert("no bookmarks", () => bookmarkController.Bookmarks.Count, () => Is.EqualTo(0));
            AddStep("seek forward (no-op)", () => bookmarkController.SeekBookmark(1));
            AddStep("seek backward (no-op)", () => bookmarkController.SeekBookmark(-1));
        }
    }
}
