using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.Mvis.Modules;
using osuTK;

namespace osu.Game.Screens.Mvis.SideBar
{
    public class SidebarContainer : VisibilityContainer
    {
        [Resolved]
        private CustomColourProvider colourProvider { get; set; }

        private readonly List<Drawable> components = new List<Drawable>();
        public bool IsHidden = true;
        private const float duration = 400;
        private Drawable currentDisplay;

        private readonly WaveContainer waveContainer;
        protected override Container<Drawable> Content => waveContainer;

        public SidebarContainer()
        {
            Anchor = Anchor.BottomRight;
            Origin = Anchor.BottomRight;
            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(0.3f, 1f);
            Depth = -float.MaxValue;

            AddInternal(waveContainer = new WaveContainer
            {
                RelativeSizeAxes = Axes.Both,
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            colourProvider.HueColour.BindValueChanged(_ => updateWaves(), true);
        }

        private void updateWaves()
        {
            //与其他Overlay保持一致
            waveContainer.FirstWaveColour = colourProvider.Light4;
            waveContainer.SecondWaveColour = colourProvider.Light3;
            waveContainer.ThirdWaveColour = colourProvider.Dark4;
            waveContainer.FourthWaveColour = colourProvider.Dark3;
        }

        public void ResizeFor(Drawable d)
        {
            if (!(d is ISidebarContent) || !components.Contains(d)) return;

            var c = (ISidebarContent)d;
            Show();

            //如果要显示的是当前正在显示的内容，则中断
            if (currentDisplay == d)
            {
                IsHidden = false;
                return;
            }

            var resizeDuration = IsHidden ? 0 : SidebarContainer.duration;

            currentDisplay?.FadeOut(resizeDuration / 2, Easing.OutQuint);

            currentDisplay = d;

            d.Delay(resizeDuration / 2).FadeIn(resizeDuration / 2);

            this.ResizeTo(new Vector2(c.ResizeWidth, c.ResizeHeight), resizeDuration, Easing.OutQuint);
            IsHidden = false;
        }

        public void AddDrawableToList(Drawable d)
        {
            d.Alpha = 0;
            components.Add(d);
        }

        protected override void PopOut()
        {
            waveContainer.Hide();
            this.FadeOut(WaveContainer.DISAPPEAR_DURATION, Easing.InExpo).OnComplete(_ => IsHidden = true);
        }

        protected override void PopIn()
        {
            waveContainer.Show();
            this.FadeIn(200);
        }
    }
}
