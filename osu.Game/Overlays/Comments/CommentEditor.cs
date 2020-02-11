// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Graphics.Sprites;
using osuTK.Graphics;

namespace osu.Game.Overlays.Comments
{
    public abstract class CommentEditor : CompositeDrawable
    {
        private const int side_padding = 8;

        protected abstract string FooterText { get; }

        protected abstract string CommitButtonText { get; }

        protected abstract string TextboxPlaceholderText { get; }

        protected FillFlowContainer ButtonsContainer;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Masking = true;
            CornerRadius = 6;
            BorderThickness = 3;
            BorderColour = colourProvider.Background3;

            AddRangeInternal(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background3
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new EditorTextbox
                        {
                            Height = 40,
                            RelativeSizeAxes = Axes.X,
                            PlaceholderText = TextboxPlaceholderText
                        },
                        new Container
                        {
                            Name = "Footer",
                            RelativeSizeAxes = Axes.X,
                            Height = 35,
                            Padding = new MarginPadding { Horizontal = side_padding },
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Font = OsuFont.GetFont(size: 12, weight: FontWeight.SemiBold),
                                    Text = FooterText
                                },
                                ButtonsContainer = new FillFlowContainer
                                {
                                    Name = "Buttons",
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                }
                            }
                        }
                    }
                }
            });
        }

        private class EditorTextbox : BasicTextBox
        {
            protected override float LeftRightPadding => side_padding;

            protected override Color4 SelectionColour => Color4.LightSkyBlue;

            private OsuSpriteText placeholder;

            public EditorTextbox()
            {
                Masking = false;
                TextContainer.Height = 0.4f;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                BackgroundUnfocused = BackgroundFocused = colourProvider.Background5;
                placeholder.Colour = colourProvider.Background3;
                BackgroundCommit = Color4.LightSkyBlue;
            }


            protected override SpriteText CreatePlaceholder() => placeholder = new OsuSpriteText
            {
                Font = OsuFont.GetFont(weight: FontWeight.Regular),
            };

            protected override Drawable GetDrawableCharacter(char c) => new OsuSpriteText { Text = c.ToString(), Font = OsuFont.GetFont(size: CalculatedTextSize) };
        }
    }
}
