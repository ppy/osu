// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Play.Options
{
    public abstract class OptionContainer : Container
    {
        /// <summary>
        /// The title of this option.
        /// </summary>
        public abstract string Title { get; }

        public OptionContainer()
        {
            Masking = true;
            Size = new Vector2(200, 100);
            CornerRadius = 5;
            BorderColour = Color4.Black;
            BorderThickness = 2;
            Depth = 10;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.5f,
                },
                new OsuSpriteText
                {
                    Origin = Anchor.TopLeft,
                    Anchor = Anchor.TopLeft,
                    Text = Title,
                    TextSize = 17,
                    Margin = new MarginPadding { Top = 5, Left = 10 },
                    Font = @"Exo2.0-Bold",
                },
                new SimpleButton
                {
                    Origin = Anchor.TopRight,
                    Anchor = Anchor.TopRight,
                    Margin = new MarginPadding { Top = 5, Right = 10 },
                    Icon = FontAwesome.fa_bars,
                    Scale = new Vector2(0.7f),
                },
            };
        }
    }
}
