using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK.Graphics;

namespace osu.Game.Screens.Mvis.SideBar.Header
{
    public class HeaderTabItem : OsuClickableContainer
    {
        private readonly Box activeBox;
        private readonly OsuSpriteText title;
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
                activeBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Height = 0,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft
                },
                title = new OsuSpriteText
                {
                    Text = content.Title,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Margin = new MarginPadding { Horizontal = 5 }
                }
            };
        }

        protected override void LoadComplete()
        {
            colourProvider.HueColour.BindValueChanged(_ =>
            {
                activeBox.Colour = colourProvider.Highlight1;
                title.Colour = isActive ? Color4.White : colourProvider.Highlight1;
            }, true);

            base.LoadComplete();
        }

        protected override bool OnHover(HoverEvent e)
        {
            if (!isActive)
                activeBox.ResizeHeightTo(0.15f, 300, Easing.OutQuint);

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            if (!isActive)
                activeBox.ResizeHeightTo(0, 300, Easing.OutQuint);

            base.OnHoverLost(e);
        }

        public void MakeActive()
        {
            isActive = true;
            activeBox.ResizeHeightTo(0.2f, 300, Easing.OutQuint);
            title.Colour = Color4.White;
        }

        public void MakeInActive()
        {
            isActive = false;
            activeBox.ResizeHeightTo(IsHovered ? 0.15f : 0, 300, Easing.OutQuint);
            title.Colour = colourProvider.Highlight1;
        }
    }
}
