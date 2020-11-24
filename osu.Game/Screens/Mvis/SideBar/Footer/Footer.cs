using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Screens.Mvis.Skinning;
using osu.Game.Skinning;

namespace osu.Game.Screens.Mvis.SideBar.Footer
{
    public class Footer : CompositeDrawable
    {
        [CanBeNull]
        private Box bgBox;

        [Resolved]
        private CustomColourProvider colourProvider { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            Height = 50;
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;
            Masking = true;
            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Colour = Colour4.Black.Opacity(0.6f),
                Radius = 5,
            };

            InternalChildren = new Drawable[]
            {
                new SkinnableComponent(
                    "MSidebar-BottomBox",
                    confineMode: ConfineMode.ScaleToFill,
                    masking: true,
                    defaultImplementation: _ => createFooterBox())
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

        private Box createFooterBox()
        {
            bgBox = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = colourProvider.Background5
            };

            return bgBox;
        }

        protected override void LoadComplete()
        {
            colourProvider.HueColour.BindValueChanged(_ =>
            {
                bgBox?.FadeColour(colourProvider.Background5);
            }, true);

            base.LoadComplete();
        }
    }
}
