using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Mvis.Modules;

namespace osu.Game.Screens.Mvis.SideBar.Header
{
    public class HeaderTabItem : OsuClickableContainer
    {
        private readonly Box activeBox;
        private readonly Box hoverBox;
        private bool isActive;

        public ISidebarContent Value;

        [Resolved]
        private CustomColourProvider colourProvider { get; set; }

        public HeaderTabItem(ISidebarContent content)
        {
            AutoSizeAxes = Axes.X;
            RelativeSizeAxes = Axes.Y;
            Anchor = Anchor.TopRight;
            Origin = Anchor.TopRight;
            Value = content;
            Children = new Drawable[]
            {
                hoverBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0
                },
                activeBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Height = 0,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft
                },
                new OsuSpriteText
                {
                    Text = content.Title,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Margin = new MarginPadding { Horizontal = 10 }
                }
            };
        }

        protected override void LoadComplete()
        {
            colourProvider.HueColour.BindValueChanged(_ =>
            {
                activeBox.Colour = hoverBox.Colour = colourProvider.Highlight1;
            }, true);

            base.LoadComplete();
        }

        protected override bool OnHover(HoverEvent e)
        {
            if (!isActive)
                activeBox.ResizeHeightTo(0.2f, 300, Easing.OutElastic);

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            if (!isActive)
                activeBox.ResizeHeightTo(0, 300, Easing.OutQuint);

            base.OnHoverLost(e);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            hoverBox.FadeTo(0.6f, 2000, Easing.OutQuint);
            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            hoverBox.FadeOut(1000, Easing.OutQuint);
            base.OnMouseUp(e);
        }

        public void MakeActive()
        {
            isActive = true;
            activeBox.ResizeHeightTo(0.2f, 300, Easing.OutQuint);
        }

        public void MakeInActive()
        {
            isActive = false;
            activeBox.ResizeHeightTo(IsHovered ? 0.2f : 0, 300, Easing.OutQuint);
        }
    }
}
