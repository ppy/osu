using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Rendering;
using osu.Game.Configuration;

#nullable disable

namespace osu.Game.Graphics.Backgrounds
{
    public partial class PureColorBackground : Background
    {
        private Bindable<string> colorHex;

        [Resolved]
        private MConfigManager config { get; set; }

        private bool isFirstShow;

        public PureColorBackground(bool isFirstShow)
        {
            this.isFirstShow = isFirstShow;
        }

        [BackgroundDependencyLoader]
        private void load(IRenderer renderer)
        {
            Sprite.Texture = renderer.WhitePixel;
            Sprite.Colour = config.GetCustomLoaderColor();

            colorHex = config.GetBindable<string>(MSetting.LoaderBackgroundColor);
        }

        protected override void LoadComplete()
        {
            colorHex.BindValueChanged(_ =>
            {
                Sprite.FadeColour(config.GetCustomLoaderColor(), 300);
            });

            if (isFirstShow) this.Delay(300).Then().FadeOut(300, Easing.OutQuint).Then().Expire();
            base.LoadComplete();
        }
    }
}
