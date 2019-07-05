using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.OSD
{
    public class OsdIconToast : OsdToast
    {
        public OsdIconToast(string message, IconUsage icon)
        {
            Children = new Drawable[]
            {
                new FillFlowContainer()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Direction = FillDirection.Vertical,
                    Spacing = new osuTK.Vector2(10),
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.BottomCentre,
                            Font = OsuFont.GetFont(size: 24, weight: FontWeight.Light),
                            Text = message
                        },
                        new SpriteIcon
                        {
                            Icon = icon,
                            Size = new osuTK.Vector2(45),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.BottomCentre,
                        }
                    }
                }
            };
        }
    }
}
