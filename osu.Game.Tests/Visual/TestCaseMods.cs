// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets;
using osu.Game.Screens.Play.HUD;
using OpenTK;

namespace osu.Game.Tests.Visual
{
    [Description("mod select and icon display")]
    internal class TestCaseMods : OsuTestCase
    {
        private ModSelectOverlay modSelect;
        private ModDisplay modDisplay;

        private RulesetStore rulesets;


        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            this.rulesets = rulesets;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Add(modSelect = new ModSelectOverlay
            {
                RelativeSizeAxes = Axes.X,
                Origin = Anchor.BottomCentre,
                Anchor = Anchor.BottomCentre,
            });

            Add(modDisplay = new ModDisplay
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                AutoSizeAxes = Axes.Both,
                Position = new Vector2(0, 25),
            });

            modDisplay.Current.BindTo(modSelect.SelectedMods);

            AddStep("Toggle", modSelect.ToggleVisibility);

            foreach (var ruleset in rulesets.AvailableRulesets)
                AddStep(ruleset.CreateInstance().Description, () => modSelect.Ruleset.Value = ruleset);
        }
    }
}
