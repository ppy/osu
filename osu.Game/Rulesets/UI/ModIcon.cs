// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osuTK;
using osu.Framework.Bindables;

namespace osu.Game.Rulesets.UI
{
    public class ModIcon : Container, IHasTooltip
    {
        public readonly BindableBool Selected = new BindableBool();

        private readonly SpriteIcon modIcon;
        private readonly SpriteText modAcronym;
        private readonly SpriteIcon background;

        private const float size = 80;

        private readonly ModType type;

        public virtual string TooltipText => mod.IconTooltip;

        private Mod mod;

        public Mod Mod
        {
            get => mod;
            set
            {
                mod = value;
                updateMod(value);
            }
        }

        public ModIcon(Mod mod)
        {
            this.mod = mod ?? throw new ArgumentNullException(nameof(mod));

            type = mod.Type;

            Size = new Vector2(size);

            Children = new Drawable[]
            {
                background = new SpriteIcon
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Size = new Vector2(size),
                    Icon = OsuIcon.ModBg,
                    Shadow = true,
                },
                modAcronym = new OsuSpriteText
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Colour = OsuColour.Gray(84),
                    Alpha = 0,
                    Font = OsuFont.Numeric.With(null, 22f),
                    UseFullGlyphHeight = false,
                    Text = mod.Acronym
                },
                modIcon = new SpriteIcon
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Colour = OsuColour.Gray(84),
                    Size = new Vector2(45),
                    Icon = FontAwesome.Solid.Question
                },
            };

            updateMod(mod);
        }

        private void updateMod(Mod value)
        {
            modAcronym.Text = value.Acronym;
            modIcon.Icon = value.Icon ?? FontAwesome.Solid.Question;

            if (value.Icon is null)
            {
                modIcon.FadeOut();
                modAcronym.FadeIn();
                return;
            }

            modIcon.FadeIn();
            modAcronym.FadeOut();
        }

        private Color4 backgroundColour;
        private Color4 highlightedColour;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            switch (type)
            {
                default:
                case ModType.DifficultyIncrease:
                    backgroundColour = colours.Yellow;
                    highlightedColour = colours.YellowLight;
                    break;

                case ModType.DifficultyReduction:
                    backgroundColour = colours.Green;
                    highlightedColour = colours.GreenLight;
                    break;

                case ModType.Automation:
                    backgroundColour = colours.Blue;
                    highlightedColour = colours.BlueLight;
                    break;

                case ModType.Conversion:
                    backgroundColour = colours.Purple;
                    highlightedColour = colours.PurpleLight;
                    break;

                case ModType.Fun:
                    backgroundColour = colours.Pink;
                    highlightedColour = colours.PinkLight;
                    break;

                case ModType.System:
                    backgroundColour = colours.Gray6;
                    highlightedColour = colours.Gray7;
                    modIcon.Colour = colours.Yellow;
                    break;
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Selected.BindValueChanged(selected => background.Colour = selected.NewValue ? highlightedColour : backgroundColour, true);
        }
    }
}
