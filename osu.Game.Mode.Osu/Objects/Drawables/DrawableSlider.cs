using osu.Framework.Graphics;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Osu.Objects.Drawables.Pieces;
using OpenTK;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Modes.Osu.Objects.Drawables
{
    class DrawableSlider : DrawableOsuHitObject
    {
        public DrawableSlider(Slider h) : base(h)
        {
            Origin = Anchor.Centre;
            Position = new Vector2(h.Position.X, h.Position.Y);

            Path sliderPath;
            Add(sliderPath = new Path());

            for (int i = 0; i < h.Curve.Path.Count; ++i)
                sliderPath.Positions.Add(h.Curve.Path[i] - h.Position);

            h.Position = Vector2.Zero;
            Add(new DrawableHitCircle(h));
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

            Delay(HitObject.StartTime - 450 - Time.Current, true);

            FadeIn(200);
            Delay(450 + HitObject.Duration);
            FadeOut(200);
        }
    }
}
