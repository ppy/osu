// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens;
using osu.Game.Screens.Play;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestCasePlayerLoader : ManualInputManagerTestCase
    {
        private PlayerLoader loader;
        private OsuScreenStack stack;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            InputManager.Child = stack = new OsuScreenStack { RelativeSizeAxes = Axes.Both };
            Beatmap.Value = new TestWorkingBeatmap(new TestBeatmap(new OsuRuleset().RulesetInfo), Clock);
        });

        [Test]
        public void TestLoadContinuation()
        {
            AddStep("load dummy beatmap", () => stack.Push(loader = new PlayerLoader(() => new Player(false, false))));
            AddUntilStep("wait for current", () => loader.IsCurrentScreen());
            AddStep("mouse in centre", () => InputManager.MoveMouseTo(loader.ScreenSpaceDrawQuad.Centre));
            AddUntilStep("wait for no longer current", () => !loader.IsCurrentScreen());
            AddStep("load slow dummy beatmap", () =>
            {
                SlowLoadPlayer slow = null;

                stack.Push(loader = new PlayerLoader(() => slow = new SlowLoadPlayer(false, false)));

                Scheduler.AddDelayed(() => slow.Ready = true, 5000);
            });

            AddUntilStep("wait for no longer current", () => !loader.IsCurrentScreen());
        }

        [Test]
        public void TestModReinstantiation()
        {
            TestPlayer player = null;
            TestMod gameMod = null;
            TestMod playerMod1 = null;
            TestMod playerMod2 = null;

            AddStep("load player", () =>
            {
                Mods.Value = new[] { gameMod = new TestMod() };
                stack.Push(loader = new PlayerLoader(() => player = new TestPlayer()));
            });

            AddUntilStep("wait for loader to become current", () => loader.IsCurrentScreen());
            AddStep("mouse in centre", () => InputManager.MoveMouseTo(loader.ScreenSpaceDrawQuad.Centre));
            AddUntilStep("wait for player to be current", () => player.IsCurrentScreen());
            AddStep("retrieve mods", () => playerMod1 = (TestMod)player.Mods.Value.Single());
            AddAssert("game mods not applied", () => gameMod.Applied == false);
            AddAssert("player mods applied", () => playerMod1.Applied);

            AddStep("restart player", () =>
            {
                var lastPlayer = player;
                player = null;
                lastPlayer.Restart();
            });

            AddUntilStep("wait for player to be current", () => player.IsCurrentScreen());
            AddStep("retrieve mods", () => playerMod2 = (TestMod)player.Mods.Value.Single());
            AddAssert("game mods not applied", () => gameMod.Applied == false);
            AddAssert("player has different mods", () => playerMod1 != playerMod2);
            AddAssert("player mods applied", () => playerMod2.Applied);
        }

        private class TestMod : Mod, IApplicableToScoreProcessor
        {
            public override string Name => string.Empty;
            public override string Acronym => string.Empty;
            public override double ScoreMultiplier => 1;

            public bool Applied { get; private set; }

            public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
            {
                Applied = true;
            }

            public ScoreRank AdjustRank(ScoreRank rank, double accuracy) => rank;
        }

        private class TestPlayer : Player
        {
            public new Bindable<IReadOnlyList<Mod>> Mods => base.Mods;

            public TestPlayer()
                : base(false, false)
            {
            }
        }

        protected class SlowLoadPlayer : Player
        {
            public bool Ready;

            public SlowLoadPlayer(bool allowPause = true, bool showResults = true)
                : base(allowPause, showResults)
            {
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                while (!Ready)
                    Thread.Sleep(1);
            }
        }
    }
}
