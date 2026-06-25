// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.API;
using osu.Game.Rulesets.Osu;
using osu.Game.Scoring;
using osu.Game.Screens.Footer;
using osu.Game.Screens.Play.Leaderboards;
using osu.Game.Screens.Select;
using osuTK.Input;

namespace osu.Game.Tests.Visual.SongSelect
{
    public partial class TestSceneSongSelectBackgroundPreview : SongSelectTestScene
    {
        [Test]
        public void TestLeaderboardScoresBlockBackgroundPreview()
        {
            LoadSongSelect();
            ImportBeatmapForRuleset(0);

            AddAssert("beatmap imported", () => Beatmaps.GetAllUsableBeatmapSets().Any(), () => Is.True);

            AddAssert("beatmap selected", () => !Beatmap.IsDefault);

            AddStep("import score", () =>
            {
                var beatmapInfo = Beatmaps.GetAllUsableBeatmapSets().Single().Beatmaps.First();
                ScoreManager.Import(new ScoreInfo
                {
                    Hash = Guid.NewGuid().ToString(),
                    BeatmapHash = beatmapInfo.Hash,
                    BeatmapInfo = beatmapInfo,
                    Ruleset = new OsuRuleset().RulesetInfo,
                    User = new GuestUser(),
                });
            });

            AddStep("select ranking tab", () =>
            {
                InputManager.MoveMouseTo(SongSelect.ChildrenOfType<BeatmapDetailsArea.WedgeSelector<BeatmapDetailsArea.Header.Selection>>().Last());
                InputManager.Click(MouseButton.Left);
            });

            AddStep("set local scope", () =>
            {
                var current = LeaderboardManager.CurrentCriteria!;
                LeaderboardManager.FetchWithCriteria(current with
                {
                    Scope = BeatmapLeaderboardScope.Local,
                });
            });

            AddUntilStep("wait for scores", () => SongSelect.ChildrenOfType<BeatmapLeaderboardScore>().Any());

            double? clickTime = null;
            AddStep("click score", () =>
            {
                InputManager.MoveMouseTo(SongSelect.ChildrenOfType<BeatmapLeaderboardScore>().Single());
                InputManager.PressButton(MouseButton.Left);
                clickTime = InputManager.Time.Current;
            });
            AddUntilStep("wait for background preview", () => InputManager.Time.Current, () => Is.GreaterThan((clickTime + 3 * Screens.Select.SongSelect.REVEAL_BACKGROUND_DELAY) ?? double.PositiveInfinity));
            AddAssert("background preview not triggered", () => this.ChildrenOfType<ScreenFooter>().Single().State.Value, () => Is.EqualTo(Visibility.Visible));
            AddStep("release mouse button", () => InputManager.ReleaseButton(MouseButton.Left));
        }

