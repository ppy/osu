using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;

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
            var items = typeof(T).GetFields().Where(f => !f.IsSpecialName).Zip(
                (T[])Enum.GetValues(typeof(T)), (a, b) => new Tuple<string, T>(
                    a.GetCustomAttribute<DescriptionAttribute>()?.Description ?? a.Name, b));
            Children = new Drawable[]
            {
                text = new SpriteText { Alpha = 0 },
                dropdown = new StyledDropDownMenu<T>
                {
                    Margin = new MarginPadding { Top = 5 },
                    RelativeSizeAxes = Axes.X,
                    Items = items.Select(item => new StyledDropDownMenuItem<T>(item.Item1, item.Item2))
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

            public StyledDropDownMenu()
            {
                ComboBox.CornerRadius = 4;
                DropDown.CornerRadius = 4;
            }

            protected override void AnimateOpen()
            {
                foreach (StyledDropDownMenuItem<U> child in DropDownList.Children)
                {
                    child.FadeIn(200);
                    child.ResizeTo(new Vector2(1, 24), 200);
                }
                DropDown.Show();
            }

            protected override void AnimateClose()
            {
                foreach (StyledDropDownMenuItem<U> child in DropDownList.Children)
                {
                    child.ResizeTo(new Vector2(1, 0), 200);
                    child.FadeOut(200);
                }
            }
        }

        private class StyledDropDownComboBox : DropDownComboBox
        {
            protected override Color4 BackgroundColour => new Color4(255, 255, 255, 100);
            protected override Color4 BackgroundColourHover => Color4.HotPink;

            public StyledDropDownComboBox()
            {
                Foreground.Padding = new MarginPadding(4);
            }
        }

        private class StyledDropDownMenuItem<U> : DropDownMenuItem<U>
        {
            public StyledDropDownMenuItem(string text, U value) : base(text, value)
            {
                AutoSizeAxes = Axes.None;
                Height = 0;
                Foreground.Padding = new MarginPadding(2);
            }

            protected override void OnSelectChange()
            {
                if (!IsLoaded)
                    return;

                FormatBackground();
                FormatCaret();
                FormatLabel();
            }

            protected override void FormatCaret()
            {
                (Caret as SpriteText).Text = IsSelected ? @"+" : @"-";
            }

            protected override void FormatLabel()
            {
                if (IsSelected)
                    (Label as SpriteText).Text = @"*" + Value + @"*";
                else
                    (Label as SpriteText).Text = Value.ToString();
            }
        }
    }
}
