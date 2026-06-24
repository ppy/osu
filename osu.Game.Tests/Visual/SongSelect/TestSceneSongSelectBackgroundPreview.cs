// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.API;
using osu.Game.Rulesets.Osu;
using osu.Game.Scoring;
using osu.Game.Screens.Play.Leaderboards;
using osu.Game.Screens.Select;
using osuTK.Input;

namespace osu.Game.Tests.Visual.SongSelect
{
    public partial class TestSceneSongSelectBackgroundPreview : SongSelectTestScene
    {
        private const int backround_preview_delay_seconds = 1;

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
            AddStep("click score", () =>
            {
                InputManager.MoveMouseTo(SongSelect.ChildrenOfType<BeatmapLeaderboardScore>().Single());
                InputManager.PressButton(MouseButton.Left);
            });
            AddWaitStep("wait for background preview", backround_preview_delay_seconds);

            AddAssert("background preview not triggered", () => InputManager.HoveredDrawables.Any(hovered => hovered is BeatmapLeaderboardScore));
            InputManager.ReleaseButton(MouseButton.Left);
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
            AddStep("click scope", () =>
            {
                var header = SongSelect.ChildrenOfType<BeatmapDetailsArea.Header>().Single();
                var scopeDropdown = header.ChildrenOfType<ShearedDropdown<BeatmapLeaderboardScope>>().Single();
                InputManager.MoveMouseTo(scopeDropdown);
                InputManager.PressButton(MouseButton.Left);
            });

            AddWaitStep("wait for background preview", backround_preview_delay_seconds);
            AddAssert("background preview not triggered", () =>
            {
                return InputManager.HoveredDrawables.Any(hovered => hovered is ShearedDropdown<BeatmapLeaderboardScope>);
            });
            InputManager.ReleaseButton(MouseButton.Left);
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
            AddStep("click selected mods", () =>
            {
                var header = SongSelect.ChildrenOfType<BeatmapDetailsArea.Header>().Single();
                var toggleButton = header.ChildrenOfType<ShearedToggleButton>().Single();
                InputManager.MoveMouseTo(toggleButton);
                InputManager.PressButton(MouseButton.Left);
            });

            AddWaitStep("wait for background preview", backround_preview_delay_seconds);
            AddAssert("background preview not triggered", () =>
            {
                return InputManager.HoveredDrawables.Any(hovered => hovered is ShearedToggleButton);
            });
            InputManager.ReleaseButton(MouseButton.Left);
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

            AddAssert("hovering leaderboard wedge", () => InputManager.HoveredDrawables.Any(hovered => hovered is BeatmapLeaderboardWedge));
            AddWaitStep("wait for background preview", backround_preview_delay_seconds);
            AddAssert("background preview triggered", () => !InputManager.HoveredDrawables.Any(hovered => hovered is BeatmapLeaderboardWedge));
            InputManager.ReleaseButton(MouseButton.Left);
        }
    }
}
