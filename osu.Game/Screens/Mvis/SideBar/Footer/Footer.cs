using System;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Screens.Mvis.Skinning;
using osu.Game.Skinning;

namespace osu.Game.Screens.Mvis.SideBar.Footer
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
        private MvisScreen mvisScreen { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            Height = 50;
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;
            Masking = true;

            InternalChildren = new Drawable[]
            {
                new SkinnableComponent(
                    "MSidebar-BottomBox",
                    confineMode: ConfineMode.ScaleToFill,
                    defaultImplementation: _ => createDefaultFooter())
                {
                    Name = "侧边栏底部横条",
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    ChildAnchor = Anchor.BottomRight,
                    ChildOrigin = Anchor.BottomRight,
                    RelativeSizeAxes = Axes.Both,
                    CentreComponent = false,
                    OverrideChildAnchor = true,
                }
            };
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
                bgBox?.FadeColour(colourProvider.Background5);
                highLightBox?.FadeColour(colourProvider.Highlight1);
            }, true);

            base.LoadComplete();
        }

        protected override void Update()
        {
            Height = Math.Max(mvisScreen?.BottombarHeight ?? 0, 10);
            base.Update();
        }
    }
}
