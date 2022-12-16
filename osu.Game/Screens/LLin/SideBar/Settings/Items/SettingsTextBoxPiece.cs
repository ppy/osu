using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.LLin.SideBar.Settings.Items
{
    public partial class SettingsTextBoxPiece : SettingsPieceBasePanel, ISettingsItem<string>
    {
        public Bindable<string> Bindable { get; set; }

        public LocalisableString TooltipText { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            Bindable.BindValueChanged(onBindableChanged);
        }

        protected override IconUsage DefaultIcon => FontAwesome.Solid.ToggleOn;

        protected override void OnColorChanged()
        {
            base.OnColorChanged();
        }

        private void onBindableChanged(ValueChangedEvent<string> v)
        {
            textBox.Text = v.NewValue;
        }

        protected override void OnLeftClick()
        {
        }

        protected override void OnRightClick() => OnLeftClick();

        protected override void OnMiddleClick()
        {
        }

        private readonly PieceTextBox textBox = new PieceTextBox();

        protected override Drawable CreateSideDrawable()
        {
            return textBox;
        }

        private partial class PieceTextBox : OsuTextBox
        {
        }
    }
}
