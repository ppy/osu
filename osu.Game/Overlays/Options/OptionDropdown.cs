// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options
{
    public class OptionDropdown<T> : FillFlowContainer
    {
        private readonly Dropdown<T> dropdown;
        private readonly SpriteText text;

        public string LabelText
        {
            get { return text.Text; }
            set
            {
                text.Text = value;
            }
        }

        public Bindable<T> Bindable
        {
            get { return bindable; }
            set
            {
                bindable = value;
                dropdown.Current.BindTo(bindable);
            }
        }

        private Bindable<T> bindable;

        private IEnumerable<KeyValuePair<string, T>> items;
        public IEnumerable<KeyValuePair<string, T>> Items
        {
            get
            {
                return items;
            }
            set
            {
                items = value;
                if (dropdown != null)
                    dropdown.Items = value;
            }
        }

        public OptionDropdown()
        {
            Items = new KeyValuePair<string, T>[0];

            Direction = FillDirection.Vertical;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Children = new Drawable[]
            {
                text = new OsuSpriteText {
                    Alpha = 0,
                },
                dropdown = new OsuDropdown<T>
                {
                    Margin = new MarginPadding { Top = 5 },
                    RelativeSizeAxes = Axes.X,
                    Items = Items,
                }
            };

            dropdown.Current.DisabledChanged += disabled =>
            {
                Alpha = disabled ? 0.3f : 1;
            };
        }
    }
}
