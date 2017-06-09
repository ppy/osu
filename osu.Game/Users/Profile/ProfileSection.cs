// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Users.Profile
{
    public abstract class ProfileSection : FillFlowContainer
    {
        public abstract string Title { get; }

        protected ProfileSection()
        {
            Margin = new MarginPadding { Horizontal = UserProfile.CONTENT_X_MARGIN };
            Direction = FillDirection.Vertical;
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;
            Children = new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = Title,
                    TextSize = 16,
                    Font = @"Exo2.0-RegularItalic",
                    Margin = new MarginPadding { Vertical = 20 }
                },
                new Box
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 1,
                    Colour = OsuColour.Gray(34),
                    Depth = float.MinValue
                }
            };
        }
    }
}
