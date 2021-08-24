// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play.HUD;
using osuTK;

namespace osu.Game.Overlays.Mods
{
    public class ModButtonTooltip : VisibilityContainer, ITooltip
    {
        private readonly OsuSpriteText descriptionText;
        private readonly Box background;
        private readonly OsuSpriteText incompatibleText;

        private readonly Bindable<IReadOnlyList<Mod>> incompatibleMods = new Bindable<IReadOnlyList<Mod>>();

        [Resolved]
        private Bindable<RulesetInfo> ruleset { get; set; }

        public ModButtonTooltip()
        {
            AutoSizeAxes = Axes.Both;
            Masking = true;
            CornerRadius = 5;

            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding { Left = 10, Right = 10, Top = 5, Bottom = 5 },
                    Children = new Drawable[]
                    {
                        descriptionText = new OsuSpriteText
                        {
                            Font = OsuFont.GetFont(weight: FontWeight.Regular),
                            Margin = new MarginPadding { Bottom = 5 }
                        },
                        incompatibleText = new OsuSpriteText
                        {
                            Font = OsuFont.GetFont(weight: FontWeight.Regular),
                            Text = "Incompatible with:"
                        },
                        new ModDisplay
                        {
                            Current = incompatibleMods,
                            ExpansionMode = ExpansionMode.AlwaysExpanded,
                            Scale = new Vector2(0.7f)
                        }
                    }
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            background.Colour = colours.Gray3;
            descriptionText.Colour = colours.BlueLighter;
            incompatibleText.Colour = colours.BlueLight;
        }

        protected override void PopIn() => this.FadeIn(200, Easing.OutQuint);
        protected override void PopOut() => this.FadeOut(200, Easing.OutQuint);

        private Mod lastMod;

        public bool SetContent(object content)
        {
            if (!(content is Mod mod))
                return false;

            if (mod.Equals(lastMod)) return true;

            lastMod = mod;

            descriptionText.Text = mod.Description;

            var incompatibleTypes = mod.IncompatibleMods;

            var allMods = ruleset.Value.CreateInstance().GetAllMods();

            incompatibleMods.Value = allMods.Where(m => m.GetType() != mod.GetType() && incompatibleTypes.Any(t => t.IsInstanceOfType(m))).ToList();

            if (!incompatibleMods.Value.Any())
            {
                incompatibleText.Text = "Compatible with all mods";
                return true;
            }

            incompatibleText.Text = "Incompatible with:";

            return true;
        }

        public void Move(Vector2 pos) => Position = pos;
    }
}
