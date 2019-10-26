// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osuTK.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Allocation;
using osu.Game.Graphics.UserInterface;
using System.Collections.Generic;
using osuTK;
using osu.Framework.Bindables;

namespace osu.Game.Overlays.Comments
{
    public class ResponseContainer : Container
    {
        private const int height = 60;
        private const int corner_radius = 5;

        public readonly BindableBool Expanded = new BindableBool();

        public ResponseContainer()
        {
            Height = height;
            RelativeSizeAxes = Axes.X;
            Masking = true;
            CornerRadius = corner_radius;
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.Gray(0.2f)
                },
                new ResponseTextBox
                {
                    RelativeSizeAxes = Axes.X,
                    Height = height / 2f,
                    PlaceholderText = @"Type your response here",
                },
                new Container
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.X,
                    Height = height / 2f,
                    Padding = new MarginPadding { Horizontal = 10 },
                    Children = new Drawable[]
                    {
                        new SpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Text = @"Press enter to post.",
                            Font = OsuFont.GetFont(size: 14),
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Direction = FillDirection.Horizontal,
                            AutoSizeAxes = Axes.Both,
                            Spacing = new Vector2(3, 0),
                            Children = new[]
                            {
                                new CancelButton
                                {
                                    Expanded = { BindTarget = Expanded }
                                },
                                new Button(@"Reply"),
                            }
                        }
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    BorderThickness = 3,
                    Masking = true,
                    CornerRadius = corner_radius,
                    BorderColour = OsuColour.Gray(0.2f),
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Transparent
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Expanded.BindValueChanged(expanded =>
            {
                if (expanded.NewValue)
                    Show();
                else
                    Hide();
            }, true);
        }

        private class ResponseTextBox : TextBox
        {
            protected override float LeftRightPadding => 10;

            protected override SpriteText CreatePlaceholder() => new SpriteText
            {
                Font = OsuFont.GetFont(),
                Colour = OsuColour.Gray(0.2f),
            };

            public ResponseTextBox()
            {
                TextContainer.Height = 0.5f;
                LengthLimit = 1000;
                CornerRadius = 5;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                BackgroundUnfocused = BackgroundFocused = colours.Gray2;
            }

            protected override Drawable GetDrawableCharacter(char c) => new SpriteText { Text = c.ToString(), Font = OsuFont.GetFont(size: CalculatedTextSize) };
        }

        private class Button : LoadingButton
        {
            protected override IEnumerable<Drawable> EffectTargets => new[] { background };

            private Box background;
            private SpriteText text;

            public Button(string buttonText)
            {
                text.Text = buttonText;

                AutoSizeAxes = Axes.Both;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                IdleColour = colours.BlueDark;
                HoverColour = colours.Blue;
            }

            protected override Drawable CreateContent() => new CircularContainer
            {
                Masking = true,
                Height = 20,
                AutoSizeAxes = Axes.X,
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    text = new SpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Margin = new MarginPadding { Horizontal = 10 },
                        Font = OsuFont.GetFont(size: 14, weight: FontWeight.SemiBold),
                    }
                }
            };
        }

        private class CancelButton : Button
        {
            public readonly BindableBool Expanded = new BindableBool();

            public CancelButton()
                : base(@"Cancel")
            {
                Action = () => Expanded.Value = false;
            }

            protected override void OnLoadStarted() => IsLoading = false;
        }
    }
}
