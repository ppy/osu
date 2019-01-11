// Copyright (c) 2007-2019 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;


namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestCaseSliderInput : TestBaseSliders
    {
        private readonly Container content;
        protected override Container<Drawable> Content => content;

        public TestCaseSliderInput()
        {
            base.Content.Add(content = new OsuInputManager(new RulesetInfo { ID = 0 }));

            AddStep("Test Slider Tracking", () => createSuperSlowSlider());
        }

        private void createSuperSlowSlider()
        {
            CreateSlider(3f, 400f, 0, 0.5, 0);
        }
    }
}
