// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play.HUD;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Overlays.Mods
{
    public class IncompatibilityDisplayingModButton : ModButton
    {
        private readonly CompositeDrawable incompatibleIcon;

        [Resolved]
        private Bindable<IReadOnlyList<Mod>> selectedMods { get; set; }

        public IncompatibilityDisplayingModButton(Mod mod)
            : base(mod)
        {
            ButtonContent.Add(incompatibleIcon = new IncompatibleIcon
            {
                Anchor = Anchor.BottomRight,
                Origin = Anchor.Centre,
                Position = new Vector2(-13),
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            selectedMods.BindValueChanged(_ => Scheduler.AddOnce(updateCompatibility), true);
        }

        protected override void DisplayMod(Mod mod)
        {
            base.DisplayMod(mod);

            Scheduler.AddOnce(updateCompatibility);
        }

        private void updateCompatibility()
        {
            var m = SelectedMod ?? Mods.First();

            bool isIncompatible = false;

            if (selectedMods.Value.Count > 0 && !selectedMods.Value.Contains(m))
                isIncompatible = !ModUtils.CheckCompatibleSet(selectedMods.Value.Append(m));

            if (isIncompatible)
                incompatibleIcon.Show();
            else
                incompatibleIcon.Hide();
        }

        public override ITooltip<Mod> GetCustomTooltip() => new IncompatibilityDisplayingTooltip();

        private class IncompatibilityDisplayingTooltip : ModButtonTooltip
        {
            private readonly OsuSpriteText incompatibleText;

            private readonly Bindable<IReadOnlyList<Mod>> incompatibleMods = new Bindable<IReadOnlyList<Mod>>();

            [Resolved]
            private Bindable<RulesetInfo> ruleset { get; set; }

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

                var allMods = ruleset.Value.CreateInstance().GetAllMods();

                incompatibleMods.Value = allMods.Where(m => m.GetType() != mod.GetType() && incompatibleTypes.Any(t => t.IsInstanceOfType(m))).ToList();
                incompatibleText.Text = incompatibleMods.Value.Any() ? "Incompatible with:" : "Compatible with all mods";
            }
        }
    }
}
