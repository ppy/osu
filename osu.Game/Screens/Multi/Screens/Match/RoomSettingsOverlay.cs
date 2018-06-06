// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using OpenTK;

namespace osu.Game.Screens.Multi.Screens.Match
{
    public class RoomSettingsOverlay : OsuFocusedOverlayContainer
    {
        private const float transition_duration = 500;

        private readonly Container content;

        protected override Container<Drawable> Content => content;

        public RoomSettingsOverlay()
        {
            InternalChild = content = new Container
            {
                RelativeSizeAxes = Axes.Both,
                RelativePositionAxes = Axes.Y,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = OsuColour.FromHex(@"28242d"),
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Top = 35, Bottom = 75, Horizontal = 50 },
                        Children = new[]
                        {
                            new SectionContainer
                            {
                                Children = new[]
                                {
                                    new Section("ROOM NAME"),
                                    new Section("ROOM VISIBILITY"),
                                    new Section("GAME TYPE"),
                                },
                            },
                            new SectionContainer
                            {
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopRight,
                                Children = new[]
                                {
                                    new Section("MAX PARTICIPANTS"),
                                    new Section("PASSWORD (OPTIONAL)"),
                                },
                            },
                        },
                    },
                    new ApplyButton
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Size = new Vector2(230, 35),
                        Margin = new MarginPadding { Bottom = 20 },
                        Action = Hide,
                    },
                },
            };
        }

        protected override void PopIn()
        {
            base.PopIn();

            Content.MoveToY(0, transition_duration, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            base.PopOut();

            Content.MoveToY(-1, transition_duration, Easing.InSine);
        }

        private class SectionContainer : FillFlowContainer<Section>
        {
            public SectionContainer()
            {
                RelativeSizeAxes = Axes.Both;
                Width = 0.45f;
                Direction = FillDirection.Vertical;
                Spacing = new Vector2(45);
            }
        }

        private class Section : Container
        {
            private readonly Container content;

            protected override Container<Drawable> Content => content;

            public Section(string title)
            {
                AutoSizeAxes = Axes.Both;

                InternalChild = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(5),
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            TextSize = 12,
                            Font = @"Exo2.0-Bold",
                            Text = title.ToUpper(),
                        },
                        content = new Container
                        {
                            AutoSizeAxes = Axes.Both,
                        },
                    },
                };
            }
        }

        private class ApplyButton : OsuButton
        {
            protected override SpriteText CreateText() => new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };

            public ApplyButton()
            {
                Masking = true;
                CornerRadius = 5;
                Text = "Apply";
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                BackgroundColour = colours.Yellow;
            }
        }
    }
}
