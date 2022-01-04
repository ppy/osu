// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.Profile
{
    public abstract class ProfileSection : Container
    {
        public abstract LocalisableString Title { get; }

        public abstract string Identifier { get; }

        private readonly FillFlowContainer<Drawable> content;
        private readonly Box background;
        private readonly Box underscore;

        protected override Container<Drawable> Content => content;

        public readonly Bindable<APIUser> User = new Bindable<APIUser>();

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
                        new Container
                        {
                            AutoSizeAxes = Axes.Both,
                            Margin = new MarginPadding
                            {
                                Horizontal = UserProfileOverlay.CONTENT_X_MARGIN,
                                Top = 15,
                                Bottom = 20,
                            },
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Text = Title,
                                    Font = OsuFont.GetFont(size: 20, weight: FontWeight.Bold),
                                },
                                underscore = new Box
                                {
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.TopCentre,
                                    Margin = new MarginPadding { Top = 4 },
                                    RelativeSizeAxes = Axes.X,
                                    Height = 2,
                                }
                            }
                        },
                        // reverse ID flow is required for correct Z-ordering of the content (last item should be front-most).
                        // particularly important in BeatmapsSection, as it uses beatmap cards, which have expandable overhanging content.
                        content = new ReverseChildIDFillFlowContainer<Drawable>
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
        private void load(OverlayColourProvider colourProvider)
        {
            background.Colour = colourProvider.Background5;
            underscore.Colour = colourProvider.Highlight1;
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
                        TriangleScale = 3,
                    },
                    foreground = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                triangles.ColourLight = colourProvider.Background4;
                triangles.ColourDark = colourProvider.Background5.Darken(0.2f);
                foreground.Colour = ColourInfo.GradientVertical(colourProvider.Background5, colourProvider.Background5.Opacity(0));
            }
        }
    }
}
