// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Game.Graphics;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.Volume
{
    public class MuteButton : Container, IHasCurrentValue<bool>
    {
        public Bindable<bool> Current { get; } = new Bindable<bool>();

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

            Current.ValueChanged += newValue =>
            {
                icon.Icon = newValue ? FontAwesome.fa_volume_off : FontAwesome.fa_volume_up;
                icon.Margin = new MarginPadding { Left = newValue ? width / 2 - 15 : width / 2 - 10 }; //Magic numbers to line up both icons because they're different widths
            };
            Current.TriggerChange();
        }

        protected override bool OnHover(InputState state)
        {
            this.TransformTo<MuteButton, SRGBColour>("BorderColour", hoveredColour, 500, Easing.OutQuint);
            return true;
        }

        protected override void OnHoverLost(InputState state)
        {
            this.TransformTo<MuteButton, SRGBColour>("BorderColour", unhoveredColour, 500, Easing.OutQuint);
        }

        protected override bool OnClick(InputState state)
        {
            Current.Value = !Current.Value;
            return true;
        }
    }
}
