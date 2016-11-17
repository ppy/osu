using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Osu.Objects.Drawables.Pieces;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Modes.Osu.Objects.Drawables
{
    class DrawableSlider : DrawableHitObject
    {
        public DrawableSlider(Slider h) : base(h)
        {
            Origin = Anchor.Centre;
            RelativePositionAxes = Axes.Both;
            Position = new Vector2(h.Position.X / 512, h.Position.Y / 384);

            for (float i = 0; i <= 1; i += 0.1f)
            {
                Add(new CirclePiece
                {
                    Colour = h.Colour,
                    Hit = Hit,
                    Position = h.Curve.PositionAt(i) - h.Position //non-relative?
                });
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            //force application of the state that was set before we loaded.
            UpdateState(State);
        }

        protected override void UpdateState(ArmedState state)
        {
            if (!IsLoaded) return;

            Flush(true); //move to DrawableHitObject

            Alpha = 0;

            Delay(HitObject.StartTime - 200 - Time.Current, true);

            FadeIn(200);
            Delay(200 + HitObject.Duration);
            FadeOut(200);
        }
    }
}
