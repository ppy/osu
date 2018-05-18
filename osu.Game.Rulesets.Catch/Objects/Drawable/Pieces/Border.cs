using osu.Framework.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Catch.Objects.Drawable.Pieces
{
    public class Border : Circle
    {
        public Border(float GlowRadius, Vector2 size, float borderthickness, Color4 AccentColour, bool HyperDash)
        {
            EdgeEffect = new EdgeEffectParameters
            {
                Hollow = !HyperDash,
                Type = EdgeEffectType.Glow,
                Radius = GlowRadius,
                Colour = HyperDash ? Color4.Red : AccentColour.Darken(1).Opacity(0.6f)
            };
            Size = size;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            BorderColour = Color4.White;
            BorderThickness = borderthickness;
            Children = new Framework.Graphics.Drawable[]
            {
                new Box
                {
                    AlwaysPresent = true,
                    Colour = AccentColour,
                    Alpha = 0,
                    RelativeSizeAxes = Axes.Both
                }
            };
        }
        
    }
}
