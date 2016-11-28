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
        private Container ball;

        public DrawableSlider(Slider s) : base(s)
        {
            slider = s;

            Origin = Anchor.TopLeft;
            Position = Vector2.Zero;

            Children = new Drawable[]
            {
                path = new Path
                {
                    Colour = s.Colour,
                },
                ball = new Container
                {
                    Masking = true,
                    CornerRadius = 20,
                    AutoSizeAxes = Axes.Both,
                    Colour = Color4.Red,
                    Origin = Anchor.Centre,
                    Children = new []
                    {
                        new Box
                        {
                            Width = 40,
                            Height = 40,
                        }
                    }
                },
                startCircle = new DrawableHitCircle(new HitCircle
                {
                    StartTime = s.StartTime,
                    Position = s.Position,
                    Colour = s.Colour,
                })
                {
                    Depth = 1 //override time-based depth.
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            for (int i = 0; i < slider.Curve.Length; i += 10)
                path.Positions.Add(slider.Curve.PositionAt(i / slider.Curve.Length));

            path.PathWidth = startCircle.DrawWidth / 4;

            //force application of the state that was set before we loaded.
            UpdateState(State);
        }

        protected override void Update()
        {
            base.Update();

            ball.Alpha = Time.Current >= slider.StartTime && Time.Current <= slider.EndTime ? 1 : 0;

            double t = (Time.Current - slider.StartTime) / slider.Duration;
            if (slider.RepeatCount > 1)
            {
                double currentRepeat = (int)(t * slider.RepeatCount);
                t = (t * slider.RepeatCount) % 1;
                if (currentRepeat % 2 == 1)
                    t = 1 - t;
            }
                
            ball.Position = slider.Curve.PositionAt(t);

        }

        protected override void UpdateState(ArmedState state)
        {
            if (!IsLoaded) return;

            Flush(true); //move to DrawableHitObject

            Alpha = 0;

            Delay(HitObject.StartTime - 450 - Time.Current, true);

            FadeIn(200);
            Delay(450 + HitObject.Duration);
            FadeOut(100);
        }
    }
}
