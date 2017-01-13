using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Options
{
    public class DropdownOption<T> : FlowContainer
    {
        private DropDownMenu<T> dropdown;
        private SpriteText text;
    
        public string LabelText
        {
            get { return text.Text; }
            set
            {
                text.Text = value;
                text.Alpha = string.IsNullOrEmpty(value) ? 0 : 1;
            }
        }
        
        public Bindable<T> Bindable
        {
            get { return bindable; }
            set
            {
                if (bindable != null)
                    bindable.ValueChanged -= Bindable_ValueChanged;
                bindable = value;
                bindable.ValueChanged += Bindable_ValueChanged;
                Bindable_ValueChanged(null, null);
            }
        }

        private Bindable<T> bindable;

        void Bindable_ValueChanged(object sender, EventArgs e)
        {
            dropdown.SelectedValue = bindable.Value;
        }

        void Dropdown_ValueChanged(object sender, EventArgs e)
        {
            bindable.Value = dropdown.SelectedValue;
        }
        
        protected override void Dispose(bool isDisposing)
        {
            bindable.ValueChanged -= Bindable_ValueChanged;
            dropdown.ValueChanged -= Dropdown_ValueChanged;
            base.Dispose(isDisposing);
        }

        public DropdownOption()
        {
            if (!typeof(T).IsEnum)
                throw new InvalidOperationException("OptionsDropdown only supports enums as the generic type argument");
            Direction = FlowDirection.VerticalOnly;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Children = new Drawable[]
            {
                text = new SpriteText { Alpha = 0 },
                dropdown = new StyledDropDownMenu<T>
                {
                    Margin = new MarginPadding { Top = 5 },
                    RelativeSizeAxes = Axes.X,
                    Items = (T[])Enum.GetValues(typeof(T)),
                }
            };
            dropdown.ValueChanged += Dropdown_ValueChanged;
        }
        
        private class StyledDropDownMenu<U> : DropDownMenu<U>
        {
            protected override float DropDownListSpacing => 4;

            protected override DropDownComboBox CreateComboBox()
            {
                return new StyledDropDownComboBox();
            }
            
            protected override IEnumerable<DropDownMenuItem<U>> GetDropDownItems(IEnumerable<U> values)
            {
                return values.Select(v =>
                {
                    var field = typeof(U).GetField(Enum.GetName(typeof(U), v));
                    return new StyledDropDownMenuItem<U>(
                        field.GetCustomAttribute<DescriptionAttribute>()?.Description ?? field.Name, v);
                });
                    
            }

            public StyledDropDownMenu()
            {
                ComboBox.CornerRadius = 4;
                DropDown.CornerRadius = 4;
            }

            protected override void AnimateOpen()
            {
                foreach (StyledDropDownMenuItem<U> child in DropDownItemsContainer.Children)
                {
                    child.FadeIn(200);
                    child.ResizeTo(new Vector2(1, 24), 200);
                }
                DropDown.Show();
            }

            protected override void AnimateClose()
            {
                foreach (StyledDropDownMenuItem<U> child in DropDownItemsContainer.Children)
                {
                    child.ResizeTo(new Vector2(1, 0), 200);
                    child.FadeOut(200);
                }
            }
        }

        private class StyledDropDownComboBox : DropDownComboBox
        {
            private SpriteText label;
            protected override string Label
            {
                get { return label.Text; }
                set { label.Text = value; }
            }

            public StyledDropDownComboBox()
            {
                Foreground.Padding = new MarginPadding(4);

                Children = new[]
                {
                    label = new SpriteText(),
                    new TextAwesome
                    {
                        Icon = FontAwesome.fa_chevron_down,
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        Margin = new MarginPadding { Right = 4 },
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                BackgroundColour = Color4.Black.Opacity(0.5f);
                BackgroundColourHover = colours.PinkDarker;
            }
        }

        private class StyledDropDownMenuItem<U> : DropDownMenuItem<U>
        {
            public StyledDropDownMenuItem(string text, U value) : base(text, value)
            {
                AutoSizeAxes = Axes.None;
                Height = 0;
                Foreground.Padding = new MarginPadding(2);

                Children = new[]
                {
                    new FlowContainer
                    {
                        Direction = FlowDirection.HorizontalOnly,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            new TextAwesome
                            {
                                Icon = FontAwesome.fa_chevron_right,
                                Colour = Color4.Black,
                                Margin = new MarginPadding { Right = 3 },
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                            },
                            new SpriteText { Text = text }
                        }
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                BackgroundColour = Color4.Black.Opacity(0.5f);
                BackgroundColourHover = colours.PinkDarker;
                BackgroundColourSelected = Color4.Black.Opacity(0.5f);
            }
        }
    }
}
