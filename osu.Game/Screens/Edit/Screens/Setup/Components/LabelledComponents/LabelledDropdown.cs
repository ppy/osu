// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Screens.Edit.Screens.Setup.Components.LabelledComponents
{
    public class LabelledDropdown<T> : CompositeDrawable, IHasCurrentValue<T>
    {
        private readonly OsuDropdown<T> dropdown;
        private readonly Container content;
        private readonly OsuSpriteText label;

        private const float label_container_width = 150;
        private const float corner_radius = 15;
        private const float default_header_text_size = 20;
        private const float default_height = 40;
        private const float default_label_text_size = 16;
        private const float default_horizontal_offset = 15;
        private const float default_vertical_offset = 12;

        public Bindable<T> Current { get; } = new Bindable<T>();
        
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

        public int DropdownSelectedIndex
        {
            set => dropdown.Current.Value = dropdown.Items.ElementAt(value).Value;
        }

        public IEnumerable<KeyValuePair<string, T>> Items
        {
            get => dropdown.Items;
            private set => dropdown.Items = value;
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
            Height = default_height;

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    CornerRadius = corner_radius,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = OsuColour.FromHex("1c2125"),
                        },
                    }
                },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            label = new OsuSpriteText
                            {
                                Anchor = Anchor.TopLeft,
                                Origin = Anchor.TopLeft,
                                Position = new Vector2(default_horizontal_offset, default_vertical_offset),
                                Colour = Color4.White,
                                TextSize = default_label_text_size,
                                Text = LabelText,
                                Font = @"Exo2.0-Bold",
                            },
                            dropdown = CreateDropdown(),
                        },
                    },
                    ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Absolute, label_container_width),
                        new Dimension()
                    }
                }
            };

            Current.BindTo(dropdown.Current);
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
