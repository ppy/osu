using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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
                dropdown = new DropDownMenu<T>
                {
                    Margin = new MarginPadding { Top = 5 },
                    Height = 10,
                    RelativeSizeAxes = Axes.X,
                    Items = items.Select(item => new DropDownMenuItem<T>(item.Item1, item.Item2))
                }
            };
        }
    }
}