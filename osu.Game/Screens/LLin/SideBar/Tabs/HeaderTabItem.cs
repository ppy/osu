using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.LLin.SideBar.Tabs
{
    internal class TabControlItem : OsuClickableContainer
    {
        private readonly Box activeBox;
        private readonly SpriteIcon icon;
        private bool isActive;

        public ISidebarContent Value;
        private readonly Box flashBox;

        [Resolved]
        private CustomColourProvider colourProvider { get; set; }

        private Color4 activeColor => colourProvider.Highlight1;
        private Color4 inActiveColor => colourProvider.Dark4.Opacity(0);

        public TabControlItem(ISidebarContent content)
        {
            Value = content;
            TooltipText = Value.Title;

            Size = new Vector2(45);
            Anchor = Anchor.TopRight;
            Origin = Anchor.TopRight;
            Masking = true;
            CornerRadius = 5;
            Children = new Drawable[]
            {
                activeBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft
                },
                icon = new SpriteIcon
                {
                    Icon = content.Icon,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(18),
                    Shadow = true
                },
                flashBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.White,
                    Alpha = 0,
                }
            };
        }

        protected override void LoadComplete()
        {
            colourProvider.HueColour.BindValueChanged(_ =>
            {
                activeBox.Colour = isActive ? activeColor : inActiveColor;
                icon.Colour = isActive ? Color4.Black : Color4.White;
            }, true);

            base.LoadComplete();
        }

        protected override bool OnHover(HoverEvent e)
        {
            flashBox.FadeTo(0.2f, 300);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            flashBox.FadeTo(0f, 300);
        }

        public void MakeActive()
        {
            isActive = true;
            activeBox.FadeColour(activeColor, 300, Easing.OutQuint);
            icon.Colour = Color4.Black;
        }

        public void MakeInActive()
        {
            isActive = false;
            activeBox.FadeColour(inActiveColor, 300, Easing.OutQuint);
            icon.Colour = Color4.White;
        }
    }
}
