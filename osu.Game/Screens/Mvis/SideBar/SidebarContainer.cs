using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Screens.Mvis.Modules;
using osuTK;

namespace osu.Game.Screens.Mvis.SideBar
{
    public class SidebarContainer : WaveContainer
    {
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue1);
        private List<Drawable> components = new List<Drawable>();
        public bool IsHidden = true;
        private readonly float DURATION = 400;
        private Drawable currentDisplay;
        public SidebarContainer()
        {
            //与其他Overlay保持一致
            FirstWaveColour = colourProvider.Light4;
            SecondWaveColour = colourProvider.Light3;
            ThirdWaveColour = colourProvider.Dark4;
            FourthWaveColour = colourProvider.Dark3;

            Anchor = Anchor.BottomRight;
            Origin = Anchor.BottomRight;
            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(0.3f, 1f);
            Depth = -float.MaxValue;
        }

        public void resizeFor(Drawable d)
        {
            if ( ! (d is ISidebarContent isc) || !components.Contains(d) ) return;

            var c = d as ISidebarContent;
            Show();

            //如果要显示的是当前正在显示的内容，则中断
            if ( currentDisplay == d )
            {
                IsHidden = false;
                return;
            }

            var duration = IsHidden ? 0 : DURATION;

            currentDisplay?.FadeOut(duration / 2, Easing.OutQuint);

            currentDisplay = d;

            d.Delay(duration / 2).FadeIn(duration / 2);

            this.ResizeTo(new Vector2(c.ResizeWidth, c.ResizeHeight), duration, Easing.OutQuint);
            IsHidden = false;
        }

        public void AddDrawableToList(Drawable d)
        {
            d.Alpha = 0;
            components.Add(d);
        }

        protected override void PopOut()
        {
            base.PopOut();
            this.FadeEdgeEffectTo(0, DISAPPEAR_DURATION).OnComplete(_ => IsHidden = true);
        }

        protected override void PopIn()
        {
            base.PopIn();
            this.FadeEdgeEffectTo(1f);
        }
    }
}