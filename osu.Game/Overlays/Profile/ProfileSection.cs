// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Profile
{
    public abstract class ProfileSection : FillFlowContainer
    {
        private readonly Box underscore;
        private readonly Box sectionDivider;
        private const int underscore_height = 2;

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
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(4),
                    Margin = new MarginPadding
                    {
                        Horizontal = UserProfileOverlay.CONTENT_X_MARGIN,
                        Vertical = 10
                    },
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = Title,
                            Font = OsuFont.GetFont(size: 20, weight: FontWeight.Bold),
                        },
                        new Container
                        {
                            Height = underscore_height,
                            RelativeSizeAxes = Axes.X,
                            Child = underscore = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                            }
                        }
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
                sectionDivider = new Box
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 4,
                    Colour = Color4.Black,
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

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            underscore.Colour = colours.Seafoam;
        }
    }
}
