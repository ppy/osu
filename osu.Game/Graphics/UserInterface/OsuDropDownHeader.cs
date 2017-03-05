// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Sprites;
using OpenTK.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuDropDownHeader : DropDownHeader
    {
        private SpriteText label;
        protected override string Label
        {
            get { return label.Text; }
            set { label.Text = value; }
        }

        public OsuDropDownHeader()
        {
            Foreground.Padding = new MarginPadding(4);

            AutoSizeAxes = Axes.None;
            Margin = new MarginPadding { Bottom = 4 };
            CornerRadius = 4;
            Height = 40;

            Children = new[]
            {
                label = new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                },
                new TextAwesome
                {
                    Icon = FontAwesome.fa_chevron_down,
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Margin = new MarginPadding { Right = 4 },
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            BackgroundColour = Color4.Black.Opacity(0.5f);
            BackgroundColourHover = colours.PinkDarker;
        }
    }
}