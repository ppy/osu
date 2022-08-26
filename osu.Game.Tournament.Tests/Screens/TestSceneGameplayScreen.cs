// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Screens.Gameplay;
using osu.Game.Tournament.Screens.Gameplay.Components;

namespace osu.Game.Tournament.Tests.Screens
{
    public class TestSceneGameplayScreen : TournamentTestScene
    {
        [Cached]
        private TournamentMatchChatDisplay chat = new TournamentMatchChatDisplay { Width = 0.5f };

        [Test]
        public void TestStartupState([Values] TourneyState state)
        {
            AddStep("set state", () => IPCInfo.State.Value = state);
            createScreen();
        }

        [Test]
        public void TestStartupStateNoCurrentMatch([Values] TourneyState state)
        {
            AddStep("set null current", () => Ladder.CurrentMatch.Value = null);
            AddStep("set state", () => IPCInfo.State.Value = state);
            createScreen();
        }

        [Test]
        public void TestWarmup()
        {
            createScreen();

            checkScoreVisibility(false);

            toggleWarmup();
            checkScoreVisibility(true);

            toggleWarmup();
            checkScoreVisibility(false);
        }

        private void createScreen()
        {
            AddStep("setup screen", () =>
            {
                Remove(chat);

                Children = new Drawable[]
                {
                    new GameplayScreen(),
                    chat,
                };
            });
        }

        private void checkScoreVisibility(bool visible)
            => AddUntilStep($"scores {(visible ? "shown" : "hidden")}",
                () => this.ChildrenOfType<TeamScore>().All(score => score.Alpha == (visible ? 1 : 0)));

        private void toggleWarmup()
            => AddStep("toggle warmup", () => this.ChildrenOfType<TourneyButton>().First().TriggerClick());
    }
}
