// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Mods
{
    public class IncompatibleIcon : VisibilityContainer, IHasTooltip
    {
        private Circle circle;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Size = new Vector2(20);

            State.Value = Visibility.Hidden;
            Alpha = 0;

            InternalChildren = new Drawable[]
            {
                circle = new Circle
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colours.Gray4,
                },
                new SpriteIcon
                {
                    RelativeSizeAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Size = new Vector2(0.6f),
                    Icon = FontAwesome.Solid.Slash,
                    Colour = Color4.White,
                    Shadow = true,
                }
            };
        }

        protected override void PopIn()
        {
            this.FadeIn(200, Easing.OutQuint);
            circle.FlashColour(Color4.Red, 500, Easing.OutQuint);
            this.ScaleTo(1.8f).Then().ScaleTo(1, 500, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            this.FadeOut(200, Easing.OutQuint);
            this.ScaleTo(0.8f, 200, Easing.In);
        }

        public LocalisableString TooltipText => "Incompatible with current selected mods";
    }
}
