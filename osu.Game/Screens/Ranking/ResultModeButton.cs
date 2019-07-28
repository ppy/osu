// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Screens.Ranking
{
    public class ResultModeButton : TabItem<IResultPageInfo>, IHasTooltip
    {
        private readonly IconUsage icon;
        private Color4 activeColour;
        private Color4 inactiveColour;
        private CircularContainer colouredPart;

        public ResultModeButton(IResultPageInfo mode)
            : base(mode)
        {
            icon = mode.Icon;
            TooltipText = mode.Name;
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

        public string TooltipText { get; private set; }
    }
}
