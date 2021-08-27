// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osuTK;

namespace osu.Game.Overlays.Mods
{
    public class ModButtonTooltip : VisibilityContainer, ITooltip
    {
        private readonly OsuSpriteText descriptionText;
        private readonly Box background;

        protected override Container<Drawable> Content { get; }

        public ModButtonTooltip()
        {
            AutoSizeAxes = Axes.Both;
            Masking = true;
            CornerRadius = 5;

            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both
                },
                Content = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding { Left = 10, Right = 10, Top = 5, Bottom = 5 },
                    Children = new Drawable[]
                    {
                        descriptionText = new OsuSpriteText
                        {
                            Font = OsuFont.GetFont(weight: FontWeight.Regular),
                        },
                    }
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            background.Colour = colours.Gray3;
            descriptionText.Colour = colours.BlueLighter;
        }

        protected override void PopIn() => this.FadeIn(200, Easing.OutQuint);
        protected override void PopOut() => this.FadeOut(200, Easing.OutQuint);

        private Mod lastMod;

        protected virtual Type TargetContentType => typeof(ModButton);

        public virtual bool SetContent(object content)
        {
            if (!(content is ModButton button) || content.GetType() != TargetContentType)
                return false;

            var mod = button.SelectedMod ?? button.Mods.First();

            if (mod.Equals(lastMod)) return true;

            lastMod = mod;

            UpdateDisplay(mod);
            return true;
        }

        protected virtual void UpdateDisplay(Mod mod)
        {
            descriptionText.Text = mod.Description;
        }

        public void Move(Vector2 pos) => Position = pos;
    }
}
