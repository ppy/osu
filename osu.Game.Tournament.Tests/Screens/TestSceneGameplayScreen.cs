// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Screens.Gameplay;
using osu.Game.Tournament.Screens.Gameplay.Components;

namespace osu.Game.Tournament.Tests.Screens
{
    public class TestSceneGameplayScreen : TournamentTestScene
    {
        [Cached]
        private TournamentMatchChatDisplay chat = new TournamentMatchChatDisplay { Width = 0.5f };

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(new GameplayScreen());
            Add(chat);
        }

        [Test]
        public void TestWarmup()
        {
            checkScoreVisibility(false);

            toggleWarmup();
            checkScoreVisibility(true);

            toggleWarmup();
            checkScoreVisibility(false);
        }

        private void checkScoreVisibility(bool visible)
            => AddUntilStep($"scores {(visible ? "shown" : "hidden")}",
                () => this.ChildrenOfType<TeamScore>().All(score => score.Alpha == (visible ? 1 : 0)));

        private void toggleWarmup()
            => AddStep("toggle warmup", () => this.ChildrenOfType<TourneyButton>().First().Click());
    }
}
