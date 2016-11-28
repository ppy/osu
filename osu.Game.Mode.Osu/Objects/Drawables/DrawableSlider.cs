using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Osu.Objects.Drawables.Pieces;
using OpenTK;
using osu.Framework.Graphics.Sprites;
using OpenTK.Graphics;

namespace osu.Game.Modes.Osu.Objects.Drawables
{
    class DrawableSlider : DrawableOsuHitObject
    {
        private Path path;
        private DrawableHitCircle startCircle;
        private Slider slider;

        public DrawableSlider(Slider s) : base(s)
        {
            slider = s;

            Origin = Anchor.TopLeft;
            Position = Vector2.Zero;

            Children = new Drawable[]
            {
                startCircle = new DrawableHitCircle(new HitCircle
                {
                    StartTime = s.StartTime,
                    Position = s.Position,
                    Colour = s.Colour,
                })
                {
                    Depth = 1 //override time-based depth.
                },
                path = new Path
                {
                    Colour = s.Colour,
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            for (int i = 0; i < slider.Curve.Path.Count; ++i)
                path.Positions.Add(slider.Curve.Path[i]);

            path.PathWidth = startCircle.DrawWidth / 4;

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
