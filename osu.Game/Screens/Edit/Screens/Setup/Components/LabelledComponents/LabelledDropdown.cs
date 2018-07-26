// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Screens.Edit.Screens.Setup.Components.LabelledComponents
{
    public class LabelledDropdown<T> : CompositeDrawable
    {
        private readonly OsuDropdown<T> dropdown;
        private readonly Container content;
        private readonly OsuSpriteText label;

        public const float LABEL_CONTAINER_WIDTH = 150;
        public const float OUTER_CORNER_RADIUS = 15;
        public const float INNER_CORNER_RADIUS = 10;
        public const float DEFAULT_HEADER_TEXT_SIZE = 20;
        public const float DEFAULT_HEIGHT = 40;
        public const float DEFAULT_LABEL_TEXT_SIZE = 16;
        public const float DEFAULT_LEFT_PADDING = 15;
        public const float DEFAULT_TOP_PADDING = 12;
        public const float DEFAULT_HEADER_TEXT_PADDING = 11;
        public const float DEFAULT_HEADER_ICON_PADDING = 10;

        public event Action<T> DropdownSelectionChanged;

        public void TriggerDropdownSelectionChanged(T newValue)
        {
            DropdownSelectionChanged?.Invoke(newValue);
        }

        private string labelText;
        public string LabelText
        {
            get => labelText;
            set
            {
                labelText = value;
                label.Text = value;
            }
        }

        private float labelTextSize;
        public float LabelTextSize
        {
            get => labelTextSize;
            set
            {
                labelTextSize = value;
                label.TextSize = value;
            }
        }

        public T DropdownSelectedItem
        {
            get => dropdown.Current.Value;
            set => dropdown.Current.Value = value;
        }

        public int DropdownSelectedIndex
        {
            set => dropdown.Current.Value = dropdown.Items.ElementAt(value).Value;
        }

        public IEnumerable<KeyValuePair<string, T>> Items
        {
            get => dropdown.Items;
            private set => dropdown.Items = value;
        }

        private float height = DEFAULT_HEIGHT;
        public float Height
        {
            get => height;
            private set
            {
                height = value;
                dropdown.Height = value;
                content.Height = value;
            }
        }

        public MarginPadding Padding
        {
            get => base.Padding;
            set
            {
                base.Padding = value;
                base.Height = Height + base.Padding.Top;
            }
        }

        public MarginPadding LabelPadding
        {
            get => label.Padding;
            set => label.Padding = value;
        }

        public MarginPadding TextBoxPadding
        {
            get => dropdown.Padding;
            set => dropdown.Padding = value;
        }

        public Color4 LabelTextColour
        {
            get => label.Colour;
            set => label.Colour = value;
        }

        public Color4 BackgroundColour
        {
            get => content.Colour;
            set => content.Colour = value;
        }

        public LabelledDropdown()
        {
            RelativeSizeAxes = Axes.X;
            base.Height = DEFAULT_HEIGHT + Padding.Top;

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = DEFAULT_HEIGHT,
                    CornerRadius = OUTER_CORNER_RADIUS,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = DEFAULT_HEIGHT,
                            Colour = OsuColour.FromHex("1c2125"),
                        },
                    }
                },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.X,
                    Height = DEFAULT_HEIGHT,
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            label = new OsuSpriteText
                            {
                                Anchor = Anchor.TopLeft,
                                Origin = Anchor.TopLeft,
                                Padding = new MarginPadding { Left = DEFAULT_LEFT_PADDING, Top = DEFAULT_TOP_PADDING },
                                Colour = Color4.White,
                                TextSize = DEFAULT_LABEL_TEXT_SIZE,
                                Text = LabelText,
                                Font = @"Exo2.0-Bold",
                            },
                            dropdown = CreateDropdown(),
                        },
                    },
                    ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Absolute, LABEL_CONTAINER_WIDTH),
                        new Dimension()
                    }
                }
            };

            dropdown.Current.ValueChanged += delegate { TriggerDropdownSelectionChanged(dropdown.Current.Value); };
        }

        public void AddDropdownItem(string text, T value) => dropdown.AddDropdownItem(text, value);

        public void AddDropdownItems(IEnumerable<KeyValuePair<string, T>> items)
        {
            foreach (var i in items)
                dropdown.AddDropdownItem(i.Key, i.Value);
        }

        public void RemoveDropdownItem(T value) => dropdown.RemoveDropdownItem(value);

        protected virtual OsuDropdown<T> CreateDropdown() => new SetupDropdown<T>();
    }
}
