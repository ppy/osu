// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Collections
{
    public class CollectionDialog : OsuFocusedOverlayContainer
    {
        private const double enter_duration = 500;
        private const double exit_duration = 200;

        [Resolved]
        private CollectionManager collectionManager { get; set; }

        public CollectionDialog()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(0.5f, 0.8f);

            Masking = true;
            CornerRadius = 10;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Children = new Drawable[]
            {
                new Box
                {
                    Colour = colours.GreySeafoamDark,
                    RelativeSizeAxes = Axes.Both,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        RowDimensions = new[]
                        {
                            new Dimension(GridSizeMode.Absolute, 50),
                            new Dimension(),
                            new Dimension(GridSizeMode.Absolute, 50),
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Text = "Manage collections",
                                    Font = OsuFont.GetFont(size: 30),
                                }
                            },
                            new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = colours.GreySeafoamDarker
                                        },
                                        new CollectionList
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Items = { BindTarget = collectionManager.Collections }
                                        }
                                    }
                                }
                            },
                            new Drawable[]
                            {
                                new OsuButton
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Size = Vector2.One,
                                    Padding = new MarginPadding(10),
                                    Text = "Create new collection",
                                    Action = () => collectionManager.Collections.Add(new BeatmapCollection { Name = "My new collection" })
                                },
                            },
                        }
                    }
                }
            };
        }

        protected override void PopIn()
        {
            base.PopIn();

            this.FadeIn(enter_duration, Easing.OutQuint);
            this.ScaleTo(0.9f).Then().ScaleTo(1f, enter_duration, Easing.OutElastic);
        }

        protected override void PopOut()
        {
            base.PopOut();

            this.FadeOut(exit_duration, Easing.OutQuint);
            this.ScaleTo(0.9f, exit_duration);
        }
    }
}
