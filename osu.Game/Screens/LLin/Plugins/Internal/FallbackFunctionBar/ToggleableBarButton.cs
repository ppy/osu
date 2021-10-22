using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Screens.LLin.Plugins.Types;
using osuTK.Graphics;

namespace osu.Game.Screens.LLin.Plugins.Internal.FallbackFunctionBar
{
    public class ToggleableBarButton : SimpleBarButton
    {
        private Box indicator;

        public BindableBool Value = new BindableBool();

        public ToggleableBarButton(IToggleableFunctionProvider provider)
            : base(provider)
        {
            Value.BindTo(provider.Bindable);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(indicator = new Box
            {
                Height = 5,
                RelativeSizeAxes = Axes.X
            });
        }

        protected override void LoadComplete()
        {
            Value.BindValueChanged(onValueChanged, true);
            Value.BindDisabledChanged(onDisableChanged, true);
        }

        private void onDisableChanged(bool value)
        {
            this.FadeColour(value ? Color4.Gray : Color4.White, 300, Easing.OutQuint);
        }

        private void onValueChanged(ValueChangedEvent<bool> v)
        {
            indicator.FadeColour(v.NewValue ? Color4.Green : Color4.Gold, 300, Easing.OutQuint);
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
    }
}
