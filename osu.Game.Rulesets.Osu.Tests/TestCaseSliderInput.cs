// Copyright (c) 2007-2019 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play;


namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestCaseSliderInput : TestBaseSliders
    {
        private readonly Container content;
        protected override Container<Drawable> Content => content;

        protected override List<Mod> Mods { get; set; }

        protected Player CreatePlayer(Ruleset ruleset)
        {
            Beatmap.Value.Mods.Value = Beatmap.Value.Mods.Value.Concat(new[] { ruleset.GetAutoplayMod() });
            return new ScoreAccessiblePlayer
            {
                AllowPause = false,
                AllowLeadIn = false,
                AllowResults = false,
            };
        }

        public TestCaseSliderInput()
        {
            Mods = new List<Mod>();
            base.Content.Add(content = new OsuInputManager(new RulesetInfo { ID = 0 }));
            CreatePlayer(new OsuRuleset());

            AddStep("Test Slider Tracking", () => createSuperSlowSlider());
        }

        private void createSuperSlowSlider()
        {
            CreateSlider(3f, 400f, 0, 0.5f, 0);
        }
    }

    public class ScoreAccessiblePlayer : Player
    {
    }
}
