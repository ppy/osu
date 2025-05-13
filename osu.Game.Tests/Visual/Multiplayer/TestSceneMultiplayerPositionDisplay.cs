// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Online.API;
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

        private readonly Bindable<int?> position = new Bindable<int?>(50);

        private TestSceneGameplayLeaderboard.TestGameplayLeaderboardProvider leaderboardProvider = null!;
        private MultiplayerPositionDisplay display = null!;
        private GameplayState gameplayState = null!;

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
            });
            AddSliderStep("set score position", 1, 100, 50, r => position.Value = r);
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
    }
}
