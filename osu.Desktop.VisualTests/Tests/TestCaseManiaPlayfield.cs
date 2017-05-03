// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input;
using OpenTK.Input;
using osu.Game.Rulesets.UI;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseManiaPlayfield : TestCase
    {
        public override string Description => @"Mania playfield";

        protected override double TimePerAction => 200;

        public override void Reset()
        {
            base.Reset();

            int max_columns = 9;

            for (int i = 1; i <= max_columns; i++)
            {
                int tempI = i;

                AddStep($@"{i} column" + (i > 1 ? "s" : ""), () =>
                {
                    Clear();
                    Add(new ManiaPlayfield(tempI)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    });
                });

                AddStep($"Trigger keys down", () => ((ManiaPlayfield)Children.First()).Columns.Children.ForEach(triggerKeyDown));
                AddStep($"Trigger keys up", () => ((ManiaPlayfield)Children.First()).Columns.Children.ForEach(triggerKeyUp));
            }
        }

        private void triggerKeyDown(Column column)
        {
            column.TriggerKeyDown(new InputState(), new KeyDownEventArgs
            {
                Key = column.Key,
                Repeat = false
            });
        }

        private void triggerKeyUp(Column column)
        {
            column.TriggerKeyUp(new InputState(), new KeyUpEventArgs
            {
                Key = column.Key
            });
        }
    }

    public class ManiaPlayfield : Container
    {
        public readonly FlowContainer<Column> Columns;

        public ManiaPlayfield(int columnCount)
        {
            if (columnCount > 9)
                throw new ArgumentException($@"{columnCount} columns is not supported.");
            if (columnCount <= 0)
                throw new ArgumentException($@"Can't have zero or fewer columns.");

            RelativeSizeAxes = Axes.Y;
            AutoSizeAxes = Axes.X;

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
            };

            for (int i = 0; i < columnCount; i++)
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

    public class Column : Container, IHasAccentColour
    {
        private const float key_size = 50;

        private const float key_icon_size = 10;
        private const float key_icon_corner_radius = 3;
        private const float key_icon_border_radius = 2;

        private const float hit_target_height = 10;
        private const float hit_target_bar_height = 2;

        private const float column_width = 45;
        private const float special_column_width = 70;

        private Color4 accentColour;
        public Color4 AccentColour
        {
            get { return accentColour; }
            set
            {
                if (accentColour == value)
                    return;
                accentColour = value;

                setAccentColour();
            }
        }

        private bool isSpecialColumn;
        public bool IsSpecialColumn
        {
            get { return isSpecialColumn; }
            set
            {
                isSpecialColumn = value;
                Width = isSpecialColumn ? special_column_width : column_width;
            }
        }

        public Key Key;

        private Box background;
        private Container hitTargetBar;
        private Container keyIcon;

        public Column()
        {
            RelativeSizeAxes = Axes.Y;
            Width = column_width;

            Children = new Drawable[]
            {
                background = new Box
                {
                    Name = "Foreground",
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.2f
                },
                new FillFlowContainer
                {
                    Name = "Key + hit target",
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new[]
                    {
                        new Container
                        {
                            Name = "Key",
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            RelativeSizeAxes = Axes.X,
                            Height = key_size,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Name = "Key gradient",
                                    RelativeSizeAxes = Axes.Both,
                                    ColourInfo = ColourInfo.GradientVertical(Color4.Black, Color4.Black.Opacity(0)),
                                    Alpha = 0.5f
                                },
                                keyIcon = new Container
                                {
                                    Name = "Key icon",
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Size = new Vector2(key_icon_size),
                                    Masking = true,
                                    CornerRadius = key_icon_corner_radius,
                                    BorderThickness = 2,
                                    BorderColour = Color4.White, // Not true
                                    Children = new[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Alpha = 0,
                                            AlwaysPresent = true
                                        }
                                    }
                                }
                            }
                        },
                        new Container
                        {
                            Name = "Hit target",
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            RelativeSizeAxes = Axes.X,
                            Height = hit_target_height,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Name = "Background",
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.Black
                                },
                                hitTargetBar = new Container
                                {
                                    Name = "Bar",
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomCentre,
                                    RelativeSizeAxes = Axes.X,
                                    Height = hit_target_bar_height,
                                    Masking = true,
                                    Children = new[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        private void setAccentColour()
        {
            background.Colour = AccentColour;

            hitTargetBar.EdgeEffect = new EdgeEffect
            {
                Type = EdgeEffectType.Glow,
                Radius = 5,
                Colour = AccentColour.Opacity(0.5f),
            };

            keyIcon.EdgeEffect = new EdgeEffect
            {
                Type = EdgeEffectType.Glow,
                Radius = 5,
                Colour = AccentColour.Opacity(0.5f),
            };
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Key == Key && !args.Repeat)
            {
                background.FadeTo(background.Alpha + 0.2f, 50, EasingTypes.OutQuint);
                keyIcon.ScaleTo(1.4f, 50, EasingTypes.OutQuint);
            }

            return false;
        }

        protected override bool OnKeyUp(InputState state, KeyUpEventArgs args)
        {
            if (args.Key == Key)
            {
                background.FadeTo(0.2f, 800, EasingTypes.OutQuart);
                keyIcon.ScaleTo(1f, 400, EasingTypes.OutQuart);
            }

            return false;
        }
    }
}
