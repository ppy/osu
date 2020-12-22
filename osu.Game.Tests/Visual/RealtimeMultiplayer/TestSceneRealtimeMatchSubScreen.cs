// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Multi.RealtimeMultiplayer;
using osu.Game.Screens.Multi.RealtimeMultiplayer.Match;
using osu.Game.Tests.Beatmaps;
using osuTK.Input;

namespace osu.Game.Tests.Visual.RealtimeMultiplayer
{
    public class TestSceneRealtimeMatchSubScreen : RealtimeMultiplayerTestScene
    {
        private RealtimeMatchSubScreen screen;

        public TestSceneRealtimeMatchSubScreen()
            : base(false)
        {
        }

        [SetUp]
        public new void Setup() => Schedule(() =>
        {
            Room.Name.Value = "Test Room";
        });

        [SetUpSteps]
        public void SetupSteps()
        {
            AddStep("load match", () => LoadScreen(screen = new RealtimeMatchSubScreen(Room)));
            AddUntilStep("wait for load", () => screen.IsCurrentScreen());
        }

        [Test]
        public void TestSettingValidity()
        {
            AddAssert("create button not enabled", () => !this.ChildrenOfType<RealtimeMatchSettingsOverlay.CreateOrUpdateButton>().Single().Enabled.Value);

            AddStep("set playlist", () =>
            {
                Room.Playlist.Add(new PlaylistItem
                {
                    Beatmap = { Value = new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo },
                    Ruleset = { Value = new OsuRuleset().RulesetInfo },
                });
            });

            AddAssert("create button enabled", () => this.ChildrenOfType<RealtimeMatchSettingsOverlay.CreateOrUpdateButton>().Single().Enabled.Value);
        }

        [Test]
        public void TestCreatedRoom()
        {
            AddStep("set playlist", () =>
            {
                Room.Playlist.Add(new PlaylistItem
                {
                    Beatmap = { Value = new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo },
                    Ruleset = { Value = new OsuRuleset().RulesetInfo },
                });
            });

            AddStep("click create button", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<RealtimeMatchSettingsOverlay.CreateOrUpdateButton>().Single());
                InputManager.Click(MouseButton.Left);
            });

            AddWaitStep("wait", 10);
        }
    }
}
