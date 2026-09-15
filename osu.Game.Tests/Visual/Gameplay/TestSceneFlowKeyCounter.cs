// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Screens.Play.HUD;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneFlowKeyCounter : OsuManualInputManagerTestScene
    {
        public TestSceneFlowKeyCounter()
        {
            FlowKeyCounter counter;
            var trigger = new KeyCounterKeyboardTrigger(Key.X);

            Children = new Drawable[]
            {
                trigger,
                counter = new FlowKeyCounter(trigger)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            };

            AddSliderStep("flow duration", 250f, 2000f, 500f, duration => counter.FlowDuration.Value = duration);
            AddToggleStep("hide trigger name", hidden => counter.ShowTriggerName.Value = !hidden);
            AddStep("color to red", () => counter.AccentColour.Value = Color4.Red);
            AddStep("color to white", () => counter.AccentColour.Value = Color4.White);

            AddStep("press X", () => InputManager.PressKey(Key.X));
            AddStep("release X", () => InputManager.ReleaseKey(Key.X));
        }
    }
}
