using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Configuration;
using osuTK.Graphics;

namespace osu.Game.Screens.Backgrounds
{
    public class BackgroundScreenPureColor : BackgroundScreen
    {
        public readonly Color4 BackgroundColor;

        [Resolved]
        private OsuConfigManager config { get; set; }

        public BackgroundScreenPureColor(Color4 color = new Color4())
        {
            InternalChild = new Box
            {
                Colour = color,
                RelativeSizeAxes = Axes.Both,
            };

            BackgroundColor = color;
        }

        public override bool Equals(BackgroundScreen other)
        {
            if (!(other is BackgroundScreenPureColor otherBeatmapBackground)) return false;

            return BackgroundColor == otherBeatmapBackground.BackgroundColor;
        }

        public override void OnSuspending(IScreen next)
        {
            if (config.Get<BackgroundSource>(OsuSetting.MenuBackgroundSource) != BackgroundSource.LoaderBackground)
                this.FadeOut(500, Easing.OutExpo);
        }

        public override void OnResuming(IScreen last)
        {
            if (config.Get<BackgroundSource>(OsuSetting.MenuBackgroundSource) == BackgroundSource.LoaderBackground)
                this.FadeIn();
            else
                this.FadeOut();
        }
    }
}
