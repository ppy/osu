// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Online.API;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Screens.Play;
using osu.Game.Screens.Select.Leaderboards;
using osu.Game.Tests.Gameplay;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public partial class TestSceneMultiplayerPositionDisplay : OsuTestScene
    {
        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        [Test]
        public void TestAppearance()
        {
            TestGameplayLeaderboardProvider leaderboard = null!;
            MultiplayerPositionDisplay display = null!;
            GameplayState gameplayState = null!;

            AddStep("create content", () =>
            {
                leaderboard = new TestGameplayLeaderboardProvider();
                Children = new Drawable[]
                {
                    leaderboard,
                    new DependencyProvidingContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        CachedDependencies =
                        [
                            (typeof(IGameplayLeaderboardProvider), leaderboard),
                            (typeof(GameplayState), gameplayState = TestGameplayState.Create(new OsuRuleset()))
                        ],
                        Child = display = new MultiplayerPositionDisplay
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        }
                    }
                };
            });
            AddSliderStep("set score position", 1, 100, 50, r =>
            {
                if (leaderboard.IsNotNull() && leaderboard.Score.IsNotNull())
                    leaderboard.Score.Position.Value = r;
            });
            AddStep("unset position", () => leaderboard.Score.Position.Value = null);

            AddStep("toggle leaderboard on", () => config.SetValue(OsuSetting.GameplayLeaderboard, true));
            AddUntilStep("display visible", () => display.Alpha, () => Is.EqualTo(1));

            AddStep("toggle leaderboard off", () => config.SetValue(OsuSetting.GameplayLeaderboard, false));
            AddUntilStep("display hidden", () => display.Alpha, () => Is.EqualTo(0));

            AddStep("enter break", () => ((Bindable<LocalUserPlayingState>)gameplayState.PlayingState).Value = LocalUserPlayingState.Break);
            AddUntilStep("display visible", () => display.Alpha, () => Is.EqualTo(1));

            AddStep("exit break", () => ((Bindable<LocalUserPlayingState>)gameplayState.PlayingState).Value = LocalUserPlayingState.Playing);
            AddUntilStep("display hidden", () => display.Alpha, () => Is.EqualTo(0));

            AddStep("toggle leaderboard on", () => config.SetValue(OsuSetting.GameplayLeaderboard, true));
            AddUntilStep("display visible", () => display.Alpha, () => Is.EqualTo(1));

            AddStep("change local user", () => ((DummyAPIAccess)API).LocalUser.Value = new GuestUser());
            AddUntilStep("display hidden", () => display.Alpha, () => Is.EqualTo(0));
        }

        private partial class TestGameplayLeaderboardProvider : Component, IGameplayLeaderboardProvider
        {
            public GameplayLeaderboardScore Score { get; private set; } = null!;

            IBindableList<GameplayLeaderboardScore> IGameplayLeaderboardProvider.Scores => scores;
            private readonly BindableList<GameplayLeaderboardScore> scores = new BindableList<GameplayLeaderboardScore>();

            [BackgroundDependencyLoader]
            private void load(IAPIProvider api)
            {
                scores.Add(Score = new GameplayLeaderboardScore(api.LocalUser.Value, true, new BindableLong()));
            }
        }
    }
}
