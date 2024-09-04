using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.UI.ReplayAnalysis
{
    public partial class HitMarkerClick : HitMarker
    {
        public HitMarkerClick()
        {
            InternalChildren = new Drawable[]
            {
                new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(15),
                    Masking = true,
                    BorderThickness = 2,
                    BorderColour = Color4.White,
                    Child = new Box
                    {
                        Colour = Color4.Black,
                        RelativeSizeAxes = Axes.Both,
                        AlwaysPresent = true,
                        Alpha = 0,
                    },
                },
            };
        }
    }
}
