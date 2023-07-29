// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play.HUD;
using osuTK;

namespace osu.Game.Overlays.Mods
{
    internal partial class IncompatibilityDisplayingTooltip : ModButtonTooltip
    {
        private readonly OsuSpriteText incompatibleText;

        private readonly Bindable<IReadOnlyList<Mod>> incompatibleMods = new Bindable<IReadOnlyList<Mod>>();

        [Resolved]
        private Bindable<RulesetInfo> ruleset { get; set; } = null!;

        public IncompatibilityDisplayingTooltip()
        {
            AddRange(new Drawable[]
            {
                incompatibleText = new OsuSpriteText
                {
                    Margin = new MarginPadding { Top = 5 },
                    Font = OsuFont.GetFont(weight: FontWeight.Regular),
                    Text = "Incompatible with:"
                },
                new ModDisplay
                {
                    Current = incompatibleMods,
                    ExpansionMode = ExpansionMode.AlwaysExpanded,
                    Scale = new Vector2(0.7f)
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            incompatibleText.Colour = colours.BlueLight;
        }

        protected override void UpdateDisplay(Mod mod)
        {
            base.UpdateDisplay(mod);

            var incompatibleTypes = mod.IncompatibleMods;

            var allMods = ruleset.Value.CreateInstance().AllMods;

            incompatibleMods.Value = allMods.Where(m => m.GetType() != mod.GetType() && incompatibleTypes.Any(t => t.IsInstanceOfType(m))).Select(m => m.CreateInstance()).ToList();
            incompatibleText.Text = incompatibleMods.Value.Any() ? "Incompatible with:" : "Compatible with all mods";
        }
    }
}
