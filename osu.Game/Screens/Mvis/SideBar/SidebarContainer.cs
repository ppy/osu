using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
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
        private ISidebarContent currentDisplay;
        private SampleChannel popInSample;
        private SampleChannel popOutSample;

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

        [BackgroundDependencyLoader]
        private void load(AudioManager audioManager)
        {
            popInSample = audioManager.Samples.Get(@"UI/overlay-pop-in");
            popOutSample = audioManager.Samples.Get(@"UI/overlay-pop-out");
        }

        public void resizeFor(Drawable d)
        {
            if ( ! (d is ISidebarContent isc) || !components.Contains(d) ) return;

            var c = d as ISidebarContent;
            Show();

            //如果要显示的是当前正在显示的内容，则中断
            if ( currentDisplay == c )
            {
                IsHidden = false;
                return;
            }

            currentDisplay = c;
            var duration = IsHidden ? 0 : DURATION;

            foreach (var item in components)
                item.FadeOut(duration / 2, Easing.OutQuint);

            d.Delay(duration / 2).FadeIn(duration / 2);

            this.ResizeTo(new Vector2(c.ResizeWidth, c.ResizeHeight), duration, Easing.OutQuint);
            IsHidden = false;
        }

        public void AddDrawableToList(Drawable d) =>
            components.Add(d);

        protected override void PopOut()
        {
            base.PopOut();
            this.FadeEdgeEffectTo(0, DISAPPEAR_DURATION).OnComplete(_ => IsHidden = true);
            popOutSample.Play();
        }

        protected override void PopIn()
        {
            base.PopIn();
            this.FadeEdgeEffectTo(1f);
            popInSample.Play();
        }
    }
}