using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.UI.ReplayAnalysis
{
    public partial class HitMarkerRightClick : HitMarker
    {
        public HitMarkerRightClick()
        {
            const float length = 20;

            Colour = Color4.GreenYellow;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(3, length),
                    Colour = Colour4.Black.Opacity(0.5F)
                },
                new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(3, length),
                    Rotation = 90,
                    Colour = Colour4.Black.Opacity(0.5F)
                },
                new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(1, length),
                },
                new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(1, length),
                    Rotation = 90,
                }
            };
        }
    }
}
