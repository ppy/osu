// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using AutoMapper.Internal;
using NUnit.Framework;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD.KPSCounter;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneKeysPerSecondCounter : PlayerTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new ManiaRuleset();
        protected override bool HasCustomSteps => false;
        protected override bool Autoplay => false;
        protected override TestPlayer CreatePlayer(Ruleset ruleset) => new TestPlayer(true, false);

        private GameplayClock gameplayClock;
        private DrawableRuleset drawableRuleset;

        private KeysPerSecondCounter counter;

        private void createCounter()
        {
            AddStep("Create counter", () =>
            {
                gameplayClock = Player.GameplayClockContainer.GameplayClock;
                drawableRuleset = Player.DrawableRuleset;

                Player.HUDOverlay.Add(counter = new KeysPerSecondCounter
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(5),
                });
                counter.SmoothingTime.Value = 0;
            });
            AddUntilStep("Counter created", () => Player.HUDOverlay.Contains(counter));
        }

        [Test]
        public void TestBasic()
        {
            createCounter();

            AddStep("press 1 key", () => InputManager.Key(Key.D));
            AddAssert("KPS = 1", () => counter.Current.Value == 1);
            AddUntilStep("Wait for KPS cooldown", () => counter.Current.Value <= 0);
            AddStep("press 4 keys", () =>
            {
                InputManager.Key(Key.D);
                InputManager.Key(Key.F);
                InputManager.Key(Key.J);
                InputManager.Key(Key.K);
            });
            AddAssert("KPS = 4", () => counter.Current.Value == 4);
            AddStep("Pause player", () => Player.Pause());
            AddAssert("KPS = 4", () => counter.Current.Value == 4);
            AddStep("Resume player", () => Player.Resume());
            AddStep("press 4 keys", () =>
            {
                InputManager.Key(Key.D);
                InputManager.Key(Key.F);
                InputManager.Key(Key.J);
                InputManager.Key(Key.K);
            });
            AddAssert("KPS = 8", () => counter.Current.Value == 8);
            AddUntilStep("Wait for KPS cooldown", () => counter.Current.Value <= 0);
            AddStep("Add DT", () =>
            {
                var dt = new ManiaModDoubleTime
                {
                    SpeedChange =
                    {
                        Value = 2
                    }
                };
                Player.Mods.Value.Concat((dt.Yield()).ToArray());
            });
        }
    }
}
