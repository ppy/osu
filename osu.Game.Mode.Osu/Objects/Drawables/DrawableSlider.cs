using osu.Framework.Graphics;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Osu.Objects.Drawables.Pieces;
using OpenTK;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Modes.Osu.Objects.Drawables
{
    class DrawableSlider : DrawableOsuHitObject
    {
        public DrawableSlider(Slider s) : base(s)
        {
            Origin = Anchor.Centre;
            Position = new Vector2(s.Position.X, s.Position.Y);

            Path sliderPath;
            Add(sliderPath = new Path());

            for (int i = 0; i < s.Curve.Path.Count; ++i)
                sliderPath.Positions.Add(s.Curve.Path[i] - s.Position);

            Add(new DrawableHitCircle(new HitCircle
            {
                StartTime = s.StartTime,
                Position = sliderPath.Positions[0] - s.Position,
            })
            {
                Depth = 1
            });
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
