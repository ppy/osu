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

            foreach (var item in components)
            {
                item.FadeOut(300);
            }

            d.FadeIn(300);

            this.ResizeTo(new Vector2(c.ResizeWidth, c.ResizeHeight), 600, Easing.OutQuint);
        }

        public void AddDrawableToList(Drawable d) =>
            components.Add(d);

        protected override void PopOut()
        {
            base.PopOut();
            IsHidden = true;
        }

        protected override void PopIn()
        {
            base.PopIn();
            IsHidden = false;
        }
    }
}