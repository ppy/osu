using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Settings;

namespace osu.Game.Screens.LLin.Plugins.Types.SettingsItems
{
    public abstract class SettingsEntry
    {
        public IBindable Bindable { get; set; }

        public LocalisableString Name
        {
            get => name;
            set
            {
                if (string.IsNullOrEmpty(name.ToString()))
                    name = value;
                else if (AllowChangeName)
                    name = value;
                else
                    throw new InvalidOperationException("不允许覆盖原始名称");
            }
        }

        public LocalisableString Description
        {
            get => desc;
            set
            {
                if (string.IsNullOrEmpty(desc.ToString()))
                    desc = value;
                else if (AllowChangeDescription)
                    desc = value;
                else
                    throw new InvalidOperationException("不允许覆盖原始名称");
            }
        }

        protected bool AllowChangeName;
        protected bool AllowChangeDescription;

        private LocalisableString name;
        private LocalisableString desc;

        public abstract Drawable ToSettingsItem();
        public abstract Drawable ToLLinSettingsItem();
    }

    public class SpecratorSettingsEntry : SettingsEntry
    {
        public override Drawable ToSettingsItem()
        {
            return new OsuSpriteText
            {
                Text = Name,
                Font = OsuFont.GetFont(size: 19),
                Margin = new MarginPadding { Horizontal = 20, Vertical = (11.5f / 2) }
            };
        }

        public override Drawable ToLLinSettingsItem()
        {
            throw new NotImplementedException();
        }
    }

    public class NumberSettingsEntry<T> : SettingsEntry
        where T : struct, IEquatable<T>, IComparable<T>, IConvertible
    {
        public bool DisplayAsPercentage = false;
        public float KeyboardStep = 0.1f;

        public override Drawable ToSettingsItem()
        {
            return new SettingsSlider<T>
            {
                Current = (Bindable<T>)Bindable,
                LabelText = Name,
                TooltipText = Description.ToString(),
                DisplayAsPercentage = this.DisplayAsPercentage,
                KeyboardStep = this.KeyboardStep
            };
        }

        public override Drawable ToLLinSettingsItem()
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanSettingsEntry : SettingsEntry
    {
        public override Drawable ToSettingsItem()
        {
            return new SettingsCheckbox
            {
                Current = (Bindable<bool>)Bindable,
                LabelText = Name,
                TooltipText = Description
            };
        }

        public override Drawable ToLLinSettingsItem()
        {
            throw new NotImplementedException();
        }
    }

    public class ListSettingsEntry<T> : SettingsEntry
    {
        public IEnumerable<T> Values;

        public override Drawable ToSettingsItem()
        {
            return new SettingsDropdown<T>
            {
                LabelText = Name,
                Current = (Bindable<T>)Bindable,
                Items = Values
            };
        }

        public override Drawable ToLLinSettingsItem()
        {
            throw new NotImplementedException();
        }
    }

    public class StringSettingsEntry<T> : SettingsEntry
        where T : struct, IEquatable<T>, IComparable<T>, IConvertible
    {
        public override Drawable ToSettingsItem()
        {
            return new SettingsTextBox
            {
                Current = (Bindable<string>)Bindable,
                LabelText = Name,
                TooltipText = Description.ToString()
            };
        }

        public override Drawable ToLLinSettingsItem()
        {
            throw new NotImplementedException();
        }
    }

    public class EnumSettingsEntry<T> : SettingsEntry
        where T : struct, Enum
    {
        public override Drawable ToSettingsItem()
        {
            return new SettingsEnumDropdown<T>
            {
                Current = (Bindable<T>)Bindable,
                LabelText = Name,
                TooltipText = Description.ToString()
            };
        }

        public override Drawable ToLLinSettingsItem()
        {
            throw new NotImplementedException();
        }
    }
}
