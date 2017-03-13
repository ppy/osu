// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using System.Collections.Generic;

namespace osu.Game.Overlays.Options
{
    public class OptionDropDown<T> : FillFlowContainer
    {
        private DropDownMenu<T> dropdown;
        private SpriteText text;

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
                if (bindable != null)
                    bindable.ValueChanged -= bindable_ValueChanged;
                bindable = value;
                bindable.ValueChanged += bindable_ValueChanged;
                bindable_ValueChanged(null, null);

                if (bindable.Disabled)
                    Alpha = 0.3f;
            }
        }

        private Bindable<T> bindable;

        private void bindable_ValueChanged(object sender, EventArgs e)
        {
            dropdown.SelectedValue = bindable.Value;
        }

        private void dropdown_ValueChanged(object sender, EventArgs e)
        {
            bindable.Value = dropdown.SelectedValue;
        }

        protected override void Dispose(bool isDisposing)
        {
            bindable.ValueChanged -= bindable_ValueChanged;
            dropdown.ValueChanged -= dropdown_ValueChanged;
            base.Dispose(isDisposing);
        }

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
                if(dropdown != null)
                {
                    dropdown.Items = value;

                    // We need to refresh the dropdown because our items changed,
                    // thus its selected value may be outdated.
                    if (bindable != null)
                        dropdown.SelectedValue = bindable.Value;
                }
            }
        }

        public OptionDropDown()
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
                dropdown = new OsuDropDownMenu<T>
                {
                    Margin = new MarginPadding { Top = 5 },
                    RelativeSizeAxes = Axes.X,
                    Items = Items,
                }
            };
            dropdown.ValueChanged += dropdown_ValueChanged;
        }
    }
}
