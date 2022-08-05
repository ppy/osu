// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD.KPSCounter;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneKeyPerSecondCounter : PlayerTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new ManiaRuleset();
        protected override bool HasCustomSteps => false;
        protected override bool Autoplay => true;

        private GameplayClock gameplayClock;
        private DrawableRuleset drawableRuleset;

        // private DependencyProvidingContainer dependencyContainer;
        private KeysPerSecondCounter counter;

        [SetUpSteps]
        public new void SetUpSteps()
        {
            /*
            CreateTest(() => AddStep("Create components", () =>
            {
                Logger.Log($"{(Player != null ? Player.ToString() : "null")}", level: LogLevel.Debug);
                dependencyContainer = new DependencyProvidingContainer
                {
                    RelativePositionAxes = Axes.Both,
                };
            }));
        */
        }

        private void createCounter()
        {
            AddStep("Create counter", () =>
            {
                /*
                if (!Contains(dependencyContainer))
                {
                    Add(dependencyContainer);
                }

                if (dependencyContainer.CachedDependencies.Length == 0)
                {
                    dependencyContainer.CachedDependencies = new (Type, object)[]
                    {
                        (typeof(GameplayClock), ,
                        (typeof(DrawableRuleset),)
                    };
                }
                Dependencies.Cache(gameplayClock = Player.GameplayClockContainer.GameplayClock));
                */

                Dependencies.Cache(gameplayClock = Player.GameplayClockContainer.GameplayClock);
                Dependencies.Cache(drawableRuleset = Player.DrawableRuleset);

                Add(counter = new KeysPerSecondCounter
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Scale = new Vector2(5),
                        Position = new Vector2(10, 100)
                    }
                );
            });
            AddAssert("ensure counter added", () => Contains(counter));
        }

        [Test]
        public void TestInGameTimeConsistency()
        {
            createCounter();

            AddUntilStep("Wait until first note", () => counter.Current.Value != 0);
            AddStep("Pause gameplay", () => gameplayClock.IsPaused.Value = true);
            AddAssert("KPS = 1", () => counter.Current.Value == 1);
        }
    }
}
