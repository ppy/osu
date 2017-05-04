// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Testing;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Sprites;
using osu.Game.Screens.Menu;
using OpenTK.Graphics;
using osu.Game.Screens.Play;
using osu.Framework.Allocation;
using osu.Game.Overlays.Mods;
using osu.Game.Database;
using osu.Game.Rulesets.Mods;
using OpenTK;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseIngameModsContainer : TestCase
    {
        public override string Description => @"Ingame mods visualization";

        private ModSelectOverlay modSelect;
        private ModsContainer modsContainer;
        private RulesetDatabase rulesets;

        [BackgroundDependencyLoader]
        private void load(RulesetDatabase rulesets)
        {
            this.rulesets = rulesets;
        }

        public override void Reset()
        {
            base.Reset();

            Add(modSelect = new ModSelectOverlay
            {
                RelativeSizeAxes = Axes.X,
                Origin = Anchor.BottomCentre,
                Anchor = Anchor.BottomCentre,
            });

            Add(modsContainer = new ModsContainer
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                AutoSizeAxes = Axes.Both,
                Position = new Vector2(0, 25),
            });

            modSelect.SelectedMods.ValueChanged += SelectedMods_ValueChanged;
            modSelect.SelectedMods.TriggerChange();

            AddStep("ToggleModSelect", modSelect.ToggleVisibility);
            foreach (var ruleset in rulesets.AllRulesets)
                AddStep(ruleset.CreateInstance().Description, () => modSelect.Ruleset.Value = ruleset);
        }

        private void SelectedMods_ValueChanged(System.Collections.Generic.IEnumerable<Mod> newValue)
        {
            modsContainer.Clear();
            foreach (Mod mod in modSelect.SelectedMods.Value)
                modsContainer.Add(mod);
        }
    }
}
