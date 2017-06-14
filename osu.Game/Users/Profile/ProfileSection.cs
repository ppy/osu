// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
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

        private readonly FillFlowContainer content;
        protected override Container<Drawable> Content => content;

        protected ProfileSection()
        {
            Direction = FillDirection.Vertical;
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;
            InternalChildren = new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = Title,
                    TextSize = 20,
                    Font = @"Exo2.0-RegularItalic",
                    Margin = new MarginPadding
                    {
                        Horizontal = UserProfile.CONTENT_X_MARGIN,
                        Vertical = 20
                    }
                },
                content = new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Margin = new MarginPadding
                    {
                        Horizontal = UserProfile.CONTENT_X_MARGIN,
                        Bottom = 20
                    }
                },
                new Box
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 1,
                    Colour = OsuColour.Gray(34),
                    EdgeSmoothness = new Vector2(1)
                }
            };
        }
    }
}
