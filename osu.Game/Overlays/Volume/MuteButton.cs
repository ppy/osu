// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Volume
{
    public class MuteButton : Container, IHasCurrentValue<bool>
    {
        private readonly Bindable<bool> current = new Bindable<bool>();

        public Bindable<bool> Current
        {
            get => current;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                current.UnbindBindings();
                current.BindTo(value);
            }
        }

        private Color4 hoveredColour, unhoveredColour;
        private const float width = 100;
        public const float HEIGHT = 35;

        public MuteButton()
        {
            Masking = true;
            BorderThickness = 3;
            CornerRadius = HEIGHT / 2;
            Size = new Vector2(width, HEIGHT);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            hoveredColour = colours.YellowDark;
            BorderColour = unhoveredColour = colours.Gray1.Opacity(0.9f);

            SpriteIcon icon;
            AddRange(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colours.Gray1,
                    Alpha = 0.9f,
                },
                icon = new SpriteIcon
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Size = new Vector2(20),
                }
            });

            Current.ValueChanged += muted =>
            {
                icon.Icon = muted.NewValue ? FontAwesome.Solid.VolumeOff : FontAwesome.Solid.VolumeUp;
                icon.Margin = new MarginPadding { Left = muted.NewValue ? width / 2 - 15 : width / 2 - 10 }; //Magic numbers to line up both icons because they're different widths
            };
            Current.TriggerChange();
        }

        protected override bool OnHover(HoverEvent e)
        {
            this.TransformTo<MuteButton, SRGBColour>("BorderColour", hoveredColour, 500, Easing.OutQuint);
            return false;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            this.TransformTo<MuteButton, SRGBColour>("BorderColour", unhoveredColour, 500, Easing.OutQuint);
        }

        protected override bool OnClick(ClickEvent e)
        {
            Current.Value = !Current.Value;
            return true;
        }
    }
}
