// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Edit.List
{
    public class DrawableContainer : DrawableList
    {
        private readonly DrawableListItem item;
        private readonly OsuSpriteText text;
        private readonly GridContainer gridContainer;

        public DrawableContainer()
        {
            gridContainer = new GridContainer
            {
                AutoSizeAxes = Axes.Both,
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension(GridSizeMode.AutoSize)
                },
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension()
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        text = new OsuSpriteText
                        {
                            Text = @"v",
                            Padding = new MarginPadding
                            {
                                Right = 5
                            },
                        },
                        item = new DrawableListItem(new Container(), @"SkinnableContainer"),
                    },
                    new Drawable?[]
                    {
                        null,
                        Container
                    }
                }
            };
        }

        public override Drawable GetDrawableListItem() => gridContainer;
    }
}
