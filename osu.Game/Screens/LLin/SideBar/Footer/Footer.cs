using System;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Screens.LLin.SideBar.Footer
{
    public class Footer : CompositeDrawable
    {
        [CanBeNull]
        private Box bgBox;

        [CanBeNull]
        private Box highLightBox;

        [Resolved]
        private CustomColourProvider colourProvider { get; set; }

        [Resolved(CanBeNull = true)]
        private IImplementLLin mvisScreen { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            Height = 50;
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;
            Masking = true;
        }

        private Drawable createDefaultFooter()
        {
            bgBox = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = colourProvider.Background5
            };

            highLightBox = new Box
            {
                RelativeSizeAxes = Axes.X,
                Height = 3,
                Colour = colourProvider.Highlight1
            };

            var c = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    bgBox,
                    highLightBox
                }
            };

            return c;
        }

        protected override void LoadComplete()
        {
            colourProvider.HueColour.BindValueChanged(_ =>
            {
                bgBox?.FadeColour(colourProvider.Dark5);
                highLightBox?.FadeColour(colourProvider.Highlight1);
            }, true);

            base.LoadComplete();
        }

        protected override void Update()
        {
            Height = Math.Max((int)(mvisScreen?.BottomBarHeight ?? 0), 10) + 5;
            base.Update();
        }
    }
}
