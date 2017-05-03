// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.UI;
using OpenTK;
using OpenTK.Graphics;
using osu.Game.Rulesets.Mania.Judgements;
using osu.Framework.Graphics.Containers;
using System;
using osu.Framework.Graphics.Primitives;
using osu.Game.Graphics;
using osu.Framework.Allocation;
using OpenTK.Input;
using System.Linq;

namespace osu.Game.Rulesets.Mania.UI
{
    public class ManiaPlayfield : Playfield<ManiaBaseHit, ManiaJudgement>
    {
        public readonly FlowContainer<Column> Columns;

        public ManiaPlayfield(int columns)
        {
            if (columns > 9)
                throw new ArgumentException($@"{columns} columns is not supported.");
            if (columns <= 0)
                throw new ArgumentException($@"Can't have zero or fewer columns.");

            Children = new Drawable[]
            {
                new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black
                        },
                        Columns = new FillFlowContainer<Column>
                        {
                            RelativeSizeAxes = Axes.Y,
                            AutoSizeAxes = Axes.X,
                            Direction = FillDirection.Horizontal,
                            Padding = new MarginPadding { Left = 1, Right = 1 },
                            Spacing = new Vector2(1, 0)
                        }
                    }
                }
            };

            for (int i = 0; i < columns; i++)
                Columns.Add(new Column());
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            var columnColours = new Color4[]
            {
                colours.RedDark,
                colours.GreenDark,
                colours.BlueDark // Special column
            };

            int columnCount = Columns.Children.Count();
            int halfColumns = columnCount / 2;

            var keys = new Key[] { Key.A, Key.S, Key.D, Key.F, Key.Space, Key.J, Key.K, Key.L, Key.Semicolon };

            for (int i = 0; i < halfColumns; i++)
            {
                Column leftColumn = Columns.Children.ElementAt(i);
                Column rightColumn = Columns.Children.ElementAt(columnCount - 1 - i);

                Color4 accent = columnColours[i % 2];
                leftColumn.AccentColour = rightColumn.AccentColour = accent;
                leftColumn.Key = keys[keys.Length / 2 - halfColumns + i];
                rightColumn.Key = keys[keys.Length / 2 + halfColumns - i];
            }

            bool hasSpecial = halfColumns * 2 < columnCount;
            if (hasSpecial)
            {
                Column specialColumn = Columns.Children.ElementAt(halfColumns);
                specialColumn.IsSpecialColumn = true;
                specialColumn.AccentColour = columnColours[2];
                specialColumn.Key = keys[keys.Length / 2];
            }
        }
    }
}