using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Configuration;
using osu.Game.Screens.Menu;
using osuTK.Graphics;

namespace osu.Game.Screens.Backgrounds
{
    public partial class BackgroundScreenPureColor : BackgroundScreen
    {
        public readonly Color4 BackgroundColor;
        private readonly Box box;

        [Resolved]
        private OsuConfigManager config { get; set; }

        [Resolved]
        private MConfigManager mConfig { get; set; }

        public BackgroundScreenPureColor(Color4 color = new Color4())
        {
            InternalChild = box = new Box
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

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            //如果：
            //背景源不是加载页背景
            //下一个屏幕不是IntroSkipped
            //则淡出此屏幕, 因为我希望屏幕能在Intro为跳过时，背景可以无缝切换到BackgroundScreenDefault, 其他时间则正常淡出
            if (config.Get<BackgroundSource>(OsuSetting.MenuBackgroundSource) != BackgroundSource.LoaderBackground
                || !(e.Next is IntroSkipped))
                this.FadeOut(500, Easing.OutExpo);
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            //如果：
            //背景源是加载页背景
            //则显示此屏幕
            //否则, 淡出此屏幕, 因为我希望该背景只会在源为加载页时显示在outro上
            if (config.Get<BackgroundSource>(OsuSetting.MenuBackgroundSource) == BackgroundSource.LoaderBackground)
            {
                box.Colour = mConfig.GetCustomLoaderColor();
                this.FadeIn();
            }
            else
                this.FadeOut();
        }
    }
}
