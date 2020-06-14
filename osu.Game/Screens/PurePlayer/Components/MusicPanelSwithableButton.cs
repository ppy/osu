using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;

namespace osu.Game.Screens.PurePlayer.Components
{
    public class MusicPanelSwitchableButton : MusicPanelButton
    {
        public BindableBool Value = new BindableBool();
        public Framework.Graphics.Colour.ColourInfo ActivateColor = Color4Extensions.FromHex(@"88b300");
        public Framework.Graphics.Colour.ColourInfo InActivateColor = Color4Extensions.FromHex(@"fff");
        protected override void LoadComplete()
        {
            Value.BindValueChanged(OnValueChanged, true);
        }

        private void OnValueChanged(ValueChangedEvent<bool> v)
        {
            switch ( v.NewValue )
            {
                case false:
                    contentFillFlow.FadeColour( InActivateColor, 500, Easing.OutQuint );
                    break;

                case true:
                    contentFillFlow.FadeColour( ActivateColor, 500, Easing.OutQuint );
                    break;
            }
        }

        protected override bool OnClick(Framework.Input.Events.ClickEvent e)
        {
            Value.Toggle();
            return base.OnClick(e);
        }
    }
}