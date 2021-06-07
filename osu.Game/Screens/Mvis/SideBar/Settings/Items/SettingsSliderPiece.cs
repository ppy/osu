using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;

namespace osu.Game.Screens.Mvis.SideBar.Settings.Items
{
    public class SettingsSliderPiece<T> : SettingsPieceBasePanel, ISettingsItem<T>
        where T : struct, IEquatable<T>, IComparable<T>, IConvertible
    {
        public Bindable<T> Bindable { get; set; }

        public string TooltipText { get; set; }

        public bool DisplayAsPercentage;
        public bool TransferValueOnCommit;

        public SettingsSliderPiece()
        {
            TooltipText = TooltipText + "点击重置";
        }

        protected override IconUsage DefaultIcon => FontAwesome.Solid.SlidersH;

        protected override Drawable CreateSideDrawable() => new SettingsSlider<T>
        {
            RelativeSizeAxes = Axes.Both,
            Current = Bindable,
            DisplayAsPercentage = DisplayAsPercentage,
            TransferValueOnCommit = TransferValueOnCommit,
        };

        protected override bool OnClick(ClickEvent e)
        {
            Bindable.Value = Bindable.Default;
            return base.OnClick(e);
        }
    }
}
