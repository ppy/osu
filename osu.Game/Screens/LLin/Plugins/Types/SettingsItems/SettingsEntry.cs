using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.LLin.SideBar.Settings.Items;

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

        public IconUsage Icon = FontAwesome.Regular.QuestionCircle;

        protected bool AllowChangeName;
        protected bool AllowChangeDescription;

        private LocalisableString name;
        private LocalisableString desc;

        public abstract Drawable ToSettingsItem();
        public abstract Drawable ToLLinSettingsItem();
    }

    public class SeparatorSettingsEntry : SettingsEntry
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
            return new SettingsSeparatorPiece
            {
                Description = Name,
                Icon = this.Icon
            };
        }
    }

    public class NumberSettingsEntry<T> : SettingsEntry
        where T : struct, IEquatable<T>, IComparable<T>, IConvertible
    {
        public bool DisplayAsPercentage = false;
        public float KeyboardStep = 0.1f;
        public bool CommitOnMouseRelease = false;

        public NumberSettingsEntry()
        {
            Icon = FontAwesome.Solid.SlidersH;
        }

        public override Drawable ToSettingsItem()
        {
            return new Overlays.Settings.SettingsSlider<T>
            {
                //todo: 感觉这么做有些dirty，但起码能用
                //todo: 可以换成"Current = { BindTarget = ... }"这样？
                Current = (Bindable<T>)Bindable.GetBoundCopy(),
                LabelText = Name,
                TooltipText = Description.ToString(),
                DisplayAsPercentage = this.DisplayAsPercentage,
                KeyboardStep = this.KeyboardStep,
                TransferValueOnCommit = CommitOnMouseRelease
            };
        }

        public override Drawable ToLLinSettingsItem()
        {
            return new SettingsSliderPiece<T>
            {
                Description = Name,
                TooltipText = Description,
                Bindable = (Bindable<T>)Bindable.GetBoundCopy(),
                Icon = this.Icon,
                DisplayAsPercentage = this.DisplayAsPercentage,
                TransferValueOnCommit = CommitOnMouseRelease
            };
        }
    }

    public class BooleanSettingsEntry : SettingsEntry
    {
        public BooleanSettingsEntry()
        {
            Icon = FontAwesome.Solid.ToggleOn;
        }

        public override Drawable ToSettingsItem()
        {
            return new SettingsCheckbox
            {
                Current = (Bindable<bool>)Bindable.GetBoundCopy(),
                LabelText = Name,
                TooltipText = Description
            };
        }

        public override Drawable ToLLinSettingsItem()
        {
            return new SettingsTogglePiece
            {
                Description = Name,
                TooltipText = Description,
                Bindable = (Bindable<bool>)Bindable.GetBoundCopy(),
                Icon = this.Icon
            };
        }
    }

    public class ListSettingsEntry<T> : SettingsEntry
    {
        public IEnumerable<T> Values;

        public ListSettingsEntry()
        {
            Icon = FontAwesome.Solid.List;
        }

        public override Drawable ToSettingsItem()
        {
            return new SettingsDropdown<T>
            {
                LabelText = Name,
                Current = (Bindable<T>)Bindable.GetBoundCopy(),
                Items = Values
            };
        }

        public override Drawable ToLLinSettingsItem()
        {
            return new SettingsListPiece<T>
            {
                Description = Name,
                TooltipText = Description,
                Bindable = (Bindable<T>)Bindable.GetBoundCopy(),
                Icon = this.Icon,
                Values = this.Values.ToList()
            };
        }
    }

    public class StringSettingsEntry : SettingsEntry
    {
        public StringSettingsEntry()
        {
            Icon = FontAwesome.Solid.TextWidth;
        }

        public override Drawable ToSettingsItem()
        {
            return new SettingsTextBox
            {
                Current = (Bindable<string>)Bindable.GetBoundCopy(),
                LabelText = Name,
                TooltipText = Description.ToString()
            };
        }

        public override Drawable ToLLinSettingsItem()
        {
            return new SettingsStringPiece
            {
                Bindable = (Bindable<string>)Bindable.GetBoundCopy(),
                Description = Name,
                TooltipText = Description,
                Icon = this.Icon
            };
        }
    }

    public class EnumSettingsEntry<T> : SettingsEntry
        where T : struct, Enum
    {
        public EnumSettingsEntry()
        {
            Icon = FontAwesome.Solid.List;
        }

        public override Drawable ToSettingsItem()
        {
            return new SettingsEnumDropdown<T>
            {
                Current = (Bindable<T>)Bindable.GetBoundCopy(),
                LabelText = Name,
                TooltipText = Description.ToString()
            };
        }

        public override Drawable ToLLinSettingsItem()
        {
            return new SettingsEnumPiece<T>
            {
                Description = Name,
                TooltipText = Description,
                Bindable = (Bindable<T>)Bindable.GetBoundCopy(),
                Icon = this.Icon
            };
        }
    }
}
