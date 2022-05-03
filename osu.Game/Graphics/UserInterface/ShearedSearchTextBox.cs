// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    public class ShearedSearchTextBox : CompositeDrawable, IHasCurrentValue<string>
    {
        private const float icon_container_width = 50;
        private const float corner_radius = 7;
        private const float height = 42;
        private readonly Vector2 shear = new Vector2(ShearedOverlayContainer.SHEAR, 0);

        public FocusedTextBox TextBox;

        public Bindable<string> Current
        {
            get => TextBox.Current;
            set => TextBox.Current = value;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Height = height;
            Shear = shear;
            Masking = true;
            CornerRadius = corner_radius;
            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Box
                    {
                        Colour = colourProvider.Background3,
                        RelativeSizeAxes = Axes.Both
                    },
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                new Container
                                {
                                    Name = @"Search box container",
                                    RelativeSizeAxes = Axes.Both,
                                    CornerRadius = corner_radius,
                                    Masking = true,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            Colour = colourProvider.Background4,
                                            RelativeSizeAxes = Axes.Both
                                        },
                                        TextBox = new SearchTextBox
                                        {
                                            Shear = -shear,
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            RelativeSizeAxes = Axes.X,
                                            Padding = new MarginPadding
                                            {
                                                Horizontal = corner_radius + shear.X
                                            }
                                        }
                                    }
                                },
                                new Container
                                {
                                    Name = @"Icon container",
                                    RelativeSizeAxes = Axes.Y,
                                    Width = icon_container_width,
                                    Origin = Anchor.CentreRight,
                                    Anchor = Anchor.CentreRight,
                                    Children = new Drawable[]
                                    {
                                        new SpriteIcon
                                        {
                                            Icon = FontAwesome.Solid.Search,
                                            Origin = Anchor.Centre,
                                            Anchor = Anchor.Centre,
                                            Size = new Vector2(16),
                                            Shear = -shear
                                        }
                                    }
                                }
                            }
                        },
                        ColumnDimensions = new[] { new Dimension(), new Dimension(GridSizeMode.AutoSize) }
                    }
                }
            };
        }

        public override bool HandleNonPositionalInput => TextBox.HandleNonPositionalInput;

        private class InnerSearchTextBox : SearchTextBox
        {

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                BackgroundFocused = colourProvider.Background4;
                BackgroundUnfocused = colourProvider.Background4;
                Placeholder.Colour = Color4.White;
                PlaceholderText = @"Search";
            }

            protected override SpriteText CreatePlaceholder() => new OsuSpriteText
            {
                Font = OsuFont.GetFont(size: 20, weight: FontWeight.SemiBold)
            };

            protected override Drawable GetDrawableCharacter(char c) => new FallingDownContainer
            {
                AutoSizeAxes = Axes.Both,
                Child = new OsuSpriteText { Text = c.ToString(), Font = OsuFont.GetFont(size: 20, weight: FontWeight.SemiBold) },
            };
        }
    }
}
