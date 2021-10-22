using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osuTK.Graphics;

namespace osu.Game.Screens.LLin.SideBar.Settings.Items
{
    public class SettingsTogglePiece : SettingsPieceBasePanel, ISettingsItem<bool>
    {
        public Bindable<bool> Bindable { get; set; }

        public LocalisableString TooltipText { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            Bindable.BindValueChanged(onBindableChanged);
        }

        protected override IconUsage DefaultIcon => FontAwesome.Solid.ToggleOn;

        protected override void OnColorChanged()
        {
            BgBox.Colour = Bindable.Value ? ColourProvider.ActiveColor : ColourProvider.InActiveColor;
            FillFlow.Colour = Bindable.Value ? Color4.Black : Color4.White;
        }

        private void onBindableChanged(ValueChangedEvent<bool> v)
        {
            switch (v.NewValue)
            {
                case true:
                    BgBox.FadeColour(ColourProvider.ActiveColor, 300, Easing.OutQuint);
                    FillFlow.FadeColour(Color4.Black, 300, Easing.OutQuint);
                    break;

                case false:
                    BgBox.FadeColour(ColourProvider.InActiveColor, 300, Easing.OutQuint);
                    FillFlow.FadeColour(Color4.White, 300, Easing.OutQuint);
                    break;
            }
        }

        protected override void OnLeftClick()
        {
            Bindable.Value = !Bindable.Value;
        }

        protected override void OnRightClick() => OnLeftClick();

        protected override void OnMiddleClick()
        {
            Bindable.Value = Bindable.Default;
        }
    }
}
