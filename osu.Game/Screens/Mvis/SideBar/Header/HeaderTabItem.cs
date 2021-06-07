using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Mvis.Skinning;
using osuTK;
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

        private Color4 activeColor => colourProvider.Highlight1;
        private Color4 inActiveColor => colourProvider.Dark4;

        public HeaderTabItem(ISidebarContent content)
        {
            AutoSizeAxes = Axes.X;
            Height = 40;
            Anchor = Anchor.TopRight;
            Origin = Anchor.TopRight;
            Value = content;
            Masking = true;
            CornerRadius = 5;
            Margin = new MarginPadding { Vertical = 10, Horizontal = 5 };
            Children = new Drawable[]
            {
                activeBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft
                },
                title = new OsuSpriteText
                {
                    Text = content.Title,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Margin = new MarginPadding { Horizontal = 5 }
                },
                new PlaceHolder
                {
                    Size = new Vector2(40)
                }
            };
        }

        protected override void LoadComplete()
        {
            colourProvider.HueColour.BindValueChanged(_ =>
            {
                activeBox.Colour = isActive ? activeColor : inActiveColor;
                title.Colour = isActive ? Color4.Black : activeColor;//
            }, true);

            base.LoadComplete();
        }

        public void MakeActive()
        {
            isActive = true;
            activeBox.FadeColour(activeColor, 300, Easing.OutQuint);
            title.Colour = Color4.Black;
        }

        public void MakeInActive()
        {
            isActive = false;
            activeBox.FadeColour(inActiveColor, 300, Easing.OutQuint);
            title.Colour = Color4.White;
            title.Colour = colourProvider.Highlight1;
        }
    }
}
