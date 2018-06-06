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
using OpenTK.Graphics;

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
                                    new Section("ROOM NAME")
                                    {
                                        Child = new SettingsTextBox(),
                                    },
                                    new Section("ROOM VISIBILITY"),
                                    new Section("GAME TYPE")
                                    {
                                        Child = new GameTypePicker(),
                                    },
                                },
                            },
                            new SectionContainer
                            {
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopRight,
                                Children = new[]
                                {
                                    new Section("MAX PARTICIPANTS")
                                    {
                                        Child = new SettingsTextBox(),
                                    },
                                    new Section("PASSWORD (OPTIONAL)")
                                    {
                                        Child = new SettingsTextBox("Password"),
                                    },
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

        private class SettingsTextBox : OsuTextBox
        {
            private readonly Container labelContainer;

            protected override Color4 BackgroundUnfocused => Color4.Black;
            protected override Color4 BackgroundFocused => Color4.Black;

            protected override Drawable GetDrawableCharacter(char c) => new OsuSpriteText
            {
                Text = c.ToString(),
                TextSize = 18,
            };

            public SettingsTextBox(string label = null)
            {
                RelativeSizeAxes = Axes.X;

                if (label != null)
                {
                    // todo: overflow broken
                    Add(labelContainer = new Container
                    {
                        AutoSizeAxes = Axes.X,
                        RelativeSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = OsuColour.FromHex(@"3d3943"),
                            },
                            new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Font = @"Exo2.0-Bold",
                                Text = label,
                                Margin = new MarginPadding { Horizontal = 10 },
                            },
                        },
                    });
                }
            }

            protected override void UpdateAfterChildren()
            {
                base.UpdateAfterChildren();

                if (labelContainer != null)
                    TextContainer.Padding = new MarginPadding { Horizontal = labelContainer.DrawWidth };
            }
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
                AutoSizeAxes = Axes.Y;
                RelativeSizeAxes = Axes.X;

                InternalChild = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
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
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
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
