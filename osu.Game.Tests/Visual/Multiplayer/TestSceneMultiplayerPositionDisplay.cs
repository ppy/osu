// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Screens.Play;
using osu.Game.Screens.Select.Leaderboards;
using osu.Game.Tests.Gameplay;
using osu.Game.Tests.Visual.Gameplay;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public partial class TestSceneMultiplayerPositionDisplay : OsuTestScene
    {
        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        private GameplayLeaderboardScore score = null!;

        private readonly Bindable<int?> position = new Bindable<int?>(8);

        private TestSceneGameplayLeaderboard.TestGameplayLeaderboardProvider leaderboardProvider = null!;
        private MultiplayerPositionDisplay display = null!;
        private GameplayState gameplayState = null!;

        private const int player_count = 32;

        [Test]
        public void TestAppearance()
        {
            AddStep("create content", () =>
            {
                Children = new Drawable[]
                {
                    new DependencyProvidingContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        CachedDependencies =
                        [
                            (typeof(IGameplayLeaderboardProvider), leaderboardProvider = new TestSceneGameplayLeaderboard.TestGameplayLeaderboardProvider()),
                            (typeof(GameplayState), gameplayState = TestGameplayState.Create(new OsuRuleset()))
                        ],
                        Child = display = new MultiplayerPositionDisplay
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        }
                    }
                };

                score = leaderboardProvider.CreateLeaderboardScore(new BindableLong(), API.LocalUser.Value, true);
                score.Position.BindTo(position);

                for (int i = 0; i < player_count - 1; i++)
                {
                    var r = leaderboardProvider.CreateRandomScore(new APIUser());
                    r.Position.Value = i;
                }
            });

            AddSliderStep("set score position", 1, player_count, position.Value!.Value, r => position.Value = r);
            AddStep("unset position", () => position.Value = null);

            AddStep("toggle leaderboardProvider on", () => config.SetValue(OsuSetting.GameplayLeaderboard, true));
            AddUntilStep("display visible", () => display.Alpha, () => Is.EqualTo(1));

            AddStep("toggle leaderboardProvider off", () => config.SetValue(OsuSetting.GameplayLeaderboard, false));
            AddUntilStep("display hidden", () => display.Alpha, () => Is.EqualTo(0));

            AddStep("enter break", () => ((Bindable<LocalUserPlayingState>)gameplayState.PlayingState).Value = LocalUserPlayingState.Break);
            AddUntilStep("display visible", () => display.Alpha, () => Is.EqualTo(1));

            AddStep("exit break", () => ((Bindable<LocalUserPlayingState>)gameplayState.PlayingState).Value = LocalUserPlayingState.Playing);
            AddUntilStep("display hidden", () => display.Alpha, () => Is.EqualTo(0));

            AddStep("toggle leaderboardProvider on", () => config.SetValue(OsuSetting.GameplayLeaderboard, true));
            AddUntilStep("display visible", () => display.Alpha, () => Is.EqualTo(1));

            AddStep("change local user", () => ((DummyAPIAccess)API).LocalUser.Value = new GuestUser());
            AddUntilStep("display hidden", () => display.Alpha, () => Is.EqualTo(0));
        }

        [Test]
        public void TestTwoPlayers()
        {
            AddStep("create content", () =>
            {
                Children = new Drawable[]
                {
                    new DependencyProvidingContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        CachedDependencies =
                        [
                            (typeof(IGameplayLeaderboardProvider), leaderboardProvider = new TestSceneGameplayLeaderboard.TestGameplayLeaderboardProvider()),
                            (typeof(GameplayState), gameplayState = TestGameplayState.Create(new OsuRuleset()))
                        ],
                        Child = display = new MultiplayerPositionDisplay
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        }
                    }
                };

                score = leaderboardProvider.CreateLeaderboardScore(new BindableLong(), API.LocalUser.Value, true);
                score.Position.BindTo(position);

                var r = leaderboardProvider.CreateRandomScore(new APIUser());
                r.Position.Value = 1;
            });

            AddStep("first place", () => position.Value = 1);
            AddStep("second place", () => position.Value = 2);
        }
    }
}
