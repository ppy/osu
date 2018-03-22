using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;

namespace osu.Game.Rulesets.Vitaru.Objects.Drawables.Pieces
{
    public class SeekingBulletPiece : BeatSyncedContainer
    {
        public SeekingBulletPiece(DrawableSeekingBullet seekingBullet)
        {
            Masking = true;
            RelativeSizeAxes = Axes.Both;
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            BorderThickness = 4;
            AlwaysPresent = true;
            BorderColour = seekingBullet.AccentColour;
            CornerRadius = 4;

            Child = new Box
            {
                RelativeSizeAxes = Axes.Both
            };
            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Radius = 8,
                Colour = seekingBullet.AccentColour.Opacity(0.25f),
            };
        }

        protected override void Update()
        {
            base.Update();

            this.RotateTo((float)(Clock.CurrentTime / 1000 * 90) * 2);
        }
    }
}
