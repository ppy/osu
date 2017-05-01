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

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseManiaPlayfield : TestCase
    {
        public override string Description => @"Mania playfield";

        public override void Reset()
        {
            base.Reset();

            Add(new Column
            {
                AccentColour = new Color4(187, 17, 119, 255)
            });
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

        private Box foreground;
        private Container hitTargetBar;
        private Container keyIcon;

        public Column()
        {
            RelativeSizeAxes = Axes.Y;
            Width = column_width;

            Children = new Drawable[]
            {
                new Box
                {
                    Name = "Background",
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Left = 1, Right = 1 },
                    Children = new[]
                    {
                        foreground = new Box
                        {
                            Name = "Foreground",
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0.2f
                        },
                    }
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
                                new Box
                                {
                                    Name = "Key down foreground",
                                    Alpha = 0f,
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
            foreground.Colour = AccentColour;

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
    }
}
