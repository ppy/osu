// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Select.Leaderboards;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneDrawableGameplayLeaderboardScore : OsuTestScene
    {
        private readonly APIUser user = new APIUser { Username = "user" };
        private readonly BindableLong totalScore = new BindableLong();
        private readonly Bindable<int?> position = new Bindable<int?>();
        private readonly BindableBool quit = new BindableBool();
        private readonly BindableBool expanded = new BindableBool();

        public TestSceneDrawableGameplayLeaderboardScore()
        {
            AddSliderStep("total score", 0, 1_000_000, 500_000, s => totalScore.Value = s);
            AddSliderStep("position", 1, 100, 5, s => position.Value = s);
            AddToggleStep("toggle quit", q => quit.Value = q);
            AddToggleStep("toggle expanded", e => expanded.Value = e);
        }

        private static readonly OsuColour osu_colour = new OsuColour();

        private static readonly object?[][] leaderboard_variants =
        {
            new object?[] { false, null },
            new object?[] { true, null },
            new object?[] { false, osu_colour.TeamColourRed },
            new object?[] { true, osu_colour.TeamColourRed },
            new object?[] { false, osu_colour.TeamColourBlue },
            new object?[] { true, osu_colour.TeamColourBlue },
        };

        [TestCaseSource(nameof(leaderboard_variants))]
        public void TestVariants(bool tracked, Color4? teamColour)
        {
            AddStep("show", () =>
            {
                GameplayLeaderboardScore score = new GameplayLeaderboardScore(user, tracked, totalScore)
                {
                    Position = { BindTarget = position },
                    HasQuit = { BindTarget = quit },
                    TeamColour = teamColour,
                };
                Child = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Y,
                    Width = 250,
                    Child = new DrawableGameplayLeaderboardScore(score)
                    {
                        Expanded = { BindTarget = expanded },
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                };
            });
        }
    }
}