        [Test]
        public void TestShearedDropdownBlocksBackgroundPreview()
        {
            LoadSongSelect();
            ImportBeatmapForRuleset(0);

            AddAssert("beatmap imported", () => Beatmaps.GetAllUsableBeatmapSets().Any(), () => Is.True);

            AddAssert("beatmap selected", () => !Beatmap.IsDefault);

            AddStep("import score", () =>
            {
                var beatmapInfo = Beatmaps.GetAllUsableBeatmapSets().Single().Beatmaps.First();
                ScoreManager.Import(new ScoreInfo
                {
                    Hash = Guid.NewGuid().ToString(),
                    BeatmapHash = beatmapInfo.Hash,
                    BeatmapInfo = beatmapInfo,
                    Ruleset = new OsuRuleset().RulesetInfo,
                    User = new GuestUser(),
                });
            });

            AddStep("select ranking tab", () =>
            {
                InputManager.MoveMouseTo(SongSelect.ChildrenOfType<BeatmapDetailsArea.WedgeSelector<BeatmapDetailsArea.Header.Selection>>().Last());
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("wait for leaderboard controls", () => SongSelect.ChildrenOfType<BeatmapDetailsArea.Header>().Any());

            double? clickTime = null;
            AddStep("click scope", () =>
            {
                var header = SongSelect.ChildrenOfType<BeatmapDetailsArea.Header>().Single();
                var scopeDropdown = header.ChildrenOfType<ShearedDropdown<BeatmapLeaderboardScope>>().Single();
                InputManager.MoveMouseTo(scopeDropdown);
                InputManager.PressButton(MouseButton.Left);
                clickTime = InputManager.Time.Current;
            });
            AddUntilStep("wait for background preview", () => InputManager.Time.Current, () => Is.GreaterThan((clickTime + 3 * Screens.Select.SongSelect.REVEAL_BACKGROUND_DELAY) ?? double.PositiveInfinity));
            AddAssert("background preview not triggered", () => this.ChildrenOfType<ScreenFooter>().Single().State.Value, () => Is.EqualTo(Visibility.Visible));
            AddStep("release mouse button", () => InputManager.ReleaseButton(MouseButton.Left));
        }

        [Test]
        public void TestToggleButtonBlocksBackgroundPreview()
        {
            LoadSongSelect();
            ImportBeatmapForRuleset(0);

            AddAssert("beatmap imported", () => Beatmaps.GetAllUsableBeatmapSets().Any(), () => Is.True);

            AddAssert("beatmap selected", () => !Beatmap.IsDefault);

            AddStep("import score", () =>
            {
                var beatmapInfo = Beatmaps.GetAllUsableBeatmapSets().Single().Beatmaps.First();
                ScoreManager.Import(new ScoreInfo
                {
                    Hash = Guid.NewGuid().ToString(),
                    BeatmapHash = beatmapInfo.Hash,
                    BeatmapInfo = beatmapInfo,
                    Ruleset = new OsuRuleset().RulesetInfo,
                    User = new GuestUser(),
                });
            });

            AddStep("select ranking tab", () =>
            {
                InputManager.MoveMouseTo(SongSelect.ChildrenOfType<BeatmapDetailsArea.WedgeSelector<BeatmapDetailsArea.Header.Selection>>().Last());
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("wait for leaderboard controls", () => SongSelect.ChildrenOfType<BeatmapDetailsArea.Header>().Any());

            double? clickTime = null;
            AddStep("click selected mods", () =>
            {
                var header = SongSelect.ChildrenOfType<BeatmapDetailsArea.Header>().Single();
                var toggleButton = header.ChildrenOfType<ShearedToggleButton>().Single();
                InputManager.MoveMouseTo(toggleButton);
                InputManager.PressButton(MouseButton.Left);
                clickTime = InputManager.Time.Current;
            });
            AddUntilStep("wait for background preview", () => InputManager.Time.Current, () => Is.GreaterThan((clickTime + 3 * Screens.Select.SongSelect.REVEAL_BACKGROUND_DELAY) ?? double.PositiveInfinity));
            AddAssert("background preview not triggered", () => this.ChildrenOfType<ScreenFooter>().Single().State.Value, () => Is.EqualTo(Visibility.Visible));
            AddStep("release mouse button", () => InputManager.ReleaseButton(MouseButton.Left));
        }

        [Test]
        public void TestLeaderboardWedgeTriggersBackgroundPreview()
        {
            LoadSongSelect();
            ImportBeatmapForRuleset(0);

            AddAssert("beatmap imported", () => Beatmaps.GetAllUsableBeatmapSets().Any(), () => Is.True);

            AddAssert("beatmap selected", () => !Beatmap.IsDefault);

            AddStep("select ranking tab", () =>
            {
                InputManager.MoveMouseTo(SongSelect.ChildrenOfType<BeatmapDetailsArea.WedgeSelector<BeatmapDetailsArea.Header.Selection>>().Last());
                InputManager.Click(MouseButton.Left);
            });

            AddStep("click leaderboard background", () =>
            {
                var leaderboardWedge = SongSelect.ChildrenOfType<BeatmapLeaderboardWedge>().Single();
                // No scores, click on empty space
                InputManager.MoveMouseTo(leaderboardWedge.ScreenSpaceDrawQuad.Centre);
                InputManager.PressButton(MouseButton.Left);
            });

            AddUntilStep("background preview triggered", () => this.ChildrenOfType<ScreenFooter>().Single().State.Value, () => Is.EqualTo(Visibility.Hidden));
            AddStep("release mouse button", () => InputManager.ReleaseButton(MouseButton.Left));
        }
    }
}
