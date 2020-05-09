using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Screens.Mvis.UI
{
    public class BottomBarSongProgressInfo : FillFlowContainer
    {
        public OsuSpriteText timeCurrent;
        public OsuSpriteText timeTotal;

        public BottomBarSongProgressInfo()
        {
            Spacing = new Vector2(5);
            Children = new Drawable[]
            {
                timeCurrent = new OsuSpriteText
                {
                    Origin = Anchor.CentreLeft,
                    Anchor = Anchor.CentreLeft,
                },
                new OsuSpriteText
                {
                    Origin = Anchor.CentreLeft,
                    Anchor = Anchor.CentreLeft,
                    Text = "/"
                },
                timeTotal = new OsuSpriteText
                {
                    Origin = Anchor.CentreLeft,
                    Anchor = Anchor.CentreLeft,
                }
            };
        }
    }
}