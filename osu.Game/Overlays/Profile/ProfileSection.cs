// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osuTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;
using osuTK.Graphics;

namespace osu.Game.Overlays.Profile
{
    public abstract class ProfileSection : FillFlowContainer
    {
        public abstract string Title { get; }

        public abstract string Identifier { get; }

        private readonly FillFlowContainer content;

        protected override Container<Drawable> Content => content;

        public readonly Bindable<User> User = new Bindable<User>();

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
                    Font = OsuFont.GetFont(size: 20, weight: FontWeight.Regular, italics: true),
                    Margin = new MarginPadding
                    {
                        Horizontal = UserProfileOverlay.CONTENT_X_MARGIN,
                        Vertical = 10
                    }
                },
                content = new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Padding = new MarginPadding
                    {
                        Horizontal = UserProfileOverlay.CONTENT_X_MARGIN,
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

            // placeholder
            Add(new OsuSpriteText
            {
                Text = @"coming soon!",
                Colour = Color4.Gray,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Margin = new MarginPadding { Top = 100, Bottom = 100 }
            });
        }
    }
}
