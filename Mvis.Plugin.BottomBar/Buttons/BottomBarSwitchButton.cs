using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Screens.LLin.Plugins.Types;
using osuTK.Graphics;

namespace Mvis.Plugin.BottomBar.Buttons
{
    public class BottomBarSwitchButton : BottomBarButton
    {
        public BindableBool Value = new BindableBool();

        public bool Default { get; set; }

        protected Color4 ActivateColor => ColourProvider.Highlight1;
        protected Color4 InActivateColor => ColourProvider.Background3;

        public BottomBarSwitchButton(IToggleableFunctionProvider provider)
            : base(provider)
        {
            Value.Value = Default;

            Value.BindTo(provider.Bindable);
        }

        protected override void LoadComplete()
        {
            Value.BindValueChanged(_ => updateVisuals(true), true);
            Value.BindDisabledChanged(onDisabledChanged, true);

            ColourProvider.HueColour.BindValueChanged(_ => updateVisuals());

            base.LoadComplete();
        }

        private void onDisabledChanged(bool disabled)
        {
            this.FadeColour(disabled ? Color4.Gray : Color4.White, 300, Easing.OutQuint);
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (Value.Disabled)
            {
                this.FlashColour(Color4.Red, 1000, Easing.OutQuint);
                return false;
            }

            return base.OnClick(e);
        }

        private void updateVisuals(bool animate = false)
        {
            var duration = animate ? 500 : 0;

            switch (Value.Value)
            {
                case true:
                    BgBox.FadeColour(ActivateColor, duration, Easing.OutQuint);
                    ContentFillFlow.FadeColour(Colour4.Black, duration, Easing.OutQuint);
                    if (animate)
                        OnToggledOnAnimation();
                    break;

                case false:
                    BgBox.FadeColour(InActivateColor, duration, Easing.OutQuint);
                    ContentFillFlow.FadeColour(Colour4.White, duration, Easing.OutQuint);
                    break;
            }
        }

        protected virtual void OnToggledOnAnimation()
        {
        }
    }
}
