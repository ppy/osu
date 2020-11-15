using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Screens.Mvis.Modules;
using osu.Game.Skinning;

namespace osu.Game.Screens.Mvis.SideBar.Footer
{
    public class Footer : Container
    {
        private readonly Box bgBox;

        [Resolved]
        private CustomColourProvider colourProvider { get; set; }

        public Footer()
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
                bgBox = new Box
                {
                    RelativeSizeAxes = Axes.Both
                },
                new SkinnableSprite("MSidebar-BottomBox", confineMode: ConfineMode.ScaleToFill)
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

        protected override void LoadComplete()
        {
            colourProvider.HueColour.BindValueChanged(_ =>
            {
                bgBox.Colour = colourProvider.Background5;
            }, true);

            base.LoadComplete();
        }
    }
}
