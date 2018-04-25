// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Screens.Ranking
{
    public class ResultModeButton : TabItem<ResultMode>
    {
        private readonly FontAwesome icon;
        private Color4 activeColour;
        private Color4 inactiveColour;
        private CircularContainer colouredPart;

        public ResultModeButton(ResultMode mode) : base(mode)
        {
            switch (mode)
            {
                case ResultMode.Summary:
                    icon = FontAwesome.fa_asterisk;
                    break;
                case ResultMode.Ranking:
                    icon = FontAwesome.fa_list;
                    break;
                case ResultMode.Share:
                    icon = FontAwesome.fa_camera;
                    break;
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Size = new Vector2(50);

            Masking = true;
            CornerRadius = 25;

            activeColour = colours.PinkDarker;
            inactiveColour = OsuColour.Gray(0.8f);

            EdgeEffect = new EdgeEffectParameters
            {
                Colour = Color4.Black.Opacity(0.4f),
                Type = EdgeEffectType.Shadow,
                Radius = 5,
            };

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.White,
                },
                colouredPart = new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.8f),
                    BorderThickness = 4,
                    BorderColour = Color4.White,
                    Colour = inactiveColour,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            AlwaysPresent = true, //for border rendering
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Transparent,
                        },
                        new SpriteIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Shadow = false,
                            Colour = OsuColour.Gray(0.95f),
                            Icon = icon,
                            Size = new Vector2(20),
                        }
                    }
                }
            };
        }

        protected override void OnActivated() => colouredPart.FadeColour(activeColour, 200, Easing.OutQuint);

        protected override void OnDeactivated() => colouredPart.FadeColour(inactiveColour, 200, Easing.OutQuint);
    }
}
