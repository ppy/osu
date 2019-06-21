// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;
using osuTK.Graphics;

namespace osu.Game.Overlays.Profile
{
    public abstract class ProfileSection : Container
    {
        public abstract string Title { get; }

        public abstract string Identifier { get; }

        private readonly FillFlowContainer content;
        private readonly Box background;

        protected override Container<Drawable> Content => content;

        public readonly Bindable<User> User = new Bindable<User>();

        protected ProfileSection()
        {
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;

            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                new SectionTriangles
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                },
                new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Children = new Drawable[]
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
                    },
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            background.Colour = colours.GreySeafoamDarker;
        }

        private class SectionTriangles : Container
        {
            private readonly Triangles triangles;
            private readonly Box foreground;

            public SectionTriangles()
            {
                RelativeSizeAxes = Axes.X;
                Height = 100;
                Masking = true;
                Children = new Drawable[]
                {
                    triangles = new Triangles
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        RelativeSizeAxes = Axes.Both,
                        Height = 0.95f,
                        TriangleScale = 4,
                        Velocity = 0.5f,
                    },
                    foreground = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                triangles.ColourLight = colours.GreySeafoamDark;
                triangles.ColourDark = colours.GreySeafoamDarker;
                foreground.Colour = ColourInfo.GradientVertical(colours.GreySeafoamDarker, new Color4(0,0,0,0));
            }
        }
    }
}
