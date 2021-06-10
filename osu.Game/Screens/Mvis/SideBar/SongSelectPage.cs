using System;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Mvis.SideBar
{
    public class SongSelectPage : CompositeDrawable, ISidebarContent
    {
        public string Title => "歌曲选择";
        public IconUsage Icon => FontAwesome.Solid.MousePointer;

        public SongSelectPage()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Colour = Color4.White.Opacity(0.9f),
                Margin = new MarginPadding(40),
                Children = new Drawable[]
                {
                    new SpriteIcon
                    {
                        Icon = FontAwesome.Solid.MousePointer,
                        Size = new Vector2(60),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                    new OsuSpriteText
                    {
                        Text = "点任意处前往歌曲选择",
                        Font = OsuFont.GetFont(size: 45, weight: FontWeight.Bold),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                }
            };
        }

        public Action Action;

        protected override bool OnClick(ClickEvent e)
        {
            Action?.Invoke();
            return base.OnClick(e);
        }
    }
}
